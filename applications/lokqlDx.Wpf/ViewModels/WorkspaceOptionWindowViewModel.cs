using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using lokqlDx.Wpf.Models;
using lokqlDx.Wpf.Models.Messages;
using lokqlDx.Wpf.Services;

namespace lokqlDx.Wpf.ViewModels;

public partial class WorkspaceOptionWindowViewModel : ObservableRecipient
{
    [ObservableProperty]
    private string _windowTitle = string.Empty;

    [ObservableProperty]
    private string _workspaceScript = string.Empty;

    private Workspace _workspace = new Workspace();

    private readonly IDialogService _dialogService;

    public WorkspaceOptionWindowViewModel(IDialogService dialogService)
    {
        WindowTitle = "Workspace Options";
        _dialogService = dialogService;

        // Request workspace from MainViewModel
        var workspaceMsg = WeakReferenceMessenger.Default.Send(new CurrentWorkspaceMessage());
        if (workspaceMsg.HasReceivedResponse)
        {
            _workspace = workspaceMsg.Response;
            WorkspaceScript = _workspace.StartupScript;
        }
    }

    [RelayCommand]
    private void FinishDialog()
    {
        // Send workspace with text value updated
        var updatedWorkspace = _workspace with { Text = WorkspaceScript };
        WeakReferenceMessenger.Default.Send(new WorkspaceValueChanged(updatedWorkspace));

        _dialogService.SetDialogResult(true);
        _dialogService.CloseCurrentDialog();
    }

    [RelayCommand]
    private void CloseDialog()
    {
        _dialogService.CloseCurrentDialog();
    }
}
