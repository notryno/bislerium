﻿namespace BisleriumCafe.Data.Providers;

internal static class ExcelFileProviderInjection
{
    public static IServiceCollection AddExcelFileProvider(this IServiceCollection services)
    {
        return services.AddSingleton<FileProvider<User>, ExcelFileProvider<User>>()
            .AddSingleton<FileProvider<Spare>, ExcelFileProvider<Spare>>()
            .AddSingleton<FileProvider<Product>, ExcelFileProvider<Product>>() //Ryan
            .AddSingleton<FileProvider<Member>, ExcelFileProvider<Member>>() //Ryan
            .AddSingleton<FileProvider<Transaction>, CsvFileProvider<Transaction>>() //Ryan
            .AddSingleton<FileProvider<ActivityLog>, ExcelFileProvider<ActivityLog>>();
    }
}
