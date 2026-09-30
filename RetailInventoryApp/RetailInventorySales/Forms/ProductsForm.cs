using RetailInventorySales.Models;
using RetailInventorySales.Services;

namespace RetailInventorySales.Forms;

public class ProductsForm : Form
{
    private readonly ProductService service;
    private readonly DataGridView grid = Ui.Grid();
    private readonly TextBox nameInput = new() { Width = 220, MaxLength = 100, AccessibleName = "Product name" };
    private readonly TextBox priceInput = new() { Width = 110, AccessibleName = "Price" };
    private readonly TextBox stockInput = new() { Width = 110, AccessibleName = "Stock quantity" };
    private int? editingId;
    private readonly Label mode = Ui.Label("New product");

    public ProductsForm(string databasePath)
    {
        service = new ProductService(databasePath);
        Ui.Style(this, "Product Management");
        Ui.Column(grid, "Id", "ID"); Ui.Column(grid, "Name", "Product");
        Ui.Column(grid, "Price", "Price (LKR)", "N2"); Ui.Column(grid, "StockQuantity", "Stock");
        Controls.Add(grid);
        Controls.Add(Ui.Row(Ui.Button("New / Clear", Clear), Ui.Button("Edit selected", Edit),
            Ui.Button("Save product", Save), Ui.Button("Delete selected", Delete), Ui.Button("Refresh", RefreshProducts)));
        Controls.Add(Ui.Row(Ui.Label("Name"), nameInput, Ui.Label("Price (LKR)"), priceInput, Ui.Label("Stock"), stockInput));
        Controls.Add(Ui.Row(mode));
        Shown += (_, _) => Ui.Run(this, () => { Clear(); RefreshProducts(); });
    }
    private Product Selected() => grid.CurrentRow?.DataBoundItem as Product
        ?? throw new ArgumentException("Select a product first.");
    private void RefreshProducts() => grid.DataSource = service.GetProducts();
    private void Clear()
    {
        editingId = null; mode.Text = "New product"; nameInput.Clear(); priceInput.Text = "0.00"; stockInput.Text = "0"; nameInput.Focus();
    }
    private void Edit()
    {
        var product = Selected(); editingId = product.Id; mode.Text = $"Editing product #{product.Id}";
        nameInput.Text = product.Name; priceInput.Text = product.Price.ToString("0.00"); stockInput.Text = product.StockQuantity.ToString();
    }
    private void Save()
    {
        decimal price = InputValidation.Price(priceInput.Text);
        int stock = InputValidation.WholeNumber(stockInput.Text, "stock");
        service.Save(editingId, nameInput.Text, price, stock);
        Clear(); RefreshProducts();
        MessageBox.Show(this, "Product saved.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
    private void Delete()
    {
        var product = Selected();
        if (MessageBox.Show(this, $"Delete '{product.Name}'?", "Confirm deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        service.Delete(product.Id); Clear(); RefreshProducts();
    }
}
