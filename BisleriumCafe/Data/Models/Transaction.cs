namespace BisleriumCafe.Data.Models;

public class Transaction : IModel, ICloneable
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string MemberUsername { get; set; }
    public DateTime PurchaseDate { get; set; }
    public string ProductName { get; set; }
    public int Quantity { get; set; }
    public string Discount { get; set; }



    public object Clone()
    {
        return new Transaction
        {
            Id = Id,
            MemberUsername = MemberUsername,
            PurchaseDate = PurchaseDate,
            ProductName = ProductName,
            Quantity = Quantity,
            Discount = Discount,
        };
    }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}
