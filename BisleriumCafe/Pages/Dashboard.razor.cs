namespace BisleriumCafe.Pages;

public partial class Dashboard
{
    public const string Route = "/dashboard";

    private BarChartConfig Config;

    [CascadingParameter]
    private Action<string> SetAppBarTitle { get; set; }

    protected sealed override void OnInitialized()
    {
        SetAppBarTitle.Invoke("Dashboard");
        PSC.Blazor.Components.Chartjs.Models.Common.Font axisLabelFont = new()
        {
            Weight = "bold",
            Size = 16
        };

        Config = new BarChartConfig()
        {
            Options = new Options()
            {
                Responsive = true,
                Plugins = new Plugins()
                {
                    Crosshair = new Crosshair()
                    {
                        Horizontal = new CrosshairLine()
                        {
                            Color = "rgb(255, 99, 132)"
                        }
                    },
                    Zoom = new Zoom()
                    {
                        Enabled = true,
                        Mode = "xy",
                        ZoomOptions = new ZoomOptions()
                        {
                            Wheel = new Wheel()
                            {
                                Enabled = true
                            },
                            Pinch = new Pinch()
                            {
                                Enabled = true
                            },
                        }
                    },
                    Title = new Title()
                    {
                        Text = "Transaction Quantities",
                        Display = true,
                        Font = new PSC.Blazor.Components.Chartjs.Models.Common.Font()
                        {
                            Weight = "bold",
                            Size = 20
                        },
                        Position = PSC.Blazor.Components.Chartjs.Models.Common.Position.Top
                    },
                },
                Scales = new Dictionary<string, Axis>()
                {
                    {
                        Scales.XAxisId, new Axis()
                        {
                            Stacked = true,
                            Ticks = new Ticks()
                            {
                                MaxRotation = 45,
                                MinRotation = 0
                            },
                            Title = new AxesTitle()
                            {
                                Text = "Products",
                                Display = true,
                                Align = PSC.Blazor.Components.Chartjs.Models.Common.Align.Center,
                                Font = axisLabelFont
                            },
                        }
                    },
                    {
                        Scales.YAxisId, new Axis()
                        {
                            Stacked = true,
                            Title = new AxesTitle()
                            {
                                Text = "Quantity",
                                Display = true,
                                Align = PSC.Blazor.Components.Chartjs.Models.Common.Align.Center,
                                Font = axisLabelFont
                            },
                        }
                    }
                }
            }
        };

        BarDataset coffeeQuantitySet = new()
        {
            Data = new List<decimal?>(),
            BackgroundColor = new() { "rgb(0, 163, 68)" },
            Label = "Coffee",
        };

        BarDataset addonQuantitySet = new()
        {
            Data = new List<decimal?>(),
            BackgroundColor = new() { "rgb(252, 152, 0)" },
            Label = "Add-Ons"
        };


        // Fetch data from TransactionRepository
        var transactions = TransactionRepository.GetAll();

        var productNames = transactions.Select(x => x.ProductName).Distinct();

        foreach (var productName in productNames)
        {
            var productTransactions = transactions
                .Where(x => x.ProductName == productName && x.Quantity > 0)
                .ToList();

            // Sum of quantities for each product
            int totalQuantity = productTransactions.Sum(x => x.Quantity);

            Config.Data.Labels.Add(productName);

            // Check the ProductType to distinguish between Coffee and Add-Ons
            var productType = productTransactions.First().ProductType;

            if (productType == ProductType.Coffee)
            {
                coffeeQuantitySet.Data.Add(totalQuantity);
                addonQuantitySet.Data.Add(null); // Add null for Add-Ons to maintain dataset alignment
            }
            else if (productType == ProductType.AddOn)
            {
                addonQuantitySet.Data.Add(totalQuantity);
                coffeeQuantitySet.Data.Add(null); // Add null for Coffee to maintain dataset alignment
            }
        }

        Config.Data.Datasets.Add(coffeeQuantitySet);
        Config.Data.Datasets.Add(addonQuantitySet);
    }
}
