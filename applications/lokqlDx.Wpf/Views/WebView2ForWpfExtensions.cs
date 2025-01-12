using Microsoft.Web.WebView2.Core;
using System.IO;

namespace lokqlDx.Wpf.Views;

public static class WebView2ForWpfExtensions
{
    /// <summary>
    ///     Wrap NavigateToString to Async flow.
    /// </summary>
    /// <param name="coreWebView2"></param>
    /// <param name="htmlContent"></param>
    /// <returns></returns>
    public static async Task<bool> NavigateToStringAsync(this Microsoft.Web.WebView2.Wpf.WebView2 webView2, string htmlContent)
    {
        var tcs = new TaskCompletionSource<bool>();

        try
        {
            await webView2.EnsureCoreWebView2Async();
            webView2.CoreWebView2.NavigationCompleted += OnNaviCompleted;
            webView2.CoreWebView2.NavigateToString(htmlContent);
        }
        catch (Exception ex)
        {
            tcs.TrySetException(ex);
        }

        return await tcs.Task;

        void OnNaviCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            webView2.CoreWebView2.NavigationCompleted -= OnNaviCompleted;
            tcs.TrySetResult(true);
        }
    }

    /// <summary>
    ///     Capture current Web view into image stream buffer.
    /// </summary>
    /// <param name="coreWebView2"></param>
    /// <returns></returns>
    public static async Task<byte[]> CaptureCurrentViewAsync(this Microsoft.Web.WebView2.Wpf.WebView2 webView2)
    {
        await webView2.EnsureCoreWebView2Async();
        var memoryStream = new MemoryStream();
        await webView2.CoreWebView2.CapturePreviewAsync(CoreWebView2CapturePreviewImageFormat.Png, memoryStream);
        var buffer = memoryStream.GetBuffer();
        await memoryStream.DisposeAsync();
        return buffer;
    }
}
