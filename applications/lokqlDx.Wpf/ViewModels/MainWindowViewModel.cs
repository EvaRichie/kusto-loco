using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using lokqlDx.Wpf.Models.Messages;
using lokqlDx.Wpf.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lokqlDx.Wpf.ViewModels;

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
    private void OpenDialog(string dialogPrefix)
    {
        _dialogService.ShowDialog(dialogPrefix);
    }

    [RelayCommand]
    private void NavigateUri(string uriString)
    {
        if (!Uri.TryCreate(uriString, UriKind.RelativeOrAbsolute, out var uri))
            return;

        WeakReferenceMessenger.Default.Send(new NavigateWebMessage(uri));
    }
}
