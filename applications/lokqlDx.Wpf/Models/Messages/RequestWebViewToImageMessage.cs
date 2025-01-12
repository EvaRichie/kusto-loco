using CommunityToolkit.Mvvm.Messaging.Messages;

namespace lokqlDx.Wpf.Models.Messages;

internal class RequestWebViewToImageMessage : ValueChangedMessage<string>
{
    public RequestWebViewToImageMessage(string value) : base(value)
    {
    }
}
