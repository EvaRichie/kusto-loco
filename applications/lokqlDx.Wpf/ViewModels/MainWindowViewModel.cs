using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using lokqlDx.Wpf.Models;
using lokqlDx.Wpf.Models.Messages;
using lokqlDx.Wpf.Services;
using NotNullStrings;
using System.Data;
using System.Diagnostics;
using System.IO;

namespace lokqlDx.Wpf.ViewModels;

public partial class MainWindowViewModel : IRecipient<RenderKqlQueryResultAsItemSource>,
                                           IRecipient<WorkspaceValueChanged>,
                                           IRecipient<AppPreferenceValueChanged>
{
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

    void IRecipient<WorkspaceValueChanged>.Receive(WorkspaceValueChanged message)
    {
        var workspaceMerge = message.Value with { StartupScript = _currentPreferences.StartupScript };
        _currentWorkspace = workspaceMerge;
    }

    void IRecipient<AppPreferenceValueChanged>.Receive(AppPreferenceValueChanged message)
    {
        _currentPreferences = _currentPreferences with { AutoSave = message.Value.AutoSave, FontFamily = message.Value.FontFamily, StartupScript = message.Value.StartupScript };
        _preferenceService.CurrentPreference.AutoSave = _currentPreferences.AutoSave;
        _preferenceService.CurrentPreference.FontFamily = _currentPreferences.FontFamily;
        _preferenceService.CurrentPreference.StartupScript = _currentPreferences.StartupScript;

        _currentWorkspace = _currentWorkspace with { StartupScript = _currentWorkspace.StartupScript };
    }
}

public partial class MainWindowViewModel : ObservableRecipient
{
    [ObservableProperty]
    private string _windowTitle = string.Empty;

    [ObservableProperty]
    private bool _displayUseWordWrap = false;

    [ObservableProperty]
    private bool _displayShowLineNumber = false;

    [ObservableProperty]
    private string _displayFontFamilyName = Preferences.DefaultFontFamily;

    [ObservableProperty]
    private double _displayFontSize = 12;

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
    private readonly IKustoResultProcessService _kustoResultProcessService;

    private readonly WorkspaceManager _workspaceMgr;

    private Preferences _currentPreferences = new();
    private Workspace _currentWorkspace = new();

    public MainWindowViewModel(IAppPreferenceService appPreferenceService, IDialogService dialogService, IKustoResultProcessService kustoResultProcessService, WorkspaceManager workspaceMgr)
    {
        Messenger.Register<CurrentWorkspaceMessage>(this, WorkspaceMsgHandler);
        Messenger.Register<CurrentAppPreferenceMessage>(this, AppPreferenceMsgHandler);
        IsActive = true;

        WindowTitle = "lokqlDx";

        _preferenceService = appPreferenceService;
        _dialogService = dialogService;
        _kustoResultProcessService = kustoResultProcessService;

        _workspaceMgr = workspaceMgr;
        _currentWorkspace = _workspaceMgr.Workspace;
    }

    private void WorkspaceMsgHandler(object recipient, CurrentWorkspaceMessage message)
    {
        message.Reply(_workspaceMgr.Workspace);
    }

    private void AppPreferenceMsgHandler(object recipient, CurrentAppPreferenceMessage message)
    {
        message.Reply(_preferenceService.CurrentPreference);
    }

    partial void OnDisplayFontFamilyNameChanged(string value)
    {
        _currentPreferences.FontFamily = value;
    }

    partial void OnDisplayFontSizeChanged(double value)
    {
        _currentPreferences.FontSize = value;
    }

    partial void OnDisplayShowLineNumberChanged(bool value)
    {
        _currentPreferences.ShowLineNumbers = value;
    }

    partial void OnDisplayUseWordWrapChanged(bool value)
    {
        _currentPreferences.WordWrap = value;
    }

    [RelayCommand]
    private async Task InitializeAsync()
    {
        _preferenceService.EnsureDefaultFolderExists();
        await _preferenceService.LoadPreferenceAsync();
        _currentPreferences = _preferenceService.CurrentPreference;

        DisplayFontFamilyName = _currentPreferences.FontFamily;
        DisplayFontSize = _currentPreferences.FontSize;
        DisplayUseWordWrap = _currentPreferences.WordWrap;
        DisplayShowLineNumber = _currentPreferences.ShowLineNumbers;
    }

    //[RelayCommand]
    //private async Task NewWorkspaceAsync()
    //{
    //}

    //[RelayCommand]
    //private async Task OpenWorkspaceAsync()
    //{
    //    var defaultWorkspaceFolder = _workspaceMgr.ContainingFolder();
    //    var openFileDialog = _dialogService.ShowDialog();
    //}

    //[RelayCommand]
    //private async Task SaveWorkspaceAsync()
    //{
    //}

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
    }

    [RelayCommand]
    private void OpenAppPreferenceDialog(Type dialogType)
    {
        _dialogService.ShowDialog(dialogType.Name);
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
    private async Task OpenWithDefaultBrowser()
    {
        // Use Shell Execute to launch default browser.
        var processInfo = new ProcessStartInfo { UseShellExecute = true };
        if (_kustoResultProcessService.LastRender.Html.IsNotBlank())
        {
            try
            {
                var tempHtmlFile = Path.ChangeExtension(Path.GetTempFileName(), "html");
                var fileStream = File.Open(tempHtmlFile, new FileStreamOptions { Access = FileAccess.ReadWrite, Options = FileOptions.Asynchronous, Share = FileShare.None });
                var writer = new StreamWriter(fileStream);
                await writer.WriteAsync(_kustoResultProcessService.LastRender.Html);
                await writer.FlushAsync();
                await writer.DisposeAsync();

                processInfo.FileName = tempHtmlFile;
            }
            catch
            {
            }
        }

        if (Uri.TryCreate(_kustoResultProcessService.LastRender.Uri, UriKind.RelativeOrAbsolute, out var _))
        {
            processInfo.FileName = _kustoResultProcessService.LastRender.Uri;
        }

        Process.Start(processInfo);
    }

    [RelayCommand]
    private void CopyImageToClipboard()
    {
        WeakReferenceMessenger.Default.Send(new RequestWebViewToImageMessage(string.Empty));
    }

}
