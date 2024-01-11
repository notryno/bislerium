﻿namespace BisleriumCafe.Shared.Buttons;

public partial class SyncDataButton
{
    private bool IsSaving = false;

    private async Task SaveData()
    {
        if (IsSaving)
        {
            return;
        }

        IsSaving = true;
        await UserRepository.FlushAsync();
        await SpareRepository.FlushAsync();
        await ProductRepository.FlushAsync();
        await MemberRepository.FlushAsync();
        await ActivityLogRepository.FlushAsync();
        await TransactionRepository.FlushAsync();
        IsSaving = false;

        Snackbar.Add("All Data Synced!", Severity.Success);
    }
}
