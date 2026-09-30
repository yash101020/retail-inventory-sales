using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using RetailInventorySales.Models;

namespace RetailInventorySales.Data;

public class InventoryDbContext(string databasePath) : DbContext
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<SaleTransaction> Transactions => Set<SaleTransaction>();
    public DbSet<TransactionItem> TransactionItems => Set<TransactionItem>();

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        var connection = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder
        {
            DataSource = databasePath, ForeignKeys = true, DefaultTimeout = 5
        };
        options.UseSqlite(connection.ToString());
    }

    protected override void OnModelCreating(ModelBuilder model)
    {
        // Store money as integer cents, avoiding floating-point rounding in SQLite.
        var money = new ValueConverter<decimal, long>(v => (long)(v * 100m), v => v / 100m);
        model.Entity<Product>(p =>
        {
            p.HasIndex(x => x.NormalizedName).IsUnique();
            p.Property(x => x.Name).IsRequired().HasMaxLength(100);
            p.Property(x => x.NormalizedName).IsRequired();
            p.Property(x => x.Price).HasConversion(money).IsConcurrencyToken();
            p.Property(x => x.StockQuantity).IsConcurrencyToken();
            p.ToTable("Products", t =>
            {
                t.HasCheckConstraint("CK_Product_Price", "Price >= 0");
                t.HasCheckConstraint("CK_Product_Stock", "StockQuantity >= 0");
            });
        });
        model.Entity<SaleTransaction>(t =>
        {
            t.ToTable("Transactions");
            t.Property(x => x.TotalAmount).HasConversion(money);
            t.HasMany(x => x.Items).WithOne(x => x.Transaction).HasForeignKey(x => x.TransactionId);
        });
        model.Entity<TransactionItem>(i =>
        {
            i.Property(x => x.UnitPrice).HasConversion(money);
            i.Property(x => x.Subtotal).HasConversion(money);
            i.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
            i.ToTable("TransactionItems", t => t.HasCheckConstraint("CK_Item_Quantity", "Quantity > 0"));
        });
    }
}
