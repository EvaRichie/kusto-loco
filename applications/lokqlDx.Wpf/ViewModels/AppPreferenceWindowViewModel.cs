using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using lokqlDx.Wpf.Models;
using lokqlDx.Wpf.Models.Messages;
using lokqlDx.Wpf.Services;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace lokqlDx.Wpf.ViewModels;

public partial class AppPreferenceWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private string _windowTitle = string.Empty;

    [ObservableProperty]
    private ObservableCollection<string> _buildInFontFamilyNames = [];

    [ObservableProperty]
    private string _preferenceFontFamilyName = string.Empty;

    [ObservableProperty]
    private string _startupScript = string.Empty;

    [ObservableProperty]
    private bool _useAutoSave = false;

    private Preferences _preference = new();

    private readonly IDialogService _dialogService;

    public AppPreferenceWindowViewModel(IDialogService dialogService)
    {
        WindowTitle = "App Preference";
        BuildInFontFamilyNames = new ObservableCollection<string>(Fonts.SystemFontFamilies.Select(_ => _.ToString()));

        _dialogService = dialogService;

        var preferenceMsg = WeakReferenceMessenger.Default.Send(new CurrentAppPreferenceMessage());
        if (preferenceMsg.HasReceivedResponse)
        {
            _preference = preferenceMsg.Response;

            PreferenceFontFamilyName = BuildInFontFamilyNames.SingleOrDefault(fn => fn.Equals(preferenceMsg.Response.FontFamily, StringComparison.CurrentCultureIgnoreCase)) ?? Preferences.DefaultFontFamily;
            StartupScript = preferenceMsg.Response.StartupScript;
            UseAutoSave = preferenceMsg.Response.AutoSave;
        }
    }

    [RelayCommand]
    private void FinishDialog()
    {
        var preferenceMerge = _preference with { AutoSave = UseAutoSave, StartupScript = StartupScript, FontFamily = PreferenceFontFamilyName };
        WeakReferenceMessenger.Default.Send(new AppPreferenceValueChanged(preferenceMerge));

        _dialogService.SetDialogResult(true);
        _dialogService.CloseCurrentDialog();
    }

    [RelayCommand]
    private void CloseDialog()
    {
        _dialogService.CloseCurrentDialog();
    }
}
