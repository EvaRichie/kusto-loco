using CommunityToolkit.Mvvm.Messaging.Messages;

namespace lokqlDx.Wpf.Models.Messages;

internal class SwitchViewModelMessage : ValueChangedMessage<RenderType>
{
    public SwitchViewModelMessage(RenderType value) : base(value)
    {
    }
}

public enum RenderType
{
    None,
    Chart,
    DataGrid
}
