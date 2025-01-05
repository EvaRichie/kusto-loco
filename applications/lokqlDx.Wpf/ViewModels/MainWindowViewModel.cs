using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using lokqlDx.Wpf.Models;
using lokqlDx.Wpf.Models.Messages;
using lokqlDx.Wpf.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lokqlDx.Wpf.ViewModels;

internal partial class AppPreferenceViewModel : ObservableRecipient
{
    private readonly IAppPreferenceService _appPreferenceService;

    public AppPreferenceViewModel(IAppPreferenceService appPreferenceService)
    {
        _appPreferenceService = appPreferenceService;
    }

    protected override void OnActivated()
    {
        Messenger.Register<AppPreferenceViewModel, CurrentAppPreferenceMessage>(this, async (vm, msg)=> msg.Reply(await FillPreferenceAsync()));
    }

    private async Task<Preferences> FillPreferenceAsync()
    {
        _appPreferenceService.EnsureDefaultFolderExists();
        await _appPreferenceService.LoadPreferenceAsync();
        return _appPreferenceService.CurrentPreference;
    }
}

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private string _windowTitle = string.Empty;

    [ObservableProperty]
    private bool _isUseWordWrap = false;

    [ObservableProperty]
    private bool _isShowLineNumber = false;

    [ObservableProperty]
    private double _displayFontSize = 12;

    [ObservableProperty]
    private string _displayFontFamilyName = "Consolas";

    [ObservableProperty]
    private Workspace _workspacePreference = new();

    [ObservableProperty]
    private string _kqlQueryText = string.Empty;

    [ObservableProperty]
    private bool _isEditorLoading = false;

    private readonly IAppPreferenceService _preferenceService;
    private readonly IDialogService _dialogService;

    public MainWindowViewModel(IAppPreferenceService appPreferenceService, IDialogService dialogService)
    {
        WindowTitle = "lokqlDx";
        _preferenceService = appPreferenceService;
        _dialogService = dialogService;
    }

    partial void OnDisplayFontSizeChanged(double value)
    {
        _preferenceService.CurrentPreference.FontSize = value;
    }

    partial void OnIsUseWordWrapChanged(bool value)
    {
        _preferenceService.CurrentPreference.WordWrap = value;
    }

    partial void OnIsShowLineNumberChanged(bool value)
    {
        _preferenceService.CurrentPreference.ShowLineNumbers = value;
    }

    [RelayCommand]
    private async Task InitializeAsync()
    {
        await _preferenceService.LoadPreferenceAsync();

        IsUseWordWrap = _preferenceService.CurrentPreference.WordWrap;
        IsShowLineNumber = _preferenceService.CurrentPreference.ShowLineNumbers;
        DisplayFontSize = _preferenceService.CurrentPreference.FontSize;
    }

    [RelayCommand]
    private void ChangeFontSize(string mode)
    {
        if (mode.Equals("INCREASE", StringComparison.CurrentCultureIgnoreCase))
        {
            DisplayFontSize = Math.Min(40, DisplayFontSize + 1);
            return;
        }

        DisplayFontSize = Math.Max(6, DisplayFontSize - 1);
    }

    [RelayCommand]
    private void OpenWorkspaceScriptDialog(Type dialogType)
    {
        _dialogService.ShowDialog(dialogType.Name);
        if (_dialogService.LastDialogResult is string workspaceScript)
        {
            WorkspacePreference = new Workspace { StartupScript = workspaceScript, Text = KqlQueryText };
        }
    }

    [RelayCommand]
    private void OpenAppPreferenceDialog(Type dialogType)
    {
        _dialogService.ShowDialog(dialogType.Name);
        if (_dialogService.LastDialogResult is not null)
        {

        }
    }

    [RelayCommand]
    private async Task InvokeRunKqlQueryAsync()
    {
        IsEditorLoading = true;

        await Task.Delay(TimeSpan.FromSeconds(1));

        IsEditorLoading = false;
    }

    [RelayCommand]
    private void NavigateUri(string uriString)
    {
        if (!Uri.TryCreate(uriString, UriKind.RelativeOrAbsolute, out var uri))
            return;

        WeakReferenceMessenger.Default.Send(new NavigateWebMessage(uri));
    }
}
