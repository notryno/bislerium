namespace BisleriumCafe.Pages;

public partial class Products
{
    public const string Route = "/products";

    private readonly bool Dense = true;
    private readonly bool Fixed_header = true;
    private readonly bool Fixed_footer = true;
    private readonly bool Hover = true;
    private bool ReadOnly = false;
    private readonly bool CanCancelEdit = true;
    private readonly bool BlockSwitch = true;
    private string SearchString;
    private Product SelectedItem;
    private Product ElementBeforeEdit;
    private readonly TableApplyButtonPosition ApplyButtonPosition = TableApplyButtonPosition.End;
    private readonly TableEditButtonPosition EditButtonPosition = TableEditButtonPosition.End;
    private readonly TableEditTrigger EditTrigger = TableEditTrigger.RowClick;
    private IEnumerable<Product> Elements;
    private readonly Dictionary<Guid, bool> ProductDescTracks = new();

    [CascadingParameter]
    private Action<string> SetAppBarTitle { get; set; }

    protected sealed override void OnInitialized()
    {

        try
        {
            SetAppBarTitle.Invoke("Manage Products");
            Elements = ProductRepository.GetAll();
            
            if (!AuthService.IsUserAdmin())
            {
                ReadOnly = true;
            }
            foreach (Product s in Elements)
            {
                ProductDescTracks.Add(s.Id, false);
            }
            
        }
        
        catch (Exception ex)
    {
            Console.Error.WriteLine($"Error in OnInitialized: {ex.Message}");
            // Log or handle the exception as needed.
        }
    }

    private void BackupItem(object element)
    {
        try
        {
            ElementBeforeEdit = ((Product)element).Clone() as Product;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error in BackupItem: {ex.Message}");
            // Log or handle the exception as needed.
        }
    }

    private void ResetItemToOriginalValues(object element)
    {
        try
        {
            ((Product)element).Name = ElementBeforeEdit.Name;
            ((Product)element).Description = ElementBeforeEdit.Description;
            ((Product)element).ProductType = ElementBeforeEdit.ProductType; //Ryan
            ((Product)element).Price = ElementBeforeEdit.Price;
            //((Product)element).AvailableQuantity = ElementBeforeEdit.AvailableQuantity;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error in ResetItemToOriginalValues: {ex.Message}");
        }
    }

    private bool FilterFunc(Product element)
    {
        return string.IsNullOrWhiteSpace(SearchString)
               || element.Id.ToString().Contains(SearchString, StringComparison.OrdinalIgnoreCase)
               || element.Name.Contains(SearchString, StringComparison.OrdinalIgnoreCase)
               || element.Description.Contains(SearchString, StringComparison.OrdinalIgnoreCase)
               || element.ProductType.ToString().Contains(SearchString, StringComparison.OrdinalIgnoreCase) //Ryan
               || element.Price.ToString().Contains(SearchString, StringComparison.OrdinalIgnoreCase);
               //|| element.AvailableQuantity.ToString().Contains(SearchString, StringComparison.OrdinalIgnoreCase);
    }

    private void ShowBtnPress(Guid id)
    {
        ProductDescTracks[id] = !ProductDescTracks[id];
    }

    private bool GetShow(Guid id)
    {
        return ProductDescTracks.ContainsKey(id) ? ProductDescTracks[id] : (ProductDescTracks[id] = false);
    }

    private string GetLastTakenOut(Guid id)
    {
        List<ActivityLog> log = ActivityLogRepository.GetAll().Where(x => x.ProductID == id && x.Action == StockAction.Deduct && x.ApprovalStatus == ApprovalStatus.Approve).ToList();
        return log.Count == 0 ? "N/A" : log.Max(x => x.ApprovalStatusOn).ToString();
    }

    private async Task AddDialog()
    {
        DialogParameters parameters = new()
        {
            { "ChangeParentState", new Action(StateHasChanged) }
        };
        await DialogService.ShowAsync<AddProductDialog>("Add Product", parameters); //Ryan
    }

    //private async Task ActOnStock(Product product, StockAction action)
    //{
    //    if (action == StockAction.Deduct)
    //    {
    //        //if (!ApproveButton.ValidateWeekAndTime(Snackbar))
    //        //{
    //        //    return;
    //        //}

    //        if (product.AvailableQuantity == 0)
    //        {
    //            Snackbar.Add("Out of Stock!", Severity.Error);
    //            return;
    //        }
    //    }
    //    DialogParameters parameters = new()
    //    {
    //        { "StockAction", action },
    //        { "Product",  product},
    //        { "ChangeParentState", new Action(StateHasChanged) }
    //    };
    //    await DialogService.ShowAsync<StockActionDialog>($"{Enum.GetName(action)} Stock", parameters);
    //}
}