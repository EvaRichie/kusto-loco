using CommunityToolkit.Mvvm.Messaging;
using KustoLoco.Core;
using KustoLoco.Core.Console;
using KustoLoco.Core.Settings;
using Lokql.Engine;
using Lokql.Engine.Commands;
using lokqlDx.Wpf.Helpers;
using lokqlDx.Wpf.Models.Messages;
using lokqlDx.Wpf.Services;
using lokqlDx.Wpf.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace lokqlDx.Wpf.Views;

public partial class MainWindow : Window
{
    private MainWindowViewModel _viewModel;

    private readonly IWpfTextKustoConsole _kustoOpsOutput;

    private readonly KustoSettingsProvider _settingProvider;
    private InteractiveTableExplorer _interactiveExplorer;

    public MainWindow()
    {
        InitializeComponent();

        _viewModel = App.ServiceProvider.GetRequiredService<MainWindowViewModel>();
        DataContext = _viewModel;

        _kustoOpsOutput = App.ServiceProvider.GetRequiredService<IWpfTextKustoConsole>();
        _kustoOpsOutput.AttachControl(OutputText);

        _settingProvider = App.ServiceProvider.GetRequiredService<WorkspaceManager>().Settings;
        var adapter = new StandardFormatAdaptor(_settingProvider, _kustoOpsOutput.AsIKustoConsole());
        var processor = App.ServiceProvider.GetRequiredService<CommandProcessor>();
        var renderService = App.ServiceProvider.GetRequiredService<IKustoResultProcessService>();
        _interactiveExplorer = new InteractiveTableExplorer(_kustoOpsOutput.AsIKustoConsole(), adapter, _settingProvider, processor, renderService.AsRenderingSurface());

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
        //var preference = WeakReferenceMessenger.Default.Send(new CurrentAppPreferenceMessage());
        WeakReferenceMessenger.Default.Send(new NavigateWebMessage(new Uri("https://github.com/NeilMacMullen/kusto-loco/wiki/LokqlDX")));
    }

    private async void Window_Closing(object sender, CancelEventArgs e)
    {
        var appPreference = App.ServiceProvider.GetRequiredService<IAppPreferenceService>();
        await appPreference.SaveWindowInfoAsync(this);
    }
}

// For Recipient
public partial class MainWindow :
    IRecipient<NavigateWebMessage>,
    IRecipient<NavigateWebContentMessage>,
    IRecipient<RequestRunKqlMessage>,
    IRecipient<SwitchViewModelMessage>
{
    async void IRecipient<NavigateWebMessage>.Receive(NavigateWebMessage message)
    {
        await Dispatcher.InvokeAsync(async () =>
        {
            await WebViewInstance.EnsureCoreWebView2Async();
            WebViewInstance.CoreWebView2.Navigate(message.Value.ToString());
        });
    }

    async void IRecipient<NavigateWebContentMessage>.Receive(NavigateWebContentMessage message)
    {
        await Dispatcher.InvokeAsync(async () =>
        {
            await WebViewInstance.EnsureCoreWebView2Async();
            await WebViewInstance.CoreWebView2.NavigateToStringAsync(message.Value);
        });
    }

    async void IRecipient<RequestRunKqlMessage>.Receive(RequestRunKqlMessage message)
    {
        _kustoOpsOutput.PrepareForOutput();
        await Task.Run(async () => await _interactiveExplorer.RunInput(message.Value));
    }

    void IRecipient<SwitchViewModelMessage>.Receive(SwitchViewModelMessage message)
    {
        if (message.Value == RenderType.None)
            return;

        Dispatcher.Invoke(() =>
        {
            RenderingSurface.SelectedItem = message.Value == RenderType.Chart ? WebViewTabItem : DataListTabItem;
        });
    }
}
