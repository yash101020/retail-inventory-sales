using Microsoft.EntityFrameworkCore;
using RetailInventorySales.Data;
using RetailInventorySales.Models;

namespace RetailInventorySales.Services;

public class HistoryService(string databasePath)
{
    public List<SaleTransaction> GetTransactions()
    {
        using var db = new InventoryDbContext(databasePath);
        return db.Transactions.AsNoTracking().OrderByDescending(t => t.TransactionDate).ThenByDescending(t => t.Id).ToList();
    }
    public List<TransactionItem> GetItems(int transactionId)
    {
        using var db = new InventoryDbContext(databasePath);
        return db.TransactionItems.AsNoTracking().Where(i => i.TransactionId == transactionId).OrderBy(i => i.Id).ToList();
    }
}
