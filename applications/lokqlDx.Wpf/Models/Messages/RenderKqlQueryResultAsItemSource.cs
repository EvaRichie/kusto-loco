using CommunityToolkit.Mvvm.Messaging.Messages;
using KustoLoco.Core;
using lokqlDx.Wpf.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Data;

namespace lokqlDx.Wpf.Models.Messages;

internal class RenderKqlQueryResultAsItemSource : ValueChangedMessage<KustoQueryResult>
{
    public static int DefaultMaxDisplay  => 10_000;

    public bool IsValid { get; }

    public string OverFlowMessage { get; } = string.Empty;

    public IEnumerable<DataRow>? DataSource { get; }

    public DataView? DefaultDataView { get; }

    public RenderKqlQueryResultAsItemSource(KustoQueryResult value) : base(value)
    {
        if (value.RowCount == 0)
        {
            IsValid = false;
            return;
        }

        IsValid = true;
        var settingName = "datagrid.maxrows";
        var settingProvider = App.ServiceProvider.GetRequiredService<WorkspaceManager>().Settings;
        var maxRows = settingProvider.GetIntOr(settingName, DefaultMaxDisplay);
        if (value.RowCount > maxRows)
            OverFlowMessage = @$"Warning: Displaying only the first {maxRows} rows of {value.RowCount} rows.  Set {settingName} to see more";

        DefaultDataView = value.ToDataTable().DefaultView;
        DataSource = value.ToDataTable().AsEnumerable();
    }
}
