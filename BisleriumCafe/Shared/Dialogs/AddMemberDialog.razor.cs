namespace BisleriumCafe.Shared.Dialogs;

public partial class AddMemberDialog
{
    [CascadingParameter] private MudDialogInstance MudDialog { get; set; }
    [Parameter] public Action ChangeParentState { get; set; }

    private MudForm form;

    private string UserName;
    private string Phone;
    private string FullName;
    private bool IsRegularCustomer;

    private void Cancel()
    {
        MudDialog.Cancel();
    }

    private async Task AddMember()
    {
        await form.Validate();
        if (form.IsValid)
        {
            // Perform the necessary action to add a member
            // For example, you might use a service to handle member addition
            // MemberService.AddMember(UserName, Phone, FullName, IsRegularCustomer);

            ChangeParentState.Invoke();

            Snackbar.Add($"Member {UserName} is Added!", Severity.Success);
            MudDialog.Close();
        }
    }

    private IEnumerable<string> UserNameValidation(string arg)
    {
        // Validation logic for username
        // You can customize this based on your requirements
        if (string.IsNullOrWhiteSpace(arg))
        {
            yield return "Username is required!";
            yield break;
        }
        // Add more validation rules as needed
    }

    private IEnumerable<string> PhoneValidation(string arg)
    {
        // Validation logic for phone number
        // You can customize this based on your requirements
        if (string.IsNullOrWhiteSpace(arg))
        {
            yield return "Phone number is required!";
            yield break;
        }
        // Add more validation rules as needed
    }
}
