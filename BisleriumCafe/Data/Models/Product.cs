namespace BisleriumCafe.Data.Models;

public class Product : IModel, ICloneable
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; }

    public string Description { get; set; }

    public ProductType ProductType { get; set; }

    public decimal Price { get; set; }

    public int AvailableQuantity { get; set; }

    public object Clone()
    {
        return new Product
        {
            Id = Id,
            Name = Name,
            Description = Description,
            ProductType = ProductType,
            Price = Price,
            AvailableQuantity = AvailableQuantity
        };
    }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}
