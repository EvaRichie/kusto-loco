using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using lokqlDx.Wpf.Models;
using lokqlDx.Wpf.Models.Messages;
using lokqlDx.Wpf.Services;
using Microsoft.Extensions.DependencyInjection;
using NotNullStrings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lokqlDx.Wpf.ViewModels;

public partial class MainWindowViewModel : ObservableRecipient, IRecipient<RenderKqlQueryResultAsItemSource>
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

    [ObservableProperty]
    private DataView? _tableView = null;

    [ObservableProperty]
    private bool _isOverflowDataList = false;

    [ObservableProperty]
    private string _overFlowMessage = string.Empty;

    private readonly IAppPreferenceService _preferenceService;
    private readonly IDialogService _dialogService;

    public MainWindowViewModel(IAppPreferenceService appPreferenceService, IDialogService dialogService)
    {
        IsActive = true;
        Messenger.Register<CurrentAppPreferenceMessage>(this, handler);

        WindowTitle = "lokqlDx";
        _preferenceService = appPreferenceService;
        _dialogService = dialogService;
    }

    private async void handler(object recipient, CurrentAppPreferenceMessage message)
    {
        _preferenceService.EnsureDefaultFolderExists();
        await _preferenceService.LoadPreferenceAsync();

        message.Reply(_preferenceService.CurrentPreference);
    }

    void IRecipient<RenderKqlQueryResultAsItemSource>.Receive(RenderKqlQueryResultAsItemSource message)
    {
        if (!message.IsValid)
            return;

        if (message.DefaultDataView is null)
            return;

        IsOverflowDataList = message.OverFlowMessage.IsNotBlank();
        OverFlowMessage = message.OverFlowMessage;
        TableView = message.DefaultDataView;
        WeakReferenceMessenger.Default.Send(new SwitchViewModelMessage(RenderType.DataGrid));
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
        _preferenceService.EnsureDefaultFolderExists();
        await _preferenceService.LoadPreferenceAsync();

        //WeakReferenceMessenger.Default.Send(new CurrentAppPreferenceMessage());

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
    private async Task InvokeRunKqlQueryAsync(string query)
    {
        IsEditorLoading = true;

        await Task.Delay(TimeSpan.FromSeconds(1));
        WeakReferenceMessenger.Default.Send(new RequestRunKqlMessage(query));

        IsEditorLoading = false;
    }

    [RelayCommand]
    private void NavigateUri(string uriString)
    {
        if (!Uri.TryCreate(uriString, UriKind.RelativeOrAbsolute, out var uri))
            return;

        WeakReferenceMessenger.Default.Send(new NavigateWebMessage(uri));
    }

    [RelayCommand]
    private void OpenWithDefaultBrowser()
    {

    }

    [RelayCommand]
    private void CopyImageToClipboard()
    {

    }
}
