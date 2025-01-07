using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lokqlDx.Wpf.Views;

public static class WebView2Extensions
{
    /// <summary>
    ///     Wrap NavigateToString to Async flow.
    /// </summary>
    /// <param name="coreWebView2"></param>
    /// <param name="htmlContent"></param>
    /// <returns></returns>
    public static async Task<bool> NavigateToStringAsync(this CoreWebView2 coreWebView2, string htmlContent)
    {
        var tcs = new TaskCompletionSource<bool>();

        try
        {
            coreWebView2.NavigationCompleted += OnNaviCompleted;
            coreWebView2.NavigateToString(htmlContent);
        }
        catch (Exception ex)
        {
            tcs.TrySetException(ex);
        }

        return await tcs.Task;

        void OnNaviCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            coreWebView2.NavigationCompleted -= OnNaviCompleted;
            tcs.TrySetResult(true);
        }
    }
}
