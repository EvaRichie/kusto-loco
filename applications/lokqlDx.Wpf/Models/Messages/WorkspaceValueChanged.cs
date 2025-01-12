using CommunityToolkit.Mvvm.Messaging.Messages;
using lokqlDx.Wpf.Services;

namespace lokqlDx.Wpf.Models.Messages;

internal class WorkspaceValueChanged : ValueChangedMessage<Workspace>
{
    public WorkspaceValueChanged(Workspace value) : base(value)
    {
    }
}
