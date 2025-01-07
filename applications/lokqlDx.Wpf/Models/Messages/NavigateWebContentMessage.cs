using CommunityToolkit.Mvvm.Messaging.Messages;

namespace lokqlDx.Wpf.Models.Messages;


internal class NavigateWebContentMessage : ValueChangedMessage<string>
{
    public NavigateWebContentMessage(string value) : base(value)
    {
    }
}
