using System.Diagnostics;
using FufuLauncher.Models;
using FufuLauncher.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace FufuLauncher.Views;

public sealed partial class AccountPage : Page
{
    #region 字段
    private bool _isDeleting;
    #endregion

    #region 属性
    public AccountViewModel ViewModel
    {
        get;
    }
    #endregion

    #region 构造函数
    public AccountPage()
    {
        ViewModel = App.GetService<AccountViewModel>();
        DataContext = ViewModel;
        InitializeComponent();
        Debug.WriteLine("AccountPage initialized");
    }
    #endregion

    #region 页面加载与动画
    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        EntranceStoryboard.Begin();
        await Task.Delay(600);
        await ViewModel.LoadUserInfoAsync(); 
    }
    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (DataContext is AccountViewModel vm)
        {
            await vm.RefreshDataAsync();
        }
    }
    private void AvatarPicture_Loaded(object sender, RoutedEventArgs e)
    {
        AvatarEntranceStoryboard.Begin();
    }
    #endregion

    #region 账户切换
    private void OnSwitchAccountClicked(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is AccountInfo account)
        {
            ViewModel.SwitchAccountCommand.Execute(account);
        }
    }
    #endregion

    #region 账户删除
    private async void OnDeleteSavedAccountClicked(object sender, RoutedEventArgs e)
    {
        if (_isDeleting) return;
        if (sender is not Button button || button.DataContext is not AccountInfo account)
            return;

        await DeleteAccountWithConfirmationAsync(account, false);
    }

    private async void OnDeleteCurrentAccountClicked(object sender, RoutedEventArgs e)
    {
        if (_isDeleting) return;
        var accountToDelete = ViewModel.CurrentAccount;
        if (accountToDelete == null) return;

        await DeleteAccountWithConfirmationAsync(accountToDelete, true);
    }

    private async Task DeleteAccountWithConfirmationAsync(AccountInfo account, bool isCurrentAccount)
    {
        _isDeleting = true;
        try
        {
            string title = isCurrentAccount ? "删除当前账号" : "删除账号";
            string content = isCurrentAccount
                ? $"确定要删除当前账号 {account.Nickname} ({account.GameUid}) 吗？\n此操作将删除该账号的所有相关数据，且无法恢复。"
                : $"确定要删除账号 {account.Nickname} ({account.GameUid}) 吗？\n\n此操作将删除该账号的所有相关数据，包括凭证、祈愿记录和云游戏凭证，且无法恢复。";

            var result = await ShowDeleteConfirmationDialogAsync(title, content);
            if (result == ContentDialogResult.Primary)
            {
                await ViewModel.DeleteAccountAsync(account);
                Debug.WriteLine($"[Page] 账号 {account.Nickname} 删除完成");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"删除账号异常: {ex.Message}");
        }
        finally
        {
            _isDeleting = false;
        }
    }

    private async Task<ContentDialogResult> ShowDeleteConfirmationDialogAsync(string title, string content)
    {
        var dialog = new ContentDialog
        {
            Title = title,
            Content = content,
            PrimaryButtonText = "删除",
            CloseButtonText = "取消",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = this.XamlRoot
        };
        return await dialog.ShowAsync();
    }
    #endregion

    #region 其他 UI 操作
    private void OnGachaAnalysisClicked(object sender, RoutedEventArgs e)
    {
        var window = new GachaAnalysisWindow();
        window.Activate();
    }

    private async void OnCopyGameUidClicked(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is string uid && !string.IsNullOrEmpty(uid))
        {
            var dataPackage = new Windows.ApplicationModel.DataTransfer.DataPackage();
            dataPackage.SetText(uid);
            Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(dataPackage);

            var icon = button.Content as FontIcon;
            if (icon != null)
            {
                var originalGlyph = icon.Glyph;
                icon.Glyph = "";
                await Task.Delay(800);
                icon.Glyph = originalGlyph;
            }
        }
    }
    #endregion
}
