using CommunityToolkit.Mvvm.Messaging;
using DocumentFormat.OpenXml.Bibliography;
using lokqlDx.Wpf.Models.Messages;
using lokqlDx.Wpf.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace lokqlDx.Wpf.Views;

public partial class MainWindow : Window
{
    private MainWindowViewModel _viewModel;

    public MainWindow()
    {
        InitializeComponent();
        _viewModel = App.ServiceProvider.GetRequiredService<MainWindowViewModel>();
        DataContext = _viewModel;
        MinWidth = 1280;
        MinHeight = 800;

        WeakReferenceMessenger.Default.UnregisterAll(this);
        WeakReferenceMessenger.Default.RegisterAll(this);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        // here I suppose the window's menu is named "MainMenu"
        MainMenu.RaiseMenuItemClickOnKeyGesture(e);
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        await _viewModel.InitializeCommand.ExecuteAsync(default);
        _viewModel.NavigateUriCommand.Execute("https://github.com/NeilMacMullen/kusto-loco/wiki/LokqlDX");

        //Editor.AddInternalCommands(_explorer._commandProcessor.GetVerbs()
        //    .Select(v =>
        //        new IntellisenseEntry(v.Key, v.Value, string.Empty))
        //    .ToArray());
        //RegistryOperations.AssociateFileType(true);
        //PreferencesManager.EnsureDefaultFolderExists();

        //_preferenceManager.Load();

        //UpdateDynamicUiFromPreferences(_preferenceManager.Preferences);
        //_mruList = MruList.LoadFromArray(_preferenceManager.Preferences.RecentProjects);
        //RebuildRecentFilesList();

        //if (Width > 100 && Height > 100 && Left > 0 && Top > 0)
        //{
        //    Width = _preferenceManager.Preferences.WindowWidth < _minWindowSize.Width
        //        ? _minWindowSize.Width
        //        : _preferenceManager.Preferences.WindowWidth;
        //    Height = _preferenceManager.Preferences.WindowHeight < _minWindowSize.Height
        //        ? _minWindowSize.Height
        //        : _preferenceManager.Preferences.WindowHeight;
        //    Left = _preferenceManager.Preferences.WindowLeft;
        //    Top = _preferenceManager.Preferences.WindowTop;
        //}

        //var pathToLoad = _args.Any()
        //    ? _args[0]
        //    : string.Empty;
        //await LoadWorkspace(pathToLoad);
        //await Navigate("https://github.com/NeilMacMullen/kusto-loco/wiki/LokqlDX");
    }

    private void Window_Closing(object sender, CancelEventArgs e)
    {

    }
}


public partial class MainWindow : IRecipient<NavigateWebMessage>
{
    async void IRecipient<NavigateWebMessage>.Receive(NavigateWebMessage message)
    {
        await WebViewInstance.EnsureCoreWebView2Async();
        WebViewInstance.CoreWebView2.Navigate(message.Value.ToString());
    }
}
