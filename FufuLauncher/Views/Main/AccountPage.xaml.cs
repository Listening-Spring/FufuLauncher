using System.Diagnostics;
using FufuLauncher.Models;
using FufuLauncher.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace FufuLauncher.Views;

public sealed partial class AccountPage : Page
{
    public AccountViewModel ViewModel
    {
        get;
    }

    public AccountPage()
    {
        ViewModel = App.GetService<AccountViewModel>();
        DataContext = ViewModel;
        InitializeComponent();
        Debug.WriteLine("AccountPage initialized");
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e)
    {
        EntranceStoryboard.Begin();
        
        await Task.Delay(600);
        await ViewModel.LoadUserInfoAsync();
    }
    
    private void AvatarPicture_Loaded(object sender, RoutedEventArgs e)
    {
        AvatarEntranceStoryboard.Begin();
    }

    private void OnSwitchAccountClicked(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is AccountInfo account)
        {
            ViewModel.SwitchAccountCommand.Execute(account);
        }
    }

    private void OnGachaAnalysisClicked(object sender, RoutedEventArgs e)
    {
        // var dialog = new GachaDialog();
        // dialog.XamlRoot = XamlRoot;
        // await dialog.ShowAsync();

        var window = new GachaAnalysisWindow();
        window.Activate();
    }
}