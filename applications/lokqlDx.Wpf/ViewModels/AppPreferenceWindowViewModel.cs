using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lokqlDx.Wpf.ViewModels;

public partial class AppPreferenceWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private string _windowTitle = string.Empty;

    public AppPreferenceWindowViewModel()
    {
        WindowTitle = "App Preference";
    }
}
