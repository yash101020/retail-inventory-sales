using RetailInventorySales.Data;
using RetailInventorySales.Forms;
namespace RetailInventorySales;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += (_, e) => Ui.ShowError(null, e.Exception);
        try
        {
            Directory.CreateDirectory(AppPaths.DataFolder);
            using (var db = new InventoryDbContext(AppPaths.DatabasePath)) db.Database.EnsureCreated();
            Application.Run(new MainForm(AppPaths.DatabasePath));
        }
        catch (Exception ex) { Ui.ShowError(null, ex); }
    }    
}
