namespace BisleriumCafe.Data.Repositories;

internal static class RepositoryInjection
{
    public static IServiceCollection AddRepository(this IServiceCollection services)
    {
        return services.AddSingleton<Repository<User>, Repository<User>>()
            .AddSingleton<Repository<Spare>, Repository<Spare>>()
            .AddSingleton<Repository<Product>, Repository <Product>>() //Ryan
            .AddSingleton<Repository<Member>, Repository<Member>>() //Ryan
            .AddSingleton<Repository<ActivityLog>, Repository<ActivityLog>>();
    }
}
