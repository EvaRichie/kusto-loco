using KustoLoco.Core;
using KustoLoco.Core.Settings;
using KustoLoco.Rendering;

namespace lokqlDx.Wpf.Helpers;

public static class KustoQueryResultHelper
{
    public static string RenderToHtml(KustoQueryResult result, KustoSettingsProvider kustoSettingsProvider)
    {
        var renderer = new KustoResultRenderer(kustoSettingsProvider);
        return renderer.RenderToHtml(result);
    }
}
