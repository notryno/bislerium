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
    //public DateTime LastPurchaseDate { get; set; }
    public DateTime MembershipStartDate { get; set; }
    private bool _isValid;

    public bool IsValid
    {
        get
        {
            // If IsValid has been manually set, return the stored value
            if (_isValid)
            {
                return _isValid;
            }

            // Otherwise, check if the membership is still valid for 1 month
            return (DateTime.Now - MembershipStartDate).TotalDays <= 30;
        }
        set
        {
            // Allow manual setting of IsValid
            _isValid = value;
        }
    }
    

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
            //LastPurchaseDate = LastPurchaseDate,
            IsValid = IsValid,
            MembershipStartDate = MembershipStartDate,
        };
    }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}
