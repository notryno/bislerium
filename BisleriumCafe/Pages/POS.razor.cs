using System;
namespace BisleriumCafe.Pages
{
    public partial class POS
    {
        public const string Route = "/pos";
        private string MemberInput { get; set; }
        private List<Product> Products;
        private List<CartItem> ShoppingCart = new List<CartItem>();
        private Member FoundMember;
        private bool isRegular;

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
                decimal totalDiscount = 0;
                // Iterate through cart items and identify associated members
                foreach (var cartItem in ShoppingCart)
                {
                    // Get the product type of the cart item
                    var productType = cartItem.Product.ProductType;

                    Snackbar.Add($"Inside for - Product Type: {productType}");

                    // Check if the product type is "coffee"
                    if (productType.ToString().Equals("coffee", StringComparison.OrdinalIgnoreCase))
                    {
                        Snackbar.Add("TypeCoffee");

                        // If the member is found, increase the purchase count by the quantity in the cart
                        if (FoundMember != null)
                        {
                            Snackbar.Add("Found");
                            Snackbar.Add($"Current Purchase Count: {FoundMember.PurchasesCount}");

                            FoundMember.PurchasesCount += cartItem.Quantity;

                            //// Check if the member is a regular customer (daily purchases for a full month excluding weekends)
                            //UpdateRegularCustomerStatus(FoundMember);

                            if (FoundMember.IsRegularCustomer)
                            {
                                // Calculate the discount amount without modifying the original price
                                decimal discountAmount = CalculateDiscount(cartItem.Product.Price, 0.1m); // 10% discount

                                // Display the discount information
                                Snackbar.Add($"You've received a 10% discount on {cartItem.Product.Name}. Discount Amount: {discountAmount}");

                                // Accumulate the discount for the entire cart
                                totalDiscount += discountAmount;
                            }

                            // Calculate the number of complimentary drinks earned
                            int complimentaryDrinks = FoundMember.PurchasesCount / 10;

                            // Check if the member earned any complimentary drinks
                            if (complimentaryDrinks > 0)
                            {
                                // Redeem complimentary drinks
                                Snackbar.Add($"Congratulations! You've earned {complimentaryDrinks} free complimentary drink(s).", Severity.Success);

                                // Subtract the corresponding purchase count for the earned drinks
                                FoundMember.PurchasesCount -= complimentaryDrinks * 10;

                                Snackbar.Add($"Updated Purchase Count: {FoundMember.PurchasesCount}");
                            }

                            string totalDiscountString = FoundMember?.IsRegularCustomer == true ? "10%" : "-";

                            // Create a Transaction object
                            var transaction = new Transaction
                            {
                                MemberUsername = FoundMember != null ? FoundMember.UserName : "Regular Customer",
                                PurchaseDate = DateTime.Now,
                                ProductName = cartItem.Product.Name,
                                Quantity = cartItem.Quantity,
                                Discount = totalDiscountString, // Default discount for now, you can adjust this based on logic
                            };

                            // Add the transaction to the repository
                            TransactionRepository.Add(transaction);

                            if (FoundMember != null)
                            {
                                MemberRepository.Update(FoundMember);
                            }
                        }
                        else
                        {
                            // No member found, handle it accordingly (you can adjust this based on your requirements)
                            // For now, let's assume no discount for non-members
                            Snackbar.Add($"No member found. No discount applied for {cartItem.Product.Name}.");
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




        //private void UpdateRegularCustomerStatus(Member member)
        //{
        //    // Check if the member is already marked as a regular customer
        //    if (member.IsRegularCustomer)
        //    {
        //        // Skip the calculation if the member is already a regular customer
        //        return;
        //    }

        //    // Get the current date
        //    DateTime currentDate = DateTime.Now;

        //    // Get the start date for the month
        //    DateTime monthStartDate = new DateTime(currentDate.Year, currentDate.Month, 1);

        //    // Initialize counters for weekdays
        //    int totalWeekdays = 0;
        //    int purchasesOnWeekdays = 0;

        //    // Iterate through each day of the month
        //    for (DateTime date = monthStartDate; date < currentDate; date = date.AddDays(1))
        //    {
        //        // Check if the day is a weekday (Monday to Friday)
        //        if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday)
        //        {
        //            totalWeekdays++;

        //            // Check if any transactions were made on this weekday
        //            bool madePurchaseOnThisDay = TransactionRepository.GetAll()
        //                .Any(transaction => transaction.MemberUsername.Equals(member.UserName, StringComparison.OrdinalIgnoreCase)
        //                                    && transaction.PurchaseDate.Date == date.Date);

        //            if (madePurchaseOnThisDay)
        //            {
        //                purchasesOnWeekdays++;
        //            }
        //        }
        //    }

        //    // Update the IsRegularCustomer property based on the condition
        //    member.IsRegularCustomer = totalWeekdays > 0 && purchasesOnWeekdays == totalWeekdays;

        //    // Update the member in the repository
        //    MemberRepository.Update(member);
        //}

        private void UpdateRegularCustomerStatus(Member member)
        {
            // Check if the member is already marked as a regular customer
            if (member.IsRegularCustomer)
            {
                // Skip the calculation if the member is already a regular customer
                return;
            }

            // Get the current date
            DateTime currentDate = DateTime.Now;

            // Get the start date for the month
            DateTime monthStartDate = new DateTime(currentDate.Year, currentDate.Month, 1);

            // Check if any transactions were made in the current month
            bool madePurchaseThisMonth = TransactionRepository.GetAll()
                .Any(transaction => transaction.MemberUsername.Equals(member.UserName, StringComparison.OrdinalIgnoreCase)
                                    && transaction.PurchaseDate.Year == currentDate.Year
                                    && transaction.PurchaseDate.Month == currentDate.Month);

            // Update the IsRegularCustomer property based on the condition
            member.IsRegularCustomer = madePurchaseThisMonth;

            System.Diagnostics.Debug.WriteLine($"Member: {member}");


            // Update the member in the repository
            System.Diagnostics.Debug.WriteLine($"Going to update inside UpdateRegularCustomerStatus");

            MemberRepository.Update(member);
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

        private void SearchMember()
        {
            System.Diagnostics.Debug.WriteLine($"SearchMember called");
            // Check if MemberInput is a valid username or phone number
            FoundMember = validMember();

            System.Diagnostics.Debug.WriteLine($"Initial FoundMember: {FoundMember.UserName}");

            if (FoundMember != null)
            {
                Snackbar.Add($"Member found: {FoundMember.UserName}", Severity.Success);
                // Do something with the found member, if needed

                // Update regular customer status and set the isRegular variable
                UpdateRegularCustomerStatus(FoundMember);
                FoundMember = validMember();
                System.Diagnostics.Debug.WriteLine($"Second FoundMember: {FoundMember.UserName}");

                Snackbar.Add($"Is regular: {FoundMember.IsRegularCustomer}", Severity.Success);
                isRegular = FoundMember.IsRegularCustomer;
            }
            else
            {
                Snackbar.Add($"No member found with the given username or phone number.", Severity.Error);
                // Reset the isRegular variable when member is not found
                isRegular = false;
            }
        }

        private decimal CalculateDiscount(decimal originalPrice, decimal discountPercentage)
        {
            return originalPrice * discountPercentage;
        }

        private decimal CalculateTotal()
        {
            return ShoppingCart.Sum(cartItem => cartItem.Quantity * cartItem.Product.Price);
        }

        private decimal CalculateRegularCustomerDiscount()
        {
            // Define your regular customer discount logic here
            // For example, a flat 10% discount for regular customers
            return CalculateTotal() * 0.1m;
        }

        private decimal CalculateTotalWithDiscount()
        {
            // Calculate the total with regular customer discount
            return CalculateTotal() - CalculateRegularCustomerDiscount();
        }

        //private decimal CalculateTotalDiscount()
        //{
        //    decimal totalDiscount = 0;

        //    foreach (var cartItem in ShoppingCart)
        //    {
        //        // Check if the product type is "coffee"
        //        if (cartItem.Product.ProductType.ToString().Equals("coffee", StringComparison.OrdinalIgnoreCase))
        //        {
        //            // Check if the member is a regular customer
        //            if (FoundMember != null && FoundMember.IsRegularCustomer)
        //            {
        //                // Apply a flat 10% discount for regular customers
        //                decimal discountAmount = CalculateDiscount(cartItem.Product.Price, 0.1m); // 10% discount
        //                totalDiscount += discountAmount;
        //            }
        //        }
        //    }

        //    return totalDiscount;
        //}

        private Member validMember()
        {
            System.Diagnostics.Debug.WriteLine($"Valid member called");

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

                return FoundMember;
            }
            return null;
        }
    }

}

