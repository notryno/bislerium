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
        private IEnumerable<Transaction> CoffeeTypeElements;
        private IEnumerable<Transaction> CoffeeAddonsElements;

        [CascadingParameter]
        private Action<string> SetAppBarTitle { get; set; }

        protected sealed override void OnInitialized()
        {
            try
            {
                SetAppBarTitle.Invoke("Manage Products");
                CoffeeTypeElements = TransactionRepository.GetAll();
                CoffeeAddonsElements = TransactionRepository.GetAll();

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
        private decimal GetQuantity(string productName)
        {
            var transactions = TransactionRepository.GetAll().Where(t => t.ProductName == productName);
            return transactions.Sum(t => t.Quantity);
        }

        private decimal GetRevenue(string productName)
        {
            var product = ProductRepository.GetAll().FirstOrDefault(p => p.Name == productName);
            if (product != null)
            {
                var transactions = TransactionRepository.GetAll().Where(t => t.ProductName == productName);
                return transactions.Sum(t => t.Quantity * product.Price);
            }

            return 0;
        }
    }
}
