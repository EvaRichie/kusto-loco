using CommunityToolkit.Mvvm.Messaging;
using KustoLoco.Core.Settings;
using Lokql.Engine;
using Lokql.Engine.Commands;
using lokqlDx.Wpf.Models.Messages;
using lokqlDx.Wpf.Services;
using lokqlDx.Wpf.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Web.WebView2.Core;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace lokqlDx.Wpf.Views;

public partial class MainWindow : Window
{
    private MainWindowViewModel _viewModel;

    private readonly Size _minWindowSize = new(600, 400);
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

        WeakReferenceMessenger.Default.Register<NavigateWebMessage>(this);
        WeakReferenceMessenger.Default.Register<NavigateWebContentMessage>(this);
        WeakReferenceMessenger.Default.Register<RequestRunKqlMessage>(this);
        WeakReferenceMessenger.Default.Register<SwitchViewModelMessage>(this);
        WeakReferenceMessenger.Default.Register<RequestWebViewToImageMessage>(this);
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
        var preferenceMsg = WeakReferenceMessenger.Default.Send(new CurrentAppPreferenceMessage());
        if (preferenceMsg.HasReceivedResponse)
        {
            // Update Window preference.
            if (Width > 100 && Height > 100 && Left > 0 && Top > 0)
            {
                Width = preferenceMsg.Response.WindowWidth < _minWindowSize.Width
                    ? _minWindowSize.Width
                    : preferenceMsg.Response.WindowWidth;
                Height = preferenceMsg.Response.WindowHeight < _minWindowSize.Height
                    ? _minWindowSize.Height
                    : preferenceMsg.Response.WindowHeight;
                Left = preferenceMsg.Response.WindowLeft;
                Top = preferenceMsg.Response.WindowTop;
            }
        }

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
    IRecipient<SwitchViewModelMessage>,
    IRecipient<RequestWebViewToImageMessage>
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
            await WebViewInstance.NavigateToStringAsync(message.Value);
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

    async void IRecipient<RequestWebViewToImageMessage>.Receive(RequestWebViewToImageMessage message)
    {
        try
        {
            await WebViewInstance.EnsureCoreWebView2Async();
            var memoryStream = new MemoryStream();
            await WebViewInstance.CoreWebView2.CapturePreviewAsync(CoreWebView2CapturePreviewImageFormat.Png, memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);

            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = memoryStream;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();
            bitmapImage.Freeze(); // Freeze the image to make it cross-thread accessible

            Clipboard.SetImage(bitmapImage);

            _interactiveExplorer.Info("Chart copied to clipboard");
        }
        catch
        {
            // Do nothing.
        }
    }
}
