using RetailInventorySales.Data;
using RetailInventorySales.Models;

namespace RetailInventorySales.Services;

public class SalesService(string databasePath)
{
    public SaleTransaction CompleteSale(IReadOnlyList<CartItem> cart)
    {
        if (cart.Count == 0) throw new ArgumentException("Add at least one product before completing the sale.");
        using var db = new InventoryDbContext(databasePath);
        // Read current stock and save the receipt/stock changes in the SAME transaction.
        // Disposing without Commit rolls everything back if any step fails.
        using var databaseTransaction = db.Database.BeginTransaction();
        var sale = new SaleTransaction { TransactionDate = DateTime.Now };
        foreach (var line in cart)
        {
            if (line.Quantity <= 0) throw new ArgumentException("Quantity must be a whole number greater than zero.");
            var product = db.Products.Find(line.ProductId)
                ?? throw new ArgumentException("A product in the sale no longer exists. Remove it and refresh products.");
            if (line.Quantity > product.StockQuantity)
                throw new ArgumentException($"Not enough stock for {product.Name}. Available: {product.StockQuantity}.");
            if (line.UnitPrice != product.Price)
                throw new ArgumentException($"The price of {product.Name} changed. Clear the sale and add it again to review the new total.");
            sale.Items.Add(new TransactionItem
            {
                ProductId = product.Id, ProductName = product.Name, Quantity = line.Quantity,
                UnitPrice = product.Price, Subtotal = product.Price * line.Quantity
            });
            product.StockQuantity -= line.Quantity;
        }
        sale.TotalAmount = sale.Items.Sum(i => i.Subtotal);
        db.Transactions.Add(sale);
        db.SaveChanges();
        databaseTransaction.Commit();
        return sale;
    }
}
