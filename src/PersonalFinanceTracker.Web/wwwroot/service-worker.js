const CACHE_NAME = "finance-tracker-static-v1";
const OFFLINE_PAGE = "/offline.html";

self.addEventListener("install", event => {
    event.waitUntil(
        caches.open(CACHE_NAME)
            .then(cache => cache.add(OFFLINE_PAGE))
            .then(() => self.skipWaiting())
    );
});

self.addEventListener("activate", event => {
    event.waitUntil(
        caches.keys()
            .then(keys => Promise.all(keys.filter(key => key.startsWith("finance-tracker-static-") && key !== CACHE_NAME)
                .map(key => caches.delete(key))))
            .then(() => self.clients.claim())
    );
});

self.addEventListener("fetch", event => {
    const request = event.request;
    const url = new URL(request.url);

    if (request.method !== "GET" || url.origin !== self.location.origin) {
        return;
    }

    if (request.mode === "navigate") {
        // Expense pages are always fetched from the server and are never stored offline.
        event.respondWith(fetch(request).catch(() => caches.match(OFFLINE_PAGE)));
        return;
    }

    // Keep only static front-end resources in cache; ignore Blazor circuits and application data.
    const isStaticAsset = url.pathname.startsWith("/_framework/")
        || url.pathname.startsWith("/icons/")
        || /\.(?:css|js|png|svg|ico|webmanifest|woff2?)$/i.test(url.pathname);

    if (!isStaticAsset || url.pathname.startsWith("/_blazor/")) {
        return;
    }

    event.respondWith(
        fetch(request).then(response => {
            if (response.ok && response.type === "basic") {
                const copy = response.clone();
                caches.open(CACHE_NAME).then(cache => cache.put(request, copy));
            }
            return response;
        }).catch(async () => {
            const cached = await caches.match(request);
            return cached || Response.error();
        })
    );
});
