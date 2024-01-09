using System;
namespace BisleriumCafe.Pages
{
	public partial class Transaction
	{
        public const string Route = "/transaction";
        private string MemberInput { get; set; }
        private List<Product> Products;
        private List<CartItem> ShoppingCart = new List<CartItem>();
        private Member FoundMember;

        protected override void OnInitialized()
        {
            // Fetch products from the repository
            Products = (List<Product>)ProductRepository.GetAll();
        }

        private void AddToCart(Product product)
        {
            // Check if the product is already in the cart
            var existingCartItem = ShoppingCart.FirstOrDefault(item => item.Product.Id == product.Id);

            if (existingCartItem != null)
            {
                // If the product is already in the cart, increment the quantity
                existingCartItem.Quantity++;
            }
            else
            {
                // If the product is not in the cart, add a new cart item with quantity 1
                ShoppingCart.Add(new CartItem { Product = product, Addons = new List<Addon>(), Quantity = 1 });
            }

            Snackbar.Add($"Added {product.Name} to the cart.", Severity.Success);
        }


        private void Checkout()
        {
            // Check if the cart is not empty
            if (ShoppingCart.Count > 0)
            {
                // Iterate through cart items and identify associated members
                foreach (var cartItem in ShoppingCart)
                {
                    // Get the product type of the cart item
                    var productType = cartItem.Product.ProductType;

                    // Check if the product type is "coffee"
                    if (productType.ToString().Equals("coffee", StringComparison.OrdinalIgnoreCase))
                    {
                        // Find the member associated with the product (you might need to adjust this logic based on your data model)
                        FoundMember = MemberRepository.GetAll().FirstOrDefault(member =>
                            member.UserName.Equals(cartItem.Product.Name, StringComparison.OrdinalIgnoreCase));

                        // If the member is found, increase the purchase count by the quantity in the cart
                        if (FoundMember != null)
                        {
                            FoundMember.PurchasesCount += cartItem.Quantity;
                            // Update the member in the repository
                            MemberRepository.Update(FoundMember);
                        }
                    }
                }

                // Clear the shopping cart after processing
                ShoppingCart.Clear();

                Snackbar.Add("Checkout successful.", Severity.Success);
            }
            else
            {
                Snackbar.Add("Cannot checkout with an empty cart.", Severity.Error);
            }
        }

        private class CartItem
        {
            public Product Product { get; set; }
            public List<Addon> Addons { get; set; }
            public int Quantity { get; set; }
        }

        private class Addon
        {
            public string Name { get; set; }
            public decimal Price { get; set; }
        }

        private void IncreaseQuantity(CartItem cartItem)
        {
            cartItem.Quantity++;
        }

        private void DecreaseQuantity(CartItem cartItem)
        {
            if (cartItem.Quantity > 1)
            {
                cartItem.Quantity--;
            }
            else
            {
                // If quantity is 1 or less, remove the item from the cart
                ShoppingCart.Remove(cartItem);
            }
        }

        private decimal CalculateTotal()
        {
            return ShoppingCart.Sum(cartItem => cartItem.Quantity * cartItem.Product.Price);
        }

        private void SearchMember()
        {
            // Check if MemberInput is a valid username or phone number
            if (!string.IsNullOrWhiteSpace(MemberInput))
            {
                // Try to find the member by username
                FoundMember = MemberRepository.GetAll().FirstOrDefault(member =>
                    member.UserName.Equals(MemberInput, StringComparison.OrdinalIgnoreCase));

                // If the member is not found by username, try to find by phone number
                if (FoundMember == null)
                {
                    FoundMember = MemberRepository.GetAll().FirstOrDefault(member =>
                        member.Phone.Equals(MemberInput, StringComparison.OrdinalIgnoreCase));
                }
            }

            if (FoundMember != null)
            {
                Snackbar.Add($"Member found: {FoundMember.UserName}", Severity.Success);
                // Do something with the found member, if needed
            }
            else
            {
                Snackbar.Add($"No member found with the given username or phone number.", Severity.Error);
            }
        }

    }
}

