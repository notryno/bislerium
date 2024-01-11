using BisleriumCafe.Data.Repositories;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BisleriumCafe.Pages
{
    public partial class Statistics : ComponentBase
    {
        public const string Route = "/statistics";

        private bool showCoffeeTypeTable = true;


        private IEnumerable<SummedTransaction> SummedCoffeeTypeElements;
        private IEnumerable<SummedTransaction> SummedCoffeeAddonsElements;

        protected override void OnInitialized()
        {
            try
            {
                //SetAppBarTitle.Invoke("Manage Products");

                var transactions = TransactionRepository.GetAll();
                SummedCoffeeTypeElements = SumTransactions(transactions, "CoffeeType");
                SummedCoffeeAddonsElements = SumTransactions(transactions, "CoffeeAddons");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error in OnInitialized: {ex.Message}");
            }
        }

        private void ToggleTable()
        {
            showCoffeeTypeTable = !showCoffeeTypeTable;
        }
        private class SummedTransaction
        {
            public Guid Id { get; set; }
            public string ProductName { get; set; }
            public decimal Quantity { get; set; }
            public decimal Revenue { get; set; }
        }

        private IEnumerable<SummedTransaction> SumTransactions(IEnumerable<Transaction> transactions, string productType)
        {
            var groupedTransactions = transactions
                .Where(t => t.ProductType.ToString() == productType)
                .GroupBy(t => t.Id)
                .Select(group => new SummedTransaction
                {
                    Id = group.Key,
                    ProductName = group.First().ProductName,
                    Quantity = group.Sum(t => t.Quantity),
                    Revenue = group.Sum(t => t.Quantity * GetProductPrice(t.Id)),
                });

            return groupedTransactions;
        }

        private decimal GetProductPrice(Guid productId)
        {
            var product = ProductRepository.GetAll().FirstOrDefault(p => p.Id == productId);
            return product?.Price ?? 0;
        }

    }
}
