using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using lokqlDx.Wpf.Services;

namespace lokqlDx.Wpf.ViewModels;

public partial class WorkspaceOptionWindowViewModel : ObservableRecipient
{
    [ObservableProperty]
    private string _windowTitle = string.Empty;

    [ObservableProperty]
    private string _workspaceScript = string.Empty;

    private readonly IDialogService _dialogService;

    public WorkspaceOptionWindowViewModel(WorkspaceManager workspaceManager, IDialogService dialogService)
    {
        WindowTitle = "Workspace Options";
        WorkspaceScript = workspaceManager.Workspace.StartupScript;
        _dialogService = dialogService;
    }

    [RelayCommand]
    private void FinishDialog()
    {
        _dialogService.SetDialogResult(true);
        _dialogService.SetDialogResult(WorkspaceScript);
        _dialogService.CloseCurrentDialog();
    }

    [RelayCommand]
    private void CloseDialog()
    {
        _dialogService.CloseCurrentDialog();
    }
}
