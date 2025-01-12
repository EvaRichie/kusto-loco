using CommunityToolkit.Mvvm.Messaging.Messages;
using lokqlDx.Wpf.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lokqlDx.Wpf.Models.Messages;

internal class CurrentWorkspaceMessage : RequestMessage<Workspace>
{
}
