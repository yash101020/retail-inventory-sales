namespace RetailInventorySales.Data;

public static class AppPaths
{
    public static string DataFolder => Environment.GetEnvironmentVariable("RETAIL_DATA_DIR")
        ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RetailInventorySales");
    public static string DatabasePath => Path.Combine(DataFolder, "retail.db");
}
