namespace BisleriumCafe.Data.Providers;

internal static class JsonFileProvider
{
    public static IServiceCollection AddJsonFileProvider(this IServiceCollection services)
    {
        return services.AddSingleton<FileProvider<User>, JsonFileProvider<User>>()
            .AddSingleton<FileProvider<Spare>, JsonFileProvider<Spare>>()
            .AddSingleton<FileProvider<Product>, JsonFileProvider<Product>>() //Ryan
            .AddSingleton<FileProvider<Member>, JsonFileProvider<Member>>() //Ryan
            .AddSingleton<FileProvider<ActivityLog>, JsonFileProvider<ActivityLog>>();
    }
}
