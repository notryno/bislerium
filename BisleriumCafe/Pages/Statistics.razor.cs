using QuestPDF.Fluent;

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
        private string SearchCoffeeString;
        private string SearchAddOnString;

        private string selectedCoffeePeriod { get; set; } = "Daily";
        private string selectedAddOnPeriod { get; set; } = "Daily";
        private int monthNumber { get; set; } = 1;

        [CascadingParameter]
        private Action<string> SetAppBarTitle { get; set; }


        private IEnumerable<TopCoffeeData> Top5CoffeeData;
        private IEnumerable<TopAddOnData> Top5AddOnData;
        private IEnumerable<TopCoffeeData> Top5CoffeeDataDaily;
        private IEnumerable<TopAddOnData> Top5AddOnDataDaily;
        private readonly Dictionary<Guid, bool> CoffeeDescTracks = new();
        private readonly Dictionary<Guid, bool> AddOnDescTracks = new();

        protected sealed override void OnInitialized()
        {
            SetAppBarTitle.Invoke("Statistics");
                LoadTop5CoffeeDataDaily();
            LoadTop5AddOnDataDaily();
                LoadTop5CoffeeData();
                LoadTop5AddOnData();
            
        }

        private ICollection<Transaction> GetTransactions()
        {
            //return AuthService.IsUserAdmin()
            //    ? TransactionRepository.GetAll()
            //    : TransactionRepository.GetAll().Where(x => x.CreatedBy == AuthService.CurrentUser.Id).ToList();
            return TransactionRepository.GetAll();
        }

        private ICollection<Product> GetProducts()
        {
            return ProductRepository.GetAll();
        }

        private void LoadTop5CoffeeData()
        {
            DateTime currentDate = DateTime.Now;

            var products = GetProducts();

            var coffeeTransactions = GetTransactions()
                .Where(x => x.ProductType.ToString().Equals("Coffee", StringComparison.OrdinalIgnoreCase) &&
                    x.PurchaseDate.Year == currentDate.Year &&
                    x.PurchaseDate.Month == monthNumber);

            var topCoffeeData = coffeeTransactions
                .GroupBy(x => x.ProductName, StringComparer.OrdinalIgnoreCase)
                .Select(group => new TopCoffeeData
                {
                    ProductID = products.FirstOrDefault(p => p.Name.Equals(group.Key, StringComparison.OrdinalIgnoreCase))?.Id ?? Guid.Empty,
                    ProductName = group.Key,
                    ProductDescription = products.FirstOrDefault(p => p.Name.Equals(group.Key, StringComparison.OrdinalIgnoreCase))?.Description,
                    TotalQuantity = group.Sum(x => x.Quantity),
                    UnitPrice = (decimal)(products.FirstOrDefault(p => p.Name.Equals(group.Key, StringComparison.OrdinalIgnoreCase))?.Price),
                    TotalRevenue = group.Sum(x => CalculateRevenue(x))
                })
                .OrderByDescending(x => x.TotalQuantity)
                .Take(5)
                .ToList();

            Top5CoffeeData = topCoffeeData;



            foreach (TopCoffeeData s in Top5CoffeeData)
            {
                if (!CoffeeDescTracks.ContainsKey(s.ProductID))
                {
                    CoffeeDescTracks.Add(s.ProductID, false);
                }
            }
        }

        private void LoadTop5AddOnData()
        {
            DateTime currentDate = DateTime.Now;

            var products = GetProducts();

            var AddOnTransactions = GetTransactions()
                .Where(x => x.ProductType.ToString().Equals("AddOn", StringComparison.OrdinalIgnoreCase) &&
                    x.PurchaseDate.Year == currentDate.Year &&
                    x.PurchaseDate.Month == monthNumber);

            var topAddOnData = AddOnTransactions
                .GroupBy(x => x.ProductName, StringComparer.OrdinalIgnoreCase)
                .Select(group => new TopAddOnData
                {
                    ProductID = products.FirstOrDefault(p => p.Name.Equals(group.Key, StringComparison.OrdinalIgnoreCase))?.Id ?? Guid.Empty,
                    ProductName = group.Key,
                    ProductDescription = products.FirstOrDefault(p => p.Name.Equals(group.Key, StringComparison.OrdinalIgnoreCase))?.Description,
                    TotalQuantity = group.Sum(x => x.Quantity),
                    UnitPrice = (decimal)(products.FirstOrDefault(p => p.Name.Equals(group.Key, StringComparison.OrdinalIgnoreCase))?.Price),
                    TotalRevenue = group.Sum(x => CalculateRevenue(x))
                })
                .OrderByDescending(x => x.TotalQuantity)
                .Take(5)
                .ToList();

            Top5AddOnData = topAddOnData;

            foreach (TopAddOnData s in Top5AddOnData)
            {
                if (!AddOnDescTracks.ContainsKey(s.ProductID))
                {
                    AddOnDescTracks.Add(s.ProductID, false);
                }
            }
        }

        private void LoadTop5CoffeeDataDaily()
        {
            try { 
            DateTime currentDate = DateTime.Now;

            var products = GetProducts();

            var coffeeTransactions = GetTransactions()
                .Where(x => x.ProductType.ToString().Equals("Coffee", StringComparison.OrdinalIgnoreCase) &&
                    x.PurchaseDate.Year == currentDate.Year &&
                    x.PurchaseDate.Month == currentDate.Month &&
                    x.PurchaseDate.Day == currentDate.Day);

            var topCoffeeData = coffeeTransactions
                .GroupBy(x => x.ProductName, StringComparer.OrdinalIgnoreCase)
                .Select(group => new TopCoffeeData
                {
                    ProductID = products.FirstOrDefault(p => p.Name.Equals(group.Key, StringComparison.OrdinalIgnoreCase))?.Id ?? Guid.Empty,
                    ProductName = group.Key,
                    ProductDescription = products.FirstOrDefault(p => p.Name.Equals(group.Key, StringComparison.OrdinalIgnoreCase))?.Description,
                    TotalQuantity = group.Sum(x => x.Quantity),
                    UnitPrice = (decimal)(products.FirstOrDefault(p => p.Name.Equals(group.Key, StringComparison.OrdinalIgnoreCase))?.Price),
                    TotalRevenue = group.Sum(x => CalculateRevenue(x))
                })
                .OrderByDescending(x => x.TotalQuantity)
                .Take(5)
                .ToList();

            Top5CoffeeDataDaily = topCoffeeData;

            foreach (TopCoffeeData s in Top5CoffeeDataDaily)
            {
                if (!CoffeeDescTracks.ContainsKey(s.ProductID))
                {
                    CoffeeDescTracks.Add(s.ProductID, false);
                }
            }
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Error in SearchMonth: {ex.Message}", Severity.Error);
            }
        }

        private void LoadTop5AddOnDataDaily()
        {
            try
            {
                DateTime currentDate = DateTime.Now;

                var products = GetProducts();

                var AddOnTransactions = GetTransactions()
                    .Where(x => x.ProductType.ToString().Equals("AddOn", StringComparison.OrdinalIgnoreCase) &&
                        x.PurchaseDate.Year == currentDate.Year &&
                        x.PurchaseDate.Month == currentDate.Month &&
                        x.PurchaseDate.Day == currentDate.Day);

                var topAddOnData = AddOnTransactions
                    .GroupBy(x => x.ProductName, StringComparer.OrdinalIgnoreCase)
                    .Select(group => new TopAddOnData
                    {
                        ProductID = products.FirstOrDefault(p => p.Name.Equals(group.Key, StringComparison.OrdinalIgnoreCase))?.Id ?? Guid.Empty,
                        ProductName = group.Key,
                        ProductDescription = products.FirstOrDefault(p => p.Name.Equals(group.Key, StringComparison.OrdinalIgnoreCase))?.Description,
                        TotalQuantity = group.Sum(x => x.Quantity),
                        UnitPrice = (decimal)(products.FirstOrDefault(p => p.Name.Equals(group.Key, StringComparison.OrdinalIgnoreCase))?.Price),
                        TotalRevenue = group.Sum(x => CalculateRevenue(x))
                    })
                    .OrderByDescending(x => x.TotalQuantity)
                    .Take(5)
                    .ToList();

                Top5AddOnDataDaily = topAddOnData;

                foreach (TopAddOnData s in Top5AddOnDataDaily)
                {
                    if (!AddOnDescTracks.ContainsKey(s.ProductID))
                    {
                        AddOnDescTracks.Add(s.ProductID, false);
                    }
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Error in SearchMonth: {ex.Message}", Severity.Error);
            }
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
            try
            {
                Snackbar.Add(@DateTimeFormatInfo.CurrentInfo.GetMonthName(monthNumber).ToString(), Severity.Info);
                LoadTop5CoffeeData();
                LoadTop5AddOnData();
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Error in SearchMonth: {ex.Message}", Severity.Error);
            }

        }

        private bool CoffeeFilterFunc(TopCoffeeData element)
        {
            string searchStringLower = SearchCoffeeString?.ToLower() ?? string.Empty;

            return string.IsNullOrWhiteSpace(SearchCoffeeString)
                   || element.ProductID.ToString().Contains(SearchCoffeeString, StringComparison.OrdinalIgnoreCase)
                   || element.ProductName.Contains(SearchCoffeeString, StringComparison.OrdinalIgnoreCase)
                   || element.TotalQuantity.ToString().Contains(searchStringLower, StringComparison.OrdinalIgnoreCase)
                   || element.TotalRevenue.ToString().Contains(searchStringLower, StringComparison.OrdinalIgnoreCase);
        }

        private bool AddOnFilterFunc(TopAddOnData element)
        {
            string searchStringLower = SearchAddOnString?.ToLower() ?? string.Empty;

            return string.IsNullOrWhiteSpace(SearchAddOnString)
                   || element.ProductID.ToString().Contains(SearchAddOnString, StringComparison.OrdinalIgnoreCase)
                   || element.ProductName.Contains(SearchAddOnString, StringComparison.OrdinalIgnoreCase)
                   || element.TotalQuantity.ToString().Contains(searchStringLower, StringComparison.OrdinalIgnoreCase)
                   || element.TotalRevenue.ToString().Contains(searchStringLower, StringComparison.OrdinalIgnoreCase);
        }


        private class TopCoffeeData
        {
            public Guid ProductID { get; set; }
            public string ProductName { get; set; }
            public string ProductDescription { get; set; }
            public int TotalQuantity { get; set; }
            public decimal TotalRevenue { get; set; }
            public decimal UnitPrice { get; set; }
        }

        private class TopAddOnData
        {
            public Guid ProductID { get; set; }
            public string ProductName { get; set; }
            public string ProductDescription { get; set; }
            public int TotalQuantity { get; set; }
            public decimal TotalRevenue { get; set; }
            public decimal UnitPrice { get; set; }
        }

        private void ShowBtnPress(Guid id)
        {
            CoffeeDescTracks[id] = !CoffeeDescTracks[id];
        }

        private bool GetShow(Guid id)
        {
            return CoffeeDescTracks.ContainsKey(id) ? CoffeeDescTracks[id] : (CoffeeDescTracks[id] = false);
        }

        private void ShowAddOnBtnPress(Guid id)
        {
            AddOnDescTracks[id] = !AddOnDescTracks[id];
        }

        private bool GetAddOnShow(Guid id)
        {
            return AddOnDescTracks.ContainsKey(id) ? AddOnDescTracks[id] : (AddOnDescTracks[id] = false);
        }

        private void GenerateCoffeePdf()
        {
            try
            {

                var directoryPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                var fileName = @"top5coffee.pdf";
                var fullPath = Path.Combine(directoryPath, fileName);

                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(50);

                        page.Header().Element(ComposeCoffeeHeader);

                        page.Content().Element(ComposeCoffeeContent);



                        page.Footer()
                            .AlignCenter()
                            .Text(x =>
                            {
                                x.CurrentPageNumber();
                                x.Span("/");
                                x.TotalPages();
                            });
                    });
                })

                .GeneratePdf(fullPath);

                Snackbar.Add($"PDF Generated at: ${fullPath}", Severity.Success);
            }
            catch (Exception ex)
            {
                // Handle the exception here
                Snackbar.Add($"An error occurred: {ex.Message}", Severity.Error);
            }
        }

        void ComposeCoffeeContent(QuestPDF.Infrastructure.IContainer container)
        {
            container.PaddingVertical(40).Column(column =>
            {
                column.Spacing(5);

                column.Item().Element(ComposeTable);
                var totalPrice = 0.0;

                if (string.Equals(selectedCoffeePeriod, "Monthly", StringComparison.OrdinalIgnoreCase))
                {
                    totalPrice = (double)Top5CoffeeData.Sum(x => x.TotalQuantity * x.UnitPrice);
                } else
                {
                    totalPrice = (double)Top5CoffeeDataDaily.Sum(x => x.TotalQuantity * x.UnitPrice);
                }
                column.Item().AlignRight().Text($"Grand total: NPR {totalPrice}").FontSize(14);

            });
        }

        void ComposeCoffeeHeader(QuestPDF.Infrastructure.IContainer container)
        {
            var titleStyle = TextStyle.Default.FontSize(20).SemiBold().FontColor(QuestPDF.Helpers.Colors.Brown.Medium);
            var currentDate = DateTime.Today;

            container.Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text($"Top Five Coffee Purchases").Style(titleStyle);

                    column.Item().Text(text =>
                    {
                        text.Span("Generation date: ").SemiBold();
                        text.Span($"{currentDate:d}");
                    });

                    column.Item().Text(text =>
                    {
                        text.Span("Product Type: ").SemiBold();
                        text.Span($"Coffee");
                    });

                    column.Item().Text(text =>
                    {
                        text.Span("Report Type: ").SemiBold();
                        if (string.Equals(selectedCoffeePeriod, "Daily", StringComparison.OrdinalIgnoreCase))
                        {
                            text.Span($"Daily");
                        }
                        else
                        {
                            text.Span($"Monthly");
                        }
                    });
                });

                var imagePath = Path.Combine(AppContext.BaseDirectory, "..", "Resources", "logoBG.png");
                var logo = QuestPDF.Infrastructure.Image.FromFile(imagePath);
                Snackbar.Add($"{imagePath}");
                row.ConstantItem(100).Height(100).Width(100).Image(logo);

            });
        }

        void ComposeTable(QuestPDF.Infrastructure.IContainer container)
        {
            int serialNumber = 1;
            container.Table(table =>
            {
                // step 1
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(25);
                    columns.RelativeColumn(3);
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                });

                // step 2
                table.Header(header =>
                {
                    header.Cell().Element(CellStyle).Text("#");
                    header.Cell().Element(CellStyle).Text("Product");
                    header.Cell().Element(CellStyle).AlignRight().Text("Unit price");
                    header.Cell().Element(CellStyle).AlignRight().Text("Quantity");
                    header.Cell().Element(CellStyle).AlignRight().Text("Total");

                    static QuestPDF.Infrastructure.IContainer CellStyle(QuestPDF.Infrastructure.IContainer container)
                    {
                        return container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(QuestPDF.Helpers.Colors.Black);
                    }
                });

                // step 3
                if (string.Equals(selectedCoffeePeriod, "Monthly", StringComparison.OrdinalIgnoreCase)) {
                    foreach (TopCoffeeData item in Top5CoffeeData)
                    {
                        table.Cell().Element(CellStyle).Text(serialNumber);
                        table.Cell().Element(CellStyle).Text(item.ProductName);
                        table.Cell().Element(CellStyle).AlignRight().Text($"{item.UnitPrice}");
                        table.Cell().Element(CellStyle).AlignRight().Text($"{item.TotalQuantity}");
                        table.Cell().Element(CellStyle).AlignRight().Text($"NPR {item.TotalRevenue}");
                        static QuestPDF.Infrastructure.IContainer CellStyle(QuestPDF.Infrastructure.IContainer container)
                        {
                            return container.BorderBottom(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten2).PaddingVertical(5);
                        }

                        serialNumber++;
                    }
                } else
                {
                    foreach (TopCoffeeData item in Top5CoffeeDataDaily)
                    {
                        table.Cell().Element(CellStyle).Text(serialNumber);
                        table.Cell().Element(CellStyle).Text(item.ProductName);
                        table.Cell().Element(CellStyle).AlignRight().Text($"{item.UnitPrice}");
                        table.Cell().Element(CellStyle).AlignRight().Text($"{item.TotalQuantity}");
                        table.Cell().Element(CellStyle).AlignRight().Text($"NPR {item.TotalRevenue}");
                        static QuestPDF.Infrastructure.IContainer CellStyle(QuestPDF.Infrastructure.IContainer container)
                        {
                            return container.BorderBottom(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten2).PaddingVertical(5);
                        }

                        serialNumber++;
                    }
                }
                
            });
        }



        private void GenerateAddOnPdf()
        {
            try
            {
                //var topCoffeeData = Top5CoffeeData.ToList(); // Assuming Top5CoffeeData is your data collection
                var directoryPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                var fileName = @"top5addon.pdf";
                var fullPath = Path.Combine(directoryPath, fileName);

                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(50);
                        //page.PageColor(QuestPDF.Helpers.Colors.White);
                        //page.DefaultTextStyle(x => x.FontSize(20));

                        //page.Header()
                        //    .Text("Top Five Coffee Sales")
                        //    .SemiBold().FontSize(36).FontColor(QuestPDF.Helpers.Colors.Blue.Medium);
                        page.Header().Element(ComposeAddOnHeader);

                        page.Content().Element(ComposeAddOnContent);
                        //.Column(x =>
                        //{
                        //    x.Spacing(20);

                        //    foreach (var coffeeData in topCoffeeData)
                        //    {
                        //        x.Item().Text($"Product Name: {coffeeData.ProductName} \n Total Quantity: {coffeeData.TotalQuantity} \n Total Revenue: ${coffeeData.TotalRevenue}");
                        //        //x.Item(item =>
                        //        //{
                        //        //    item.Text($"Product Name: {coffeeData.ProductName}");
                        //        //    item.Text($"Total Quantity: {coffeeData.TotalQuantity}");
                        //        //    item.Text($"Total Revenue: {coffeeData.TotalRevenue}");
                        //        //});
                        //    }
                        //});



                        page.Footer()
                            .AlignCenter()
                            .Text(x =>
                            {
                                x.CurrentPageNumber();
                                x.Span("/");
                                x.TotalPages();
                            });
                    });
                })

                .GeneratePdf(fullPath);

                Snackbar.Add($"PDF Generated at: ${fullPath}", Severity.Success);
            }
            catch (Exception ex)
            {
                // Handle the exception here
                Snackbar.Add($"An error occurred: {ex.Message}", Severity.Error);
            }
        }

        void ComposeAddOnContent(QuestPDF.Infrastructure.IContainer container)
        {
            container.PaddingVertical(40).Column(column =>
            {
                column.Spacing(5);

                column.Item().Element(ComposeAddOnTable);
                var totalPrice = 0.0;
                if (string.Equals(selectedAddOnPeriod, "Daily", StringComparison.OrdinalIgnoreCase))
                {
                    totalPrice = (double)Top5AddOnDataDaily.Sum(x => x.TotalQuantity * x.UnitPrice);
                } else
                {
                    totalPrice = (double)Top5AddOnData.Sum(x => x.TotalQuantity * x.UnitPrice);
                }
                    
                column.Item().AlignRight().Text($"Grand total: NPR {totalPrice}").FontSize(14);

            });
        }

        void ComposeAddOnHeader(QuestPDF.Infrastructure.IContainer container)
        {
            var titleStyle = TextStyle.Default.FontSize(20).SemiBold().FontColor(QuestPDF.Helpers.Colors.Brown.Medium);
            var currentDate = DateTime.Today;

            container.Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text($"Top Five Add-Ons Purchases").Style(titleStyle);

                    column.Item().Text(text =>
                    {
                        text.Span("Generation date: ").SemiBold();
                        text.Span($"{currentDate:d}");
                    });

                    column.Item().Text(text =>
                    {
                        text.Span("Product Type: ").SemiBold();
                        text.Span($"Add-Ons");
                    });

                    column.Item().Text(text =>
                    {
                        text.Span("Report Type: ").SemiBold();
                        if (string.Equals(selectedAddOnPeriod, "Daily", StringComparison.OrdinalIgnoreCase))
                        {
                            text.Span($"Daily");
                        }
                        else
                        {
                            text.Span($"Monthly");
                        }
                    });
                });

                var imagePath = Path.Combine(AppContext.BaseDirectory, "..", "Resources", "logoBG.png");
                var logo = QuestPDF.Infrastructure.Image.FromFile(imagePath);
                Snackbar.Add($"{imagePath}");
                row.ConstantItem(100).Height(100).Width(100).Image(logo);

            });
        }

        void ComposeAddOnTable(QuestPDF.Infrastructure.IContainer container)
        {
            int serialNumber = 1;
            container.Table(table =>
            {
                // step 1
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(25);
                    columns.RelativeColumn(3);
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                });

                // step 2
                table.Header(header =>
                {
                    header.Cell().Element(CellStyle).Text("#");
                    header.Cell().Element(CellStyle).Text("Product");
                    header.Cell().Element(CellStyle).AlignRight().Text("Unit price");
                    header.Cell().Element(CellStyle).AlignRight().Text("Quantity");
                    header.Cell().Element(CellStyle).AlignRight().Text("Total");

                    static QuestPDF.Infrastructure.IContainer CellStyle(QuestPDF.Infrastructure.IContainer container)
                    {
                        return container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(QuestPDF.Helpers.Colors.Black);
                    }
                });

                // step 3
                if (string.Equals(selectedAddOnPeriod, "Daily", StringComparison.OrdinalIgnoreCase))
                {
                    foreach (TopAddOnData item in Top5AddOnDataDaily)
                    {
                        table.Cell().Element(CellStyle).Text(serialNumber);
                        table.Cell().Element(CellStyle).Text(item.ProductName);
                        table.Cell().Element(CellStyle).AlignRight().Text($"{item.UnitPrice}");
                        table.Cell().Element(CellStyle).AlignRight().Text($"{item.TotalQuantity}");
                        table.Cell().Element(CellStyle).AlignRight().Text($"NPR {item.TotalRevenue}");
                        static QuestPDF.Infrastructure.IContainer CellStyle(QuestPDF.Infrastructure.IContainer container)
                        {
                            return container.BorderBottom(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten2).PaddingVertical(5);
                        }

                        serialNumber++;
                    }
                } else {
                    foreach (TopAddOnData item in Top5AddOnData)
                    {
                        table.Cell().Element(CellStyle).Text(serialNumber);
                        table.Cell().Element(CellStyle).Text(item.ProductName);
                        table.Cell().Element(CellStyle).AlignRight().Text($"{item.UnitPrice}");
                        table.Cell().Element(CellStyle).AlignRight().Text($"{item.TotalQuantity}");
                        table.Cell().Element(CellStyle).AlignRight().Text($"NPR {item.TotalRevenue}");
                        static QuestPDF.Infrastructure.IContainer CellStyle(QuestPDF.Infrastructure.IContainer container)
                        {
                            return container.BorderBottom(1).BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten2).PaddingVertical(5);
                        }

                        serialNumber++;
                    }
                }
                    
            });
        }
    }
}
