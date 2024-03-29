﻿namespace BisleriumCafe.Pages;

public partial class Index
{
    protected sealed override async Task OnInitializedAsync()
    {
        await _userRepository.LoadAsync();
        await _spareRepository.LoadAsync();
        await _productRepository.LoadAsync(); //Ryan
        await _memberRepository.LoadAsync(); //Ryan
        await _transactionRepository.LoadAsync(); //Ryan
        await _activityLogRepository.LoadAsync();
        try
        {
            await _authService.CheckSession();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex}");
            Snackbar.Add(ex.Message, Severity.Error);
        }
        await Task.Delay(1000);

        _userRepository.OnDebugConsoleWriteUserNames();

        if (_authService.CurrentUser is null)
        {
            _navigationManager.NavigateTo(Login.Route);
        }
        else
        {
            _navigationManager.NavigateTo(RoleRouter.Route);
        }
    }
}