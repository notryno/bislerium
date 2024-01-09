namespace BisleriumCafe.Shared.Dialogs;

public partial class AddProductDialog
{
    [CascadingParameter] private MudDialogInstance MudDialog { get; set; }
    [Parameter] public Action ChangeParentState { get; set; }

    private MudForm form;

    private string Name;
    private string Description;
    private string ProductType;
    private decimal Price;
    private int AvailableQuantity;

    private void Cancel()
    {
        MudDialog.Cancel();
    }

    private async Task AddProduct()
    {
        await form.Validate();
        if (form.IsValid)
        {
            Product product = new()
            {
                Name = Name,
                Description = Description,
                ProductType = ProductType,
                Price = Price,
                AvailableQuantity = AvailableQuantity
            };
            ProductRepository.Add(product);
            ChangeParentState.Invoke();

            Snackbar.Add($"Product {Name} is Added!", Severity.Success);
            MudDialog.Close();
        }
    }
}
