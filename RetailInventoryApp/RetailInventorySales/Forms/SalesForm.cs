using RetailInventorySales.Models;
using RetailInventorySales.Services;

namespace RetailInventorySales.Forms;

public class SalesForm : Form
{
    private readonly ProductService products;
    private readonly SalesService sales;
    private readonly List<CartItem> cart = [];
    private readonly ComboBox productInput = new() { Width = 260, DropDownStyle = ComboBoxStyle.DropDownList, DisplayMember = "Name" };
    private readonly TextBox quantityInput = new() { Width = 85, Text = "1", AccessibleName = "Sale quantity" };
    private readonly Label available = Ui.Label("Select a product");
    private readonly Label total = Ui.Label("Total: LKR 0.00");
    private readonly DataGridView grid = Ui.Grid();

    public SalesForm(string databasePath)
    {
        products = new ProductService(databasePath); sales = new SalesService(databasePath);
        Ui.Style(this, "Sales");
        Ui.Column(grid, "ProductName", "Product"); Ui.Column(grid, "Quantity", "Quantity");
        Ui.Column(grid, "UnitPrice", "Unit price (LKR)", "N2"); Ui.Column(grid, "Subtotal", "Subtotal (LKR)", "N2");
        total.Font = new Font(Font, FontStyle.Bold);
        Controls.Add(grid);
        var footer = Ui.Row(total, Ui.Button("Remove selected", Remove), Ui.Button("Clear sale", ClearSale), Ui.Button("Complete sale", Complete));
        footer.Dock = DockStyle.Bottom; Controls.Add(footer);
        Controls.Add(Ui.Row(available));
        Controls.Add(Ui.Row(Ui.Label("Product"), productInput, Ui.Label("Quantity"), quantityInput,
            Ui.Button("Add to sale", Add), Ui.Button("Refresh products", RefreshProducts)));
        productInput.SelectedIndexChanged += (_, _) => ShowAvailable();
        Shown += (_, _) => Ui.Run(this, RefreshProducts);
        FormClosing += (_, e) =>
        {
            if (cart.Count > 0 && MessageBox.Show(this, "Discard this unfinished sale?", "Close sales", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) e.Cancel = true;
        };
    }
    private void ShowAvailable()
    {
        available.Text = productInput.SelectedItem is Product p
            ? $"Available stock: {p.StockQuantity}   |   Unit price: LKR {p.Price:N2}" : "No products. Add products in Product Management first.";
    }
    private void RefreshProducts() { productInput.DataSource = products.GetProducts(); ShowAvailable(); }
    private void RefreshCart()
    {
        grid.DataSource = null; grid.DataSource = cart.ToList(); total.Text = $"Total: LKR {cart.Sum(i => i.Subtotal):N2}";
    }
    private void Add()
    {
        if (productInput.SelectedItem is not Product p) throw new ArgumentException("Select a product first.");
        int quantity = InputValidation.SaleQuantity(quantityInput.Text);
        var existing = cart.FirstOrDefault(i => i.ProductId == p.Id);
        long combined = (long)quantity + (existing?.Quantity ?? 0);
        if (combined > p.StockQuantity) throw new ArgumentException($"Only {p.StockQuantity} units of {p.Name} are available, including any already in this sale.");
        if (existing != null && existing.UnitPrice != p.Price) throw new ArgumentException("This price changed. Remove the existing line and add the product again.");
        if (existing != null) existing.Quantity = (int)combined;
        else cart.Add(new CartItem { ProductId = p.Id, ProductName = p.Name, Quantity = quantity, UnitPrice = p.Price });
        RefreshCart();
    }
    private void Remove()
    {
        if (grid.CurrentRow?.DataBoundItem is not CartItem line) throw new ArgumentException("Select a sale item to remove.");
        cart.Remove(line); RefreshCart();
    }
    private void ClearSale()
    {
        if (cart.Count > 0 && MessageBox.Show(this, "Clear all items in this sale?", "Clear sale", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        cart.Clear(); RefreshCart(); RefreshProducts();
    }
    private void Complete()
    {
        if (cart.Count == 0) throw new ArgumentException("Add at least one product before completing the sale.");
        if (MessageBox.Show(this, $"Complete this sale for LKR {cart.Sum(i => i.Subtotal):N2}?", "Confirm sale", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        var sale = sales.CompleteSale(cart);
        cart.Clear(); RefreshCart();
        MessageBox.Show(this, $"Sale #{sale.Id} completed. Total: LKR {sale.TotalAmount:N2}. Stock updated.", "Sale completed", MessageBoxButtons.OK, MessageBoxIcon.Information);
        RefreshProducts();
    }
}
