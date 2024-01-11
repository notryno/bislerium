using System;
using System.Collections.Generic;
using System.Linq;
using BisleriumCafe.Data.Models;
using BisleriumCafe.Data.Repositories;
using Microsoft.AspNetCore.Components;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Drawing;
using QuestPDF.Fluent;
//using QuestPDF.Common;

namespace BisleriumCafe.Pages
{
	public partial class Statistics
	{
		public const string Route = "/statistics";

		private readonly bool Dense = true;
		private readonly bool Fixed_header = true;
		private readonly bool Fixed_footer = true;
		private readonly bool Hover = true;
		private readonly bool ReadOnly = false;

		private int monthNumber = DateTime.Now.Month;

		private Action<string> SetAppBarTitle { get; set; }
		private IEnumerable<TopCoffeeData> Top5CoffeeData;
		private IEnumerable<TopAddInData> Top5AddInData;

		protected sealed override void OnInitialized()
		{
			//SetAppBarTitle.Invoke("Top 5 Coffee Sales");
			LoadTop5CoffeeData();
			LoadTop5AddinData();

        }

		private ICollection<Transaction> GetTransactions()
		{
			//return AuthService.IsUserAdmin()
			//    ? TransactionRepository.GetAll()
			//    : TransactionRepository.GetAll().Where(x => x.CreatedBy == AuthService.CurrentUser.Id).ToList();
			return TransactionRepository.GetAll();
		}

		private void LoadTop5CoffeeData()
		{
            DateTime currentDate = DateTime.Now;

            var coffeeTransactions = GetTransactions()
                .Where(x => x.ProductType.ToString().Equals("Coffee", StringComparison.OrdinalIgnoreCase) &&
                    x.PurchaseDate.Year == currentDate.Year &&
                    x.PurchaseDate.Month == monthNumber);

            var topCoffeeData = coffeeTransactions
				.GroupBy(x => x.ProductName, StringComparer.OrdinalIgnoreCase)
				.Select(group => new TopCoffeeData
				{
					ProductName = group.Key,
					TotalQuantity = group.Sum(x => x.Quantity),
					TotalRevenue = group.Sum(x => CalculateRevenue(x))
                })
				.OrderByDescending(x => x.TotalQuantity)
				.Take(5)
				.ToList();

			Top5CoffeeData = topCoffeeData;
		}

        private void LoadTop5AddinData()
        {
            DateTime currentDate = DateTime.Now;

            var addinTransactions = GetTransactions()
                .Where(x => x.ProductType.ToString().Equals("AddOn", StringComparison.OrdinalIgnoreCase) &&
                    x.PurchaseDate.Year == currentDate.Year &&
                    x.PurchaseDate.Month == monthNumber);

            var topAddInData = addinTransactions
                .GroupBy(x => x.ProductName, StringComparer.OrdinalIgnoreCase)
                .Select(group => new TopAddInData
                {
                    ProductName = group.Key,
                    TotalQuantity = group.Sum(x => x.Quantity),
                    TotalRevenue = group.Sum(x => CalculateRevenue(x))
                })
                .OrderByDescending(x => x.TotalQuantity)
                .Take(5)
                .ToList();

            Top5AddInData = topAddInData;
        }

        private decimal CalculateDiscount(string discount)
        {
            if (string.IsNullOrEmpty(discount) || discount == "-")
            {
                return 0; // No discount or discount is represented as "-"
            }

            // Assume the discount string format is "10%" where '%' indicates a percentage discount
            if (discount.EndsWith("%") && decimal.TryParse(discount.TrimEnd('%'), out decimal percentageValue))
            {
                // Convert the percentage to a decimal value (e.g., 10% becomes 0.1)
                return percentageValue / 100;
            }

            return 0; // Default to no discount if format is not recognized
        }

        private decimal CalculateRevenue(Transaction transaction)
        {
            var product = GetProductByName(transaction.ProductName);

            if (product != null)
            {
                // Calculate revenue: (quantity from transaction) * (1 - discount percentage) * (price from product)
                return transaction.Quantity * (1 - CalculateDiscount(transaction.Discount)) * product.Price;
            }

            return 0; // Handle the case where the product is not found
        }


        private Product GetProductByName(string productName)
        {
            // Fetch the product from your repository based on the product name
            return ProductRepository.GetAll().FirstOrDefault(p => p.Name == productName);
            // Replace the example code with your actual repository call
        }

        private void SearchMonth()
		{
            

            Snackbar.Add(@DateTimeFormatInfo.CurrentInfo.GetMonthName(monthNumber).ToString(), Severity.Info);
            LoadTop5CoffeeData();
            LoadTop5AddinData();

        }


        private class TopCoffeeData
		{
			public string ProductName { get; set; }
			public int TotalQuantity { get; set; }
			public decimal TotalRevenue { get; set; }
		}

        private class TopAddInData
        {
            public string ProductName { get; set; }
            public int TotalQuantity { get; set; }
            public decimal TotalRevenue { get; set; }
        }

        private void GeneratePdfWithCustomData()
        {
            var topCoffeeData = Top5CoffeeData.ToList(); // Assuming Top5CoffeeData is your data collection
            var topAddInData = Top5AddInData.ToList(); // Assuming Top5CoffeeData is your data collection

            var directoryPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            var fileName = @"top5coffee.pdf";
            var fullPath = Path.Combine(directoryPath, fileName);

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(QuestPDF.Helpers.Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(20));

                    page.Header()
                        .Text("Top 5 Coffee Sales")
                        .SemiBold().FontSize(36).FontColor(QuestPDF.Helpers.Colors.Blue.Medium);

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(x =>
                        {
                            x.Spacing(20);

                            foreach (var coffeeData in topCoffeeData)
                            {
                                x.Item().Text($"Product Name: {coffeeData.ProductName} \n Total Quantity: {coffeeData.TotalQuantity} \n Total Revenue: ${coffeeData.TotalRevenue}");
                                //x.Item(item =>
                                //{
                                //    item.Text($"Product Name: {coffeeData.ProductName}");
                                //    item.Text($"Total Quantity: {coffeeData.TotalQuantity}");
                                //    item.Text($"Total Revenue: {coffeeData.TotalRevenue}");
                                //});
                            }
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Page ");
                            x.CurrentPageNumber();
                        });
                });
            })

            .GeneratePdf(fullPath);

           fileName = @"top5addins.pdf";
           fullPath = Path.Combine(directoryPath, fileName);

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(QuestPDF.Helpers.Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(20));

                    page.Header()
                        .Text("Top 5 AddIn Sales")
                        .SemiBold().FontSize(36).FontColor(QuestPDF.Helpers.Colors.Blue.Medium);

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(x =>
                        {
                            x.Spacing(20);

                            foreach (var addInData in topAddInData)
                            {
                                x.Item().Text($"Product Name: {addInData.ProductName} \n Total Quantity: {addInData.TotalQuantity} \n Total Revenue: ${addInData.TotalRevenue}");
                                //x.Item(item =>
                                //{
                                //    item.Text($"Product Name: {coffeeData.ProductName}");
                                //    item.Text($"Total Quantity: {coffeeData.TotalQuantity}");
                                //    item.Text($"Total Revenue: {coffeeData.TotalRevenue}");
                                //});
                            }
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Page ");
                            x.CurrentPageNumber();
                        });
                });
            })

            .GeneratePdf(fullPath);

            Snackbar.Add($"PDF Generated at: ${fullPath}", Severity.Success);
            Snackbar.Add($"PDF Generated at: ${fullPath}", Severity.Success);
        }
    }
}
