using System;
using MathNet.Numerics.Differentiation;

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
        private bool MemberFound = false;
        private int RedemptionQuantity = 1;

        [CascadingParameter]
        private Action<string> SetAppBarTitle { get; set; }

        protected override void OnInitialized()
        {
            // Fetch products from the repository
            Products = (List<Product>)ProductRepository.GetAll();
            SetAppBarTitle.Invoke("Point of Sale");
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
                if (AtleastOneCoffee())
                {
                    decimal totalDiscount = 0;

                    // Iterate through cart items
                    foreach (var cartItem in ShoppingCart)
                    {
                        // Get the product type of the cart item
                        var productType = cartItem.Product.ProductType;

                        // Check if the product type is "coffee"
                        if (productType.ToString().Equals("coffee", StringComparison.OrdinalIgnoreCase))
                        {
                            // If there is a member, update the purchase count
                            if (FoundMember != null)
                            {
                                FoundMember.PurchasesCount += cartItem.Quantity;

                                if (FoundMember.IsRegularCustomer)
                                {
                                    // Calculate the discount amount without modifying the original price
                                    decimal discountAmount = CalculateDiscount(cartItem.Product.Price, 0.1m); // 10% discount

                                    // Display the discount information
                                    Snackbar.Add($"You've received a 10% discount on {cartItem.Product.Name}. Discount Amount: {discountAmount}");

                                    // Accumulate the discount for the entire cart
                                    totalDiscount += discountAmount;

                                    // Create a transaction for the cart item
                                    CreateTransaction(cartItem, FoundMember?.UserName ?? "-", 0.1m);
                                }
                                else
                                {
                                    // Create a transaction for the cart item without a discount
                                    CreateTransaction(cartItem, FoundMember?.UserName ?? "-", 0.0m);
                                }

                                // Calculate the number of complimentary drinks earned
                                int complimentaryDrinks = FoundMember.PurchasesCount / 10;

                                // Check if the member earned any complimentary drinks
                                if (complimentaryDrinks > 0)
                                {
                                    // Redeem complimentary drinks
                                    Snackbar.Add($"Congratulations! You've earned {complimentaryDrinks} free complimentary drink(s).", Severity.Success);

                                    FoundMember.FreeCoffeeRedemptionCount += complimentaryDrinks;

                                    // Subtract the corresponding purchase count for the earned drinks
                                    FoundMember.PurchasesCount -= complimentaryDrinks * 10;
                                }
                            }
                            else
                            {
                                // Create a transaction for the cart item
                                CreateTransaction(cartItem, FoundMember?.UserName ?? "-", 0.0m);
                            }




                            // Update the member in the repository
                            if (FoundMember != null)
                            {
                                MemberRepository.Update(FoundMember);
                            }
                        }
                        else
                        {
                            // Create a transaction for the cart item
                            CreateTransaction(cartItem, FoundMember?.UserName ?? "-", 0.0m);

                        }
                    }

                    // Clear the shopping cart after processing
                    ShoppingCart.Clear();

                    isRegular = false;
                    MemberFound = false;
                    MemberInput = "";

                    Snackbar.Add("Checkout successful.", Severity.Success);
                }
            }
            else
            {
                Snackbar.Add("Cannot checkout with an empty cart.", Severity.Error);
            }
        }

        private void CreateTransaction(CartItem cartItem, string memberUsername, decimal discountPercentage)
        {
            // Create a Transaction object
            var transaction = new Transaction
            {
                MemberUsername = memberUsername,
                PurchaseDate = DateTime.Now,
                ProductName = cartItem.Product.Name,
                ProductType = cartItem.Product.ProductType,
                Quantity = cartItem.Quantity,
                Discount = discountPercentage > 0.0m ? $"{discountPercentage * 100}%" : "-",
            };

            // Add the transaction to the repository
            TransactionRepository.Add(transaction);
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


        // Atleast one purchase per month
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
            isRegular = false;
            MemberRepository.Update(member);
        }

        // Per month excluding weekends regular member logic
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
        //    System.Diagnostics.Debug.WriteLine($"Going to update inside UpdateRegularCustomerStatus");

        //    MemberRepository.Update(member);
        //}


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

        private void ClearMember()
        {
            // Reset MemberFound to false
            MemberFound = false;

            // Clear the MemberInput
            MemberInput = "";

            // Clear the FoundMember
            FoundMember = null;
        }

        private void SearchMember()
        {
            System.Diagnostics.Debug.WriteLine($"SearchMember called");
            // Check if MemberInput is a valid username or phone number
            FoundMember = validMember();

            if (FoundMember != null)
            {
                Snackbar.Add($"Member found: {FoundMember.UserName}", Severity.Success);
                // Do something with the found member, if needed

                // Update regular customer status and set the isRegular variable
                UpdateRegularCustomerStatus(FoundMember);
                FoundMember = validMember();
                System.Diagnostics.Debug.WriteLine($"Second FoundMember: {FoundMember.UserName}");

                MemberFound = true;

                Snackbar.Add($"Is regular: {FoundMember.IsRegularCustomer}", Severity.Success);
                isRegular = FoundMember.IsRegularCustomer;
            }
            else
            {
                Snackbar.Add($"No member found with the given username or phone number.", Severity.Error);
                // Reset the isRegular variable when member is not found
                isRegular = false;
                MemberFound = false;
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
                    member.UserName.Equals(MemberInput, StringComparison.OrdinalIgnoreCase) && member.IsValid);

                // If the member is not found by username, try to find by phone number
                if (FoundMember == null)
                {
                    FoundMember = MemberRepository.GetAll().FirstOrDefault(member =>
                        member.Phone.Equals(MemberInput, StringComparison.OrdinalIgnoreCase) && member.IsValid);

                }


                if (FoundMember == null)
                {
                    // Member not found or not valid, show Snackbar message
                    Snackbar.Add("Member not found or membership not valid. Renew membership.", Severity.Error);
                }

                return FoundMember;
            }
            return null;
        }

        private bool AtleastOneCoffee()
        {
            bool atleast = false;

            if (ShoppingCart.Count > 0)
            {
                foreach (var cartItem in ShoppingCart)
                {
                    // Assuming ProductType is a string property in the Product class
                    if (cartItem.Product.ProductType.ToString().Equals("coffee", StringComparison.OrdinalIgnoreCase))
                    {
                        atleast = true;
                        break; // No need to continue checking once we find a coffee product
                    }
                }
            }
            if (!atleast)
            {
                // Show Snackbar message if no coffee in the cart
                Snackbar.Add("Please add at least one coffee to the cart.", Severity.Error);
            }

            return atleast;
        }

        private void RedeemFreeCoffee()
        {
            try
            {

                decimal totalDiscount = 0.0m;

                // Check if the inputted quantity is valid
                if (RedemptionQuantity > 0 && RedemptionQuantity <= FoundMember.FreeCoffeeRedemptionCount)
                {
                    // Check the number of "Coffee" products in the cart
                    int coffeeCountInCart = ShoppingCart.Where(cartItem =>
         cartItem.Product.ProductType.ToString().Equals("coffee", StringComparison.OrdinalIgnoreCase))
         .Sum(cartItem => cartItem.Quantity);

                    // Check if the inputted quantity is less than or equal to the number of "Coffee" products in the cart
                    if (RedemptionQuantity <= coffeeCountInCart)
                    {
                        int oldCount = RedemptionQuantity;

                        var cartCopy = new List<CartItem>(ShoppingCart);
                        // Iterate through cart items
                        foreach (var cartItem in cartCopy)
                        {
                            // Check if the product type is "coffee"
                            if (cartItem.Product.ProductType.ToString().Equals("coffee", StringComparison.OrdinalIgnoreCase))
                            {
                                // Apply a 100% discount for the specified quantity of free coffee
                                if (RedemptionQuantity > 0)
                                {
                                    decimal discountPercentage = 1.0m; // 100% discount
                                    int oldCart = cartItem.Quantity;
                                    cartItem.Quantity = RedemptionQuantity;
                                    CreateTransaction(cartItem, FoundMember?.UserName ?? "-", discountPercentage);
                                    cartItem.Quantity = oldCart - RedemptionQuantity;
                                    RedemptionQuantity = 0; // Reset RedemptionQuantity after applying the discount
                                }
                                if (cartItem.Quantity <= 0)
                                {
                                    // If quantity is 1 or less, remove the item from the cart
                                    ShoppingCart.Remove(cartItem);

                                }
                            }
                            else
                            {
                                // For other coffee products, apply the regular discount logic
                                if (FoundMember.IsRegularCustomer)
                                {
                                    decimal discountAmount = CalculateDiscount(cartItem.Product.Price, 0.1m); // 10% 

                                    CreateTransaction(cartItem, FoundMember?.UserName ?? "-", 0.1m);

                                    totalDiscount += discountAmount;

                                    cartItem.Quantity = 0;
                                    ShoppingCart.Remove(cartItem);


                                }

                                else
                                {
                                    CreateTransaction(cartItem, FoundMember?.UserName ?? "-", 0.0m);
                                }
                            }

                        }

                        // Update the FreeCoffeeRedemptionCount
                        FoundMember.FreeCoffeeRedemptionCount -= oldCount;

                        Snackbar.Add($"FreeCount: {FoundMember.FreeCoffeeRedemptionCount}");

                        // Update the member in the repository
                        if (FoundMember != null)
                        {
                            MemberRepository.Update(FoundMember);
                        }



                        // Display success message
                        Snackbar.Add($"Successfully redeemed {RedemptionQuantity} free coffee(s).", Severity.Success);

                        // Clear the RedemptionQuantity after processing
                        RedemptionQuantity = 1;

                        SearchMember();

                        if (coffeeCountInCart == oldCount)
                        {
                            // Clear the shopping cart after processing
                            ShoppingCart.Clear();

                            isRegular = false;
                            MemberFound = false;
                            MemberInput = "";
                        }
                    }
                    else
                    {
                        // Display error message if the inputted quantity exceeds the number of "Coffee" products in the cart
                        Snackbar.Add("Cannot redeem more free coffee than the number of 'Coffee' products in the cart.", Severity.Error);
                    }
                }
                else
                {
                    // Display error message if the inputted quantity is invalid
                    Snackbar.Add("Invalid quantity for free coffee redemption.", Severity.Error);
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Error: {ex}", severity: Severity.Error);
            }
        }



    }

}

