using CommunityToolkit.Mvvm.Messaging;
using KustoLoco.Core;
using KustoLoco.Core.Settings;
using Lokql.Engine.Commands;
using lokqlDx.Wpf.Helpers;
using lokqlDx.Wpf.Models.Messages;
using NotNullStrings;
using System.Configuration;

namespace lokqlDx.Wpf.Services;

public interface IKustoResultProcessService : IResultRenderingSurface
{
    UriOrHtml LastRender { get; }

    IResultRenderingSurface AsRenderingSurface()
    {
        return (IResultRenderingSurface)this;
    }
}

public class Win32KustoResultProcessService : IKustoResultProcessService
{
    private readonly KustoSettingsProvider _settingProvider;

    public Win32KustoResultProcessService(WorkspaceManager workspaceManager)
    {
        _settingProvider = workspaceManager.Settings;
    }

    public UriOrHtml LastRender { get; private set; } = new UriOrHtml(string.Empty, string.Empty);

    public Task NavigateToUrl(Uri url)
    {
        WeakReferenceMessenger.Default.Send(new NavigateWebMessage(url));
        return Task.CompletedTask;
    }

    public Task RenderToDisplay(KustoQueryResult result)
    {
        // Render as Chart.
        if (result.Visualization.ChartType.IsNotBlank())
        {
            var htmlString = KustoQueryResultHelper.RenderToHtml(result, _settingProvider);
            LastRender = LastRender with { Html = htmlString };
            WeakReferenceMessenger.Default.Send(new NavigateWebContentMessage(htmlString));
            return Task.CompletedTask;
        }

        WeakReferenceMessenger.Default.Send(new RenderKqlQueryResultAsItemSource(result));
        return Task.CompletedTask;
    }

    public Task<byte[]> RenderToImage(KustoQueryResult result, double pWidth, double pHeight)
    {
        return Task.FromResult(Array.Empty<byte>());
    }
}


public readonly record struct UriOrHtml(string Uri, string Html);
