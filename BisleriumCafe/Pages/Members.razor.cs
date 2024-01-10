namespace BisleriumCafe.Pages;

public partial class Members
{
    public const string Route = "/members";

    private readonly bool Dense = true;
    private readonly bool Fixed_header = true;
    private readonly bool Fixed_footer = true;
    private readonly bool Hover = true;
    private readonly bool ReadOnly = false;
    private readonly bool VanCancelEdit = true;
    private readonly bool BlockSwitch = true;
    private string SearchString;
    private Member ElementBeforeEdit;
    private readonly TableApplyButtonPosition ApplyButtonPosition = TableApplyButtonPosition.End;
    private readonly TableEditButtonPosition EditButtonPosition = TableEditButtonPosition.End;
    private readonly TableEditTrigger EditTrigger = TableEditTrigger.RowClick;
    private IEnumerable<Member> Elements;

    [CascadingParameter]
    private Action<string> SetAppBarTitle { get; set; }

    protected override void OnInitialized()
    {
        SetAppBarTitle.Invoke("Manage Members");

        if (MemberRepository.Count > 0)
        {
            var membersCopy = MemberRepository.GetAll().ToList();

            // Check the validity of membership for each member
            foreach (var member in membersCopy)
            {
                // Update the IsValid property based on the membership start date
                member.IsValid = (DateTime.Now - member.MembershipStartDate).TotalDays <= 30;
                MemberRepository.Update(member);
            }
            Elements = MemberRepository.GetAll();
        }
        else
        {
            Snackbar.Add("Empty");
            Elements = new List<Member>();
        }
    }

    private void BackupItem(object element)
    {
        ElementBeforeEdit = ((Member)element).Clone() as Member;
    }

    private void ResetItemToOriginalValues(object element)
    {
        ((Member)element).UserName = ElementBeforeEdit.UserName;
        ((Member)element).Phone = ElementBeforeEdit.Phone;
        ((Member)element).FullName = ElementBeforeEdit.FullName;
        ((Member)element).IsRegularCustomer = ElementBeforeEdit.IsRegularCustomer;
        ((Member)element).PurchasesCount = ElementBeforeEdit.PurchasesCount;
        ((Member)element).FreeCoffeeRedemptionCount = ElementBeforeEdit.FreeCoffeeRedemptionCount;
        //((Member)element).LastPurchaseDate = ElementBeforeEdit.LastPurchaseDate;
        ((Member)element).IsValid = ElementBeforeEdit.IsValid;
        ((Member)element).MembershipStartDate = ElementBeforeEdit.MembershipStartDate;
    }

    private bool FilterFunc(Member element)
    {
        return string.IsNullOrWhiteSpace(SearchString)
               || element.Id.ToString().Contains(SearchString, StringComparison.OrdinalIgnoreCase)
               || element.UserName.Contains(SearchString, StringComparison.OrdinalIgnoreCase)
               || element.Phone.Contains(SearchString, StringComparison.OrdinalIgnoreCase)
               || element.FullName.Contains(SearchString, StringComparison.OrdinalIgnoreCase)
               || element.IsRegularCustomer.ToString().Contains(SearchString, StringComparison.OrdinalIgnoreCase)
               || element.PurchasesCount.ToString().Contains(SearchString, StringComparison.OrdinalIgnoreCase)
               || element.FreeCoffeeRedemptionCount.ToString().Contains(SearchString, StringComparison.OrdinalIgnoreCase)
               //|| element.LastPurchaseDate.ToString("yyyy-MM-dd").Contains(SearchString, StringComparison.OrdinalIgnoreCase)
               || element.IsValid.ToString().Contains(SearchString, StringComparison.OrdinalIgnoreCase)
               || element.MembershipStartDate.ToString("yyyy-MM-dd").Contains(SearchString, StringComparison.OrdinalIgnoreCase);
    }

    private async Task AddDialog()
    {
        DialogParameters parameters = new()
        {
            { "ChangeParentState", new Action(StateHasChanged) }
        };
        await DialogService.ShowAsync<Shared.Dialogs.AddMemberDialog>("Add Member", parameters);
    }

    private void Reload()
    {
        StateHasChanged();
    }
}
