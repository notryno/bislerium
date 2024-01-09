namespace BisleriumCafe.Data.Models;

public class Member : IModel, ICloneable
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UserName { get; set; }
    public string Phone { get; set; }
    public string FullName { get; set; }
    public bool IsRegularCustomer { get; set; }
    public int PurchasesCount { get; set; }
    public int FreeCoffeeRedemptionCount { get; set; }
    public DateTime LastPurchaseDate { get; set; }
    public DateTime MembershipStartDate { get; set; }

    public object Clone()
    {
        return new Member
        {
            Id = Id,
            UserName = UserName,
            FullName = FullName,
            Phone = Phone,
            IsRegularCustomer = IsRegularCustomer,
            PurchasesCount = PurchasesCount,
            FreeCoffeeRedemptionCount = FreeCoffeeRedemptionCount,
            LastPurchaseDate = LastPurchaseDate,
            MembershipStartDate = MembershipStartDate,
        };
    }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}
