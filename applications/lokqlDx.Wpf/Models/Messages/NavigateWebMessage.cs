using CommunityToolkit.Mvvm.Messaging.Messages;

namespace lokqlDx.Wpf.Models.Messages;

internal class NavigateWebMessage : ValueChangedMessage<Uri>
{
    public NavigateWebMessage(Uri value) : base(value)
    {
    }
}
