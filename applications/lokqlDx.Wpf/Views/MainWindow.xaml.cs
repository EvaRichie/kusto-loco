using CommunityToolkit.Mvvm.Messaging;
using KustoLoco.Core;
using KustoLoco.Core.Console;
using Lokql.Engine;
using Lokql.Engine.Commands;
using lokqlDx.Wpf.Models.Messages;
using lokqlDx.Wpf.Services;
using lokqlDx.Wpf.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace lokqlDx.Wpf.Views;

public partial class MainWindow : Window
{
    private MainWindowViewModel _viewModel;

    private readonly IWpfTextKustoConsole _kustoOpsOutput;
    private InteractiveTableExplorer _interactiveExplorer;

    public MainWindow()
    {
        InitializeComponent();

        _viewModel = App.ServiceProvider.GetRequiredService<MainWindowViewModel>();
        DataContext = _viewModel;

        _kustoOpsOutput = App.ServiceProvider.GetRequiredService<IWpfTextKustoConsole>();
        _kustoOpsOutput.AttachControl(OutputText);

        var workspaceSetting = App.ServiceProvider.GetRequiredService<WorkspaceManager>().Settings;
        var adapter = new StandardFormatAdaptor(workspaceSetting, (IKustoConsole)_kustoOpsOutput);
        var processor = App.ServiceProvider.GetRequiredService<CommandProcessor>();
        _interactiveExplorer = new InteractiveTableExplorer((IKustoConsole)_kustoOpsOutput, adapter, workspaceSetting, processor, this);

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

        WeakReferenceMessenger.Default.Send(new NavigateWebMessage(new Uri("https://github.com/NeilMacMullen/kusto-loco/wiki/LokqlDX")));
    }

    private async void Window_Closing(object sender, CancelEventArgs e)
    {
        var appPreference = App.ServiceProvider.GetRequiredService<IAppPreferenceService>();
        await appPreference.SaveWindowInfoAsync(this);
    }
}

// For Recipent
public partial class MainWindow : IRecipient<NavigateWebMessage>
{
    async void IRecipient<NavigateWebMessage>.Receive(NavigateWebMessage message)
    {
        await WebViewInstance.EnsureCoreWebView2Async();
        WebViewInstance.CoreWebView2.Navigate(message.Value.ToString());
    }
}


public partial class MainWindow : IResultRenderingSurface
{
    public Task NavigateToUrl(Uri url)
    {
        WeakReferenceMessenger.Default.Send(new NavigateWebMessage(url));
        return Task.CompletedTask;
    }

    public Task RenderToDisplay(KustoQueryResult result)
    {
        return Task.CompletedTask;
    }

    public Task<byte[]> RenderToImage(KustoQueryResult result, double pWidth, double pHeight)
    {
        return Task.FromResult(Array.Empty<byte>());
    }
}
