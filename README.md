# Personal Finance Tracker

A private spending tracker for recording expenses, organizing them by category, and reviewing monthly totals.

## Features

- Dashboard with this month’s spending, transaction count, previous-month comparison, category breakdown, and recent expenses.
- Expense entry with date, amount, category, and optional note.
- Search and filter the expense journal by month and category.
- SQLite persistence, with generated sample expenses for the current and two previous months on first startup.
- Sample rows are marked in the UI. The sample set is created only when the expense table is empty.

Amounts are displayed in Indonesian rupiah (IDR). No bank, GoPay, or OVO connection is active; the application service boundary is ready for future import providers.

## Run locally

```powershell
dotnet run --project src/PersonalFinanceTracker.Web --urls http://localhost:5007
```

Open `http://localhost:5007`. The SQLite database is stored at `src/PersonalFinanceTracker.Web/Data/app.db` and is excluded from Git.

## Phone and PWA

The app includes a web app manifest, home-screen icons, and an offline page. Expense screens and records are always requested from the server; only static front-end assets are cached. The app needs the computer and its database to be online to use expenses.

To open the development server from a phone on the same Wi-Fi, bind it to the computer's Wi-Fi IPv4 address as well as localhost:

```powershell
dotnet run --project src/PersonalFinanceTracker.Web --urls "http://localhost:5007;http://<computer-wifi-ip>:5007"
```

Then open `http://<computer-wifi-ip>:5007` on the phone. The Wi-Fi address can change. Chrome requires HTTPS for service workers and home-screen PWA installation on a phone, so the development HTTP address is for responsive browsing; use an HTTPS host to install it.

## Architecture

The solution follows Clean Architecture; project references enforce the dependency direction:

```text
PersonalFinanceTracker.Domain
        ↑
PersonalFinanceTracker.Application
        ↑
PersonalFinanceTracker.Infrastructure
        ↑
PersonalFinanceTracker.Web
```

- **Domain** contains expense and category entities and their invariants.
- **Application** owns expense use cases, DTOs, and repository ports.
- **Infrastructure** implements SQLite repositories, schema migrations, and first-run demo data.
- **Web** contains the Blazor screens and selects implementations in the composition root.

## SOLID

- Keep UI components focused on presentation and user interaction.
- Put use cases behind focused application services.
- Keep abstractions narrow and owned by the layer that needs them.
- Make Infrastructure implement application-owned ports, with Web selecting those implementations at startup.
