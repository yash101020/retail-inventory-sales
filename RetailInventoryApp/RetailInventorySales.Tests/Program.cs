using Microsoft.EntityFrameworkCore;
using RetailInventorySales.Data;
using RetailInventorySales.Models;
using RetailInventorySales.Services;

// A small executable integration-test suite: no extra test framework required.
// Every test uses a new database. Never points at the user's inventory database.
int passed = 0, failed = 0;
string root = Path.Combine(Path.GetTempPath(), "RetailInventorySalesTests", Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(root);

if (args.Length == 2 && args[0] == "--verify-persistence")
{
    using var reopened = new InventoryDbContext(args[1]);
    bool correct = reopened.Products.Single().StockQuantity == 17 && reopened.Transactions.Single().TotalAmount == 30m;
    Console.WriteLine(correct ? "PASS: Data persisted across processes." : "FAIL: Persistence mismatch.");
    return correct ? 0 : 1;
}

void Check(bool condition, string message = "Unexpected result")
{
    if (!condition) throw new Exception(message);
}
void Reject(Action action)
{
    try { action(); } catch (ArgumentException) { return; }
    throw new Exception("Expected a validation error.");
}
void Test(string title, Action<string> action)
{
    string path = Path.Combine(root, Guid.NewGuid().ToString("N") + ".db");
    try
    {
        using (var db = new InventoryDbContext(path)) db.Database.EnsureCreated();
        action(path); passed++; Console.WriteLine("PASS: " + title);
    }
    catch (Exception ex) { failed++; Console.WriteLine($"FAIL: {title}\n{ex}"); }
}
Product Add(string path, string name = "Tea", decimal price = 10m, int stock = 20)
{
    var service = new ProductService(path); service.Save(null, name, price, stock);
    return service.GetProducts().Single(p => p.Name == name);
}
CartItem Line(Product p, int quantity) => new() { ProductId = p.Id, ProductName = p.Name, UnitPrice = p.Price, Quantity = quantity };
int Stock(string path, int id) => new ProductService(path).GetProducts().Single(p => p.Id == id).StockQuantity;
int SaleCount(string path) => new HistoryService(path).GetTransactions().Count;

Test("Add valid product", p => { var product = Add(p); Check(product.Id > 0 && product.Price == 10m && product.StockQuantity == 20); });
Test("Reject empty name", p => Reject(() => new ProductService(p).Save(null, "  ", 10, 1)));
Test("Reject name longer than 100 characters", p => Reject(() => new ProductService(p).Save(null, new string('a', 101), 10, 1)));
Test("Reject negative price", p => Reject(() => new ProductService(p).Save(null, "Tea", -1, 1)));
Test("Reject negative stock", p => Reject(() => new ProductService(p).Save(null, "Tea", 10, -1)));
Test("Reject excess monetary precision", p => Reject(() => new ProductService(p).Save(null, "Tea", 1.001m, 1)));
Test("Reject price above bound", p => Reject(() => new ProductService(p).Save(null, "Tea", ProductService.MaximumPrice + 1, 1)));
Test("Reject stock above bound", p => Reject(() => new ProductService(p).Save(null, "Tea", 1, ProductService.MaximumStock + 1)));
Test("Reject duplicate ignoring case and whitespace", p => { Add(p, "Tea Bags"); Reject(() => new ProductService(p).Save(null, " tea   BAGS ", 10, 1)); });
Test("Allow zero price and zero stock", p => { var product = Add(p, "Sample", 0, 0); Check(product.Price == 0 && product.StockQuantity == 0); });
Test("Edit product", p => { var product = Add(p); var service = new ProductService(p); service.Save(product.Id, "Coffee", 12.50m, 30); var changed = service.GetProducts().Single(); Check(changed.Name == "Coffee" && changed.Price == 12.50m && changed.StockQuantity == 30); });
Test("Delete unused product", p => { var product = Add(p); new ProductService(p).Delete(product.Id); Check(new ProductService(p).GetProducts().Count == 0); });
Test("Reject edit of missing product", p => Reject(() => new ProductService(p).Save(999, "Tea", 10, 1)));
Test("Reject delete of missing product", p => Reject(() => new ProductService(p).Delete(999)));
Test("Reject empty sale", p => Reject(() => new SalesService(p).CompleteSale([])));
Test("Single-product sale, total, stock reduction and saved receipt", p => { var product = Add(p); var sale = new SalesService(p).CompleteSale([Line(product, 3)]); Check(sale.TotalAmount == 30 && Stock(p, product.Id) == 17 && SaleCount(p) == 1); });
Test("Multiple products and exact cent calculations", p => { var a = Add(p, "Tea", 12.35m); var b = Add(p, "Milk", 4.10m); var sale = new SalesService(p).CompleteSale([Line(a, 3), Line(b, 2)]); Check(sale.TotalAmount == 45.25m && Stock(p, a.Id) == 17 && Stock(p, b.Id) == 18); var items = new HistoryService(p).GetItems(sale.Id); Check(items.Count == 2 && items.Sum(i => i.Subtotal) == sale.TotalAmount); });
Test("Reject zero quantity", p => { var a = Add(p); Reject(() => new SalesService(p).CompleteSale([Line(a, 0)])); });
Test("Reject negative quantity", p => { var a = Add(p); Reject(() => new SalesService(p).CompleteSale([Line(a, -2)])); });
Test("Reject nonnumeric quantity", _ => Reject(() => InputValidation.SaleQuantity("abc")));
Test("Reject fractional quantity", _ => Reject(() => InputValidation.SaleQuantity("1.5")));
Test("Reject overflowing quantity", _ => Reject(() => InputValidation.SaleQuantity("999999999999")));
Test("Reject nonnumeric price", _ => Reject(() => InputValidation.Price("abc")));
Test("Reject nonnumeric stock", _ => Reject(() => InputValidation.WholeNumber("abc", "stock")));
Test("Reject insufficient stock without saving anything", p => { var a = Add(p); Reject(() => new SalesService(p).CompleteSale([Line(a, 21)])); Check(Stock(p, a.Id) == 20 && SaleCount(p) == 0); });
Test("Exact available quantity can be sold", p => { var a = Add(p); new SalesService(p).CompleteSale([Line(a, 20)]); Check(Stock(p, a.Id) == 0); });
Test("Repeated product cannot exceed stock", p => { var a = Add(p); Reject(() => new SalesService(p).CompleteSale([Line(a, 11), Line(a, 10)])); Check(Stock(p, a.Id) == 20 && SaleCount(p) == 0); });
Test("Missing product rejected", p => Reject(() => new SalesService(p).CompleteSale([new CartItem { ProductId = 999, Quantity = 1 }])));
Test("Later invalid line does not reduce earlier stock", p => { var a = Add(p); Reject(() => new SalesService(p).CompleteSale([Line(a, 3), new CartItem { ProductId = 999, Quantity = 1 }])); Check(Stock(p, a.Id) == 20 && SaleCount(p) == 0); });
Test("Stale price rejected", p => { var a = Add(p); new ProductService(p).Save(a.Id, a.Name, 11, 20); Reject(() => new SalesService(p).CompleteSale([Line(a, 1)])); Check(Stock(p, a.Id) == 20 && SaleCount(p) == 0); });
Test("Stock refreshed at checkout", p => { var a = Add(p); new ProductService(p).SetStock(a.Id, 2); Reject(() => new SalesService(p).CompleteSale([Line(a, 3)])); Check(Stock(p, a.Id) == 2); });
Test("Prevent deletion of sold product", p => { var a = Add(p); new SalesService(p).CompleteSale([Line(a, 1)]); Reject(() => new ProductService(p).Delete(a.Id)); Check(Stock(p, a.Id) == 19); });
Test("History keeps original name and price", p => { var a = Add(p); var sale = new SalesService(p).CompleteSale([Line(a, 2)]); new ProductService(p).Save(a.Id, "Renamed", 999, 18); var item = new HistoryService(p).GetItems(sale.Id).Single(); Check(item.ProductName == "Tea" && item.UnitPrice == 10 && item.Subtotal == 20); });
Test("Newest transactions displayed first", p => { var a = Add(p); var sales = new SalesService(p); sales.CompleteSale([Line(a, 1)]); var last = sales.CompleteSale([Line(a, 2)]); Check(new HistoryService(p).GetTransactions()[0].Id == last.Id); });
Test("Set current stock", p => { var a = Add(p); new ProductService(p).SetStock(a.Id, 30); Check(Stock(p, a.Id) == 30); });
Test("Reject invalid stock adjustment", p => { var a = Add(p); Reject(() => new ProductService(p).SetStock(a.Id, -1)); Check(Stock(p, a.Id) == 20); });
Test("Low stock includes zero and five, excludes six", p => { Add(p, "Empty", 10, 0); Add(p, "Low", 10, 5); Add(p, "Okay", 10, 6); var low = new StockService(p).GetLowStockProducts(); Check(low.Count == 2 && low[0].StockQuantity == 0 && low[1].StockQuantity == 5); });
Test("Sale triggers low stock", p => { var a = Add(p, "Tea", 10, 6); new SalesService(p).CompleteSale([Line(a, 1)]); Check(new StockService(p).GetLowStockProducts().Single().StockQuantity == 5); });
Test("Database constraint rejects negative stock", p => { var a = Add(p); using var db = new InventoryDbContext(p); try { db.Database.ExecuteSqlRaw("UPDATE Products SET StockQuantity = -1"); } catch (Microsoft.Data.Sqlite.SqliteException) { Check(Stock(p, a.Id) == 20); return; } throw new Exception("Constraint did not reject bad stock."); });
Test("Database foreign key protects sold products", p => { var a = Add(p); new SalesService(p).CompleteSale([Line(a, 1)]); using var db = new InventoryDbContext(p); try { db.Database.ExecuteSqlRaw("DELETE FROM Products"); } catch (Microsoft.Data.Sqlite.SqliteException) { return; } throw new Exception("Foreign key did not protect product."); });
Test("Forced database failure rolls back stock, transaction and items", p =>
{
    var a = Add(p);
    using (var db = new InventoryDbContext(p))
        db.Database.ExecuteSqlRaw("CREATE TRIGGER ForceItemFailure BEFORE INSERT ON TransactionItems BEGIN SELECT RAISE(ABORT, 'Simulated write failure'); END;");
    bool rejected = false;
    try { new SalesService(p).CompleteSale([Line(a, 3)]); } catch (DbUpdateException) { rejected = true; }
    using var reopened = new InventoryDbContext(p);
    Check(rejected && Stock(p, a.Id) == 20 && SaleCount(p) == 0 && reopened.TransactionItems.Count() == 0);
});
Test("Concurrent stale stock update is detected", p =>
{
    var a = Add(p); using var first = new InventoryDbContext(p); using var second = new InventoryDbContext(p);
    var one = first.Products.Single(); var two = second.Products.Single();
    one.StockQuantity = 19; first.SaveChanges(); two.StockQuantity = 18;
    try { second.SaveChanges(); } catch (DbUpdateConcurrencyException) { Check(Stock(p, a.Id) == 19); return; }
    throw new Exception("Stale update was not rejected.");
});
Test("Data persists after closing and reopening database", p => { var a = Add(p); new SalesService(p).CompleteSale([Line(a, 3)]); Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools(); using var reopened = new InventoryDbContext(p); Check(reopened.Products.Single().StockQuantity == 17 && reopened.Transactions.Single().TotalAmount == 30 && reopened.TransactionItems.Single().Quantity == 3); });
Test("Unreadable database error reaches caller", _ =>
{
    string directory = Path.Combine(root, "not-a-database"); Directory.CreateDirectory(directory);
    try { new ProductService(directory).GetProducts(); } catch (Microsoft.Data.Sqlite.SqliteException) { return; }
    throw new Exception("Expected database error was hidden.");
});

string persistencePath = Path.Combine(root, "persistence.db");
using (var db = new InventoryDbContext(persistencePath)) db.Database.EnsureCreated();
var persisted = Add(persistencePath); new SalesService(persistencePath).CompleteSale([Line(persisted, 3)]);
Console.WriteLine($"\n{passed} passed; {failed} failed.");
Console.WriteLine($"For a separate-process persistence check, rerun with: --verify-persistence \"{persistencePath}\"");
Console.WriteLine($"Test databases: {root}");
return failed == 0 ? 0 : 1;
