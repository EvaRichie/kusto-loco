using Microsoft.Web.WebView2.Core;
using System.Configuration;
using System.Data;
using System.Windows;
using NotNullStrings;
using Microsoft.Extensions.DependencyInjection;
using lokqlDx.Wpf.ViewModels;
using lokqlDx.Wpf.Services;
using lokqlDx.Wpf.Views;
using KustoLoco.Core.Console;
using Lokql.Engine.Commands;
using System.Windows.Navigation;

namespace lokqlDx.Wpf;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public static IServiceProvider ServiceProvider { get; private set; } = default!;

    public static string[] StartUpArgs { get; private set; } = [];

    public App()
    {
        var services = new ServiceCollection();

        services.AddSingleton<WorkspaceManager>();
        services.AddSingleton<IDialogService, Win32DialogService>();
        services.AddSingleton<IAppPreferenceService, Win32AppPreferenceService>();
        services.AddSingleton<IKustoResultProcessService, Win32KustoResultProcessService>();

        //services.AddSingleton<IKustoConsole, WpfOutputConsole>();
        services.AddSingleton<IWpfTextKustoConsole, WpfOutputConsole>();

        services.AddSingleton<CommandProcessor>(_ => CommandProcessor.Default());

        services.AddSingleton<MainWindowViewModel>();

        // For dialog service flow.
        services.AddTransient<WorkspaceOptionWindowViewModel>();
        services.AddTransient<AppPreferenceWindowViewModel>();

        ServiceProvider = services.BuildServiceProvider();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        StartUpArgs = e.Args;

        base.OnStartup(e);
        EnsureWebViewAvailable();
    }

    //protected override async void OnNavigating(NavigatingCancelEventArgs e)
    //{
    //    var appPreference = ServiceProvider.GetRequiredService<IAppPreferenceService>();
    //    appPreference.EnsureDefaultFolderExists();
    //    await appPreference.LoadPreferenceAsync();
    //}

    private void EnsureWebViewAvailable()
    {
        try
        {
            var webViewVersion = CoreWebView2Environment.GetAvailableBrowserVersionString();
            if (webViewVersion.IsNotBlank())
                return;
        }
        catch
        {
            // ignored
        }

        //var dlg = new MissingWebviewWindow
        //{
        //    WindowStartupLocation = WindowStartupLocation.CenterScreen
        //};
        //dlg.ShowDialog();
        //Application.Current.Shutdown();

    }
}
