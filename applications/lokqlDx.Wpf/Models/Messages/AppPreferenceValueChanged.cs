using CommunityToolkit.Mvvm.Messaging.Messages;

namespace lokqlDx.Wpf.Models.Messages;

internal class AppPreferenceValueChanged : ValueChangedMessage<Preferences>
{
    public AppPreferenceValueChanged(Preferences value) : base(value)
    {
    }
}
