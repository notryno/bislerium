namespace BisleriumCafe.Pages;

public partial class Transactions
{
    public const string Route = "/transactions";

    private readonly bool Dense = true;
    private readonly bool Fixed_header = true;
    private readonly bool Fixed_footer = true;
    private readonly bool Hover = true;
    private bool ReadOnly = false;
    private readonly bool CanCancelEdit = true;
    private readonly bool BlockSwitch = true;
    private string SearchString;
    private Transaction SelectedTransaction;
    private Transaction ElementBeforeEdit;
    private readonly TableApplyButtonPosition ApplyButtonPosition = TableApplyButtonPosition.End;
    private readonly TableEditButtonPosition EditButtonPosition = TableEditButtonPosition.End;
    private readonly TableEditTrigger EditTrigger = TableEditTrigger.RowClick;
    private IEnumerable<Transaction> Elements;

    [CascadingParameter]
    private Action<string> SetAppBarTitle { get; set; }

    protected sealed override void OnInitialized()
    {
        try
        {
            SetAppBarTitle.Invoke("Manage Transactions");
            Elements = TransactionRepository.GetAll();

            foreach (Transaction s in Elements)
            {
                // Initialize any additional tracking dictionaries if needed
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
            ElementBeforeEdit = ((Transaction)element).Clone() as Transaction;
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
            ((Transaction)element).MemberUsername = ElementBeforeEdit.MemberUsername;
            ((Transaction)element).PurchaseDate = ElementBeforeEdit.PurchaseDate;
            ((Transaction)element).ProductName = ElementBeforeEdit.ProductName;
            ((Transaction)element).Quantity = ElementBeforeEdit.Quantity;
            ((Transaction)element).Discount = ElementBeforeEdit.Discount;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error in ResetItemToOriginalValues: {ex.Message}");
        }
    }

    private bool FilterFunc(Transaction element)
    {
        return string.IsNullOrWhiteSpace(SearchString)
               || element.Id.ToString().Contains(SearchString, StringComparison.OrdinalIgnoreCase)
               || element.MemberUsername.Contains(SearchString, StringComparison.OrdinalIgnoreCase)
               || element.PurchaseDate.ToString().Contains(SearchString, StringComparison.OrdinalIgnoreCase)
               || element.ProductName.Contains(SearchString, StringComparison.OrdinalIgnoreCase)
               || element.Quantity.ToString().Contains(SearchString, StringComparison.OrdinalIgnoreCase)
               || element.Discount.ToString().Contains(SearchString, StringComparison.OrdinalIgnoreCase);
    }

    // Implement the remaining methods and logic specific to your Transaction model

}
