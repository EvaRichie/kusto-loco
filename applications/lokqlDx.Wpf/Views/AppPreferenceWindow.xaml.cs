using lokqlDx.Wpf.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace lokqlDx.Wpf.Views;

/// <summary>
/// Interaction logic for AppPreferenceWindow.xaml
/// </summary>
public partial class AppPreferenceWindow : Window
{
    private AppPreferenceWindowViewModel? _viewModel;

    public AppPreferenceWindow()
    {
        InitializeComponent();
        _viewModel = App.ServiceProvider.GetService<AppPreferenceWindowViewModel>();
        DataContext = _viewModel;
    }
}
