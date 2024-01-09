namespace BisleriumCafe.Data.Providers;

internal static class CSVFileProviderInjection
{
    public static IServiceCollection AddCsvFileProvider(this IServiceCollection services)
    {
        return services.AddSingleton<FileProvider<User>, CsvFileProvider<User>>()
            .AddSingleton<FileProvider<Spare>, CsvFileProvider<Spare>>()
            .AddSingleton<FileProvider<Product>, CsvFileProvider<Product>>() //Ryan
            .AddSingleton<FileProvider<ActivityLog>, CsvFileProvider<ActivityLog>>();
    }
}
