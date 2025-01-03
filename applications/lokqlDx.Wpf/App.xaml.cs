using Microsoft.Web.WebView2.Core;
using System.Configuration;
using System.Data;
using System.Windows;
using NotNullStrings;
using Microsoft.Extensions.DependencyInjection;
using lokqlDx.Wpf.ViewModels;
using lokqlDx.Wpf.Services;

namespace lokqlDx.Wpf;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public static IServiceProvider ServiceProvider { get; private set; } = default!;

    public static string[] StartUpArgs { get; private set; } = [];

    protected override void OnStartup(StartupEventArgs e)
    {
        var services = new ServiceCollection();

        services.AddSingleton<IDialogService, Win32DialogService>();
        services.AddSingleton<IAppPreferenceService, Win32AppPreferenceService>();

        services.AddSingleton<MainWindowViewModel>();

        ServiceProvider = services.BuildServiceProvider();

        StartUpArgs = e.Args;

        base.OnStartup(e);
        EnsureWebViewAvailable();
    }

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
