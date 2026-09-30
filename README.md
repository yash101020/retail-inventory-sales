# Retail Inventory & Sales

A Windows desktop application for managing products, stock, sales, and transaction history.

## Features

- Add, edit, view, and delete products
- Create sales with multiple products
- Automatically reduce stock after a successful sale
- Adjust stock quantities
- View sales history and receipt details
- Display low-stock alerts for products with 5 units or fewer

## Technologies

C#, .NET 10, Windows Forms, Entity Framework Core, and SQLite.

## How to Run

### Requirements

- Windows
- .NET 10 SDK
- Internet access for the first package restore

Download and extract the repository, then open the `RetailInventoryApp` folder and double-click `Run.cmd`.

Alternatively, open a terminal in the repository's main folder and run:

```powershell
dotnet run --project RetailInventoryApp/RetailInventorySales/RetailInventorySales.csproj
```

## Project Structure

| Location | Purpose |
|---|---|
| `RetailInventoryApp/RetailInventorySales` | Main application |
| `RetailInventoryApp/RetailInventorySales.Tests` | Integration and validation tests |
| `RetailInventoryApp/Run.cmd` | Windows launch script |

The application separates the user interface, business logic, data models, and database configuration into `Forms`, `Services`, `Models`, and `Data` folders.

## Testing

The test project checks product validation, sales processing, stock updates, and data persistence using separate temporary databases.

Run it from the repository's main folder:

```powershell
dotnet run --project RetailInventoryApp/RetailInventorySales.Tests/RetailInventorySales.Tests.csproj
```

The console displays the test results.

## Data Storage

The application creates a local SQLite database on its first successful launch. Data is saved between sessions.

Database location:

```text
%LOCALAPPDATA%\RetailInventorySales\retail.db
```
