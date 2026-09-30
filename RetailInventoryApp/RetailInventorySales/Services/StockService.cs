using Microsoft.EntityFrameworkCore;
using RetailInventorySales.Data;
using RetailInventorySales.Models;

namespace RetailInventorySales.Services;

public class StockService(string databasePath)
{
    public const int LowStockThreshold = 5;
    public List<Product> GetLowStockProducts()
    {
        using var db = new InventoryDbContext(databasePath);
        return db.Products.AsNoTracking().Where(p => p.StockQuantity <= LowStockThreshold)
            .OrderBy(p => p.StockQuantity).ThenBy(p => p.Name).ToList();
    }
}
