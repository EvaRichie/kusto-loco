using CommunityToolkit.Mvvm.Messaging.Messages;

namespace lokqlDx.Wpf.Models.Messages;

internal class RequestRunKqlMessage : ValueChangedMessage<string>
{
    public RequestRunKqlMessage(string value) : base(value)
    {
    }
}
