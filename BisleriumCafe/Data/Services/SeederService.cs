namespace BisleriumCafe.Data.Services;

internal class SeederService
{
    internal int MinUsers { get; set; } = 30;
    internal int MaxUsers { get; set; } = 44;
    internal int MinSpares { get; set; } = 50;
    internal int MaxSpares { get; set; } = 120;
    //internal int MinProducts { get; set; } = 50;
    //internal int MaxProducts { get; set; } = 120;
    internal int MinActivityLogs { get; set; } = 1650;
    internal int MaxActivityLogs { get; set; } = 3200;

    private readonly Repository<User> _userRepository;
    private readonly Repository<Spare> _spareRepository;
    //private readonly Repository<Product> _productRepository;
    private readonly Repository<ActivityLog> _activityLogRepository;

    public SeederService(Repository<User> userRepository, Repository<Spare> spareRepository, Repository<ActivityLog> activityLogRepository)
    {
        _userRepository = userRepository;
        _spareRepository = spareRepository;
        //_productRepository = productRepository;
        _activityLogRepository = activityLogRepository;
    }

    public async Task SeedAsync()
    {
        ICollection<User> users = _userRepository._sourceData = Seeder.GenerateUsers(MinUsers, MaxUsers);
        ICollection<Spare> spares = _spareRepository._sourceData = Seeder.GenerateSpares(MinSpares, MaxSpares);
        //ICollection<Product> product = _productRepository._sourceData = Seeder.GenerateProducts(MinProducts, MaxProducts);
        _activityLogRepository._sourceData = Seeder.GenerateActivityLogs(users, spares, MinActivityLogs, MaxActivityLogs);
        await _userRepository.FlushAsync();
        await _spareRepository.FlushAsync();
        //await _productRepository.FlushAsync();
        await _activityLogRepository.FlushAsync();
    }
}
