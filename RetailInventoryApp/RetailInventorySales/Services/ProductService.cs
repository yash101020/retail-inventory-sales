using Microsoft.EntityFrameworkCore;
using RetailInventorySales.Data;
using RetailInventorySales.Models;

namespace RetailInventorySales.Services;

public class ProductService(string databasePath)
{
    public const int MaximumStock = 1_000_000;
    public const decimal MaximumPrice = 1_000_000m;

    public List<Product> GetProducts()
    {
        using var db = new InventoryDbContext(databasePath);
        return db.Products.AsNoTracking().OrderBy(p => p.Name).ToList();
    }

    public void Save(int? id, string name, decimal price, int stock)
    {
        name = string.Join(" ", name.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        if (name.Length is 0 or > 100) throw new ArgumentException("Enter a product name with 1 to 100 characters.");
        if (price < 0 || price > MaximumPrice || decimal.Round(price, 2) != price)
            throw new ArgumentException("Price must be from 0 to 1,000,000 with at most two decimal places.");
        ValidateStock(stock);
        string normalizedName = name.ToUpperInvariant();
        using var db = new InventoryDbContext(databasePath);
        if (db.Products.Any(p => p.NormalizedName == normalizedName && p.Id != (id ?? 0)))
            throw new ArgumentException("A product with this name already exists. Use a different name.");
        var product = id.HasValue ? db.Products.Find(id.Value)
            ?? throw new ArgumentException("This product no longer exists. Refresh the list.") : new Product();
        product.Name = name;
        product.NormalizedName = normalizedName;
        product.Price = price;
        product.StockQuantity = stock;
        if (!id.HasValue) db.Products.Add(product);
        db.SaveChanges();
    }

    public void Delete(int id)
    {
        using var db = new InventoryDbContext(databasePath);
        var product = db.Products.Find(id) ?? throw new ArgumentException("This product no longer exists.");
        if (db.TransactionItems.Any(i => i.ProductId == id))
            throw new ArgumentException("This product has sales history and cannot be deleted. You can edit its details or set its stock to zero.");
        db.Products.Remove(product);
        db.SaveChanges();
    }

    public static void ValidateStock(int stock)
    {
        if (stock < 0 || stock > MaximumStock)
            throw new ArgumentException("Stock must be a whole number from 0 to 1,000,000.");
    }

    public void SetStock(int id, int stock)
    {
        ValidateStock(stock);
        using var db = new InventoryDbContext(databasePath);
        var product = db.Products.Find(id) ?? throw new ArgumentException("This product no longer exists. Refresh the list.");
        product.StockQuantity = stock;
        db.SaveChanges();
    }
}
