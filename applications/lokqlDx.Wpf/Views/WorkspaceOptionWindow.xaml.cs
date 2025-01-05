using lokqlDx.Wpf.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace lokqlDx.Wpf.Views;

/// <summary>
/// Interaction logic for WorkspaceOptionWindow.xaml
/// </summary>
public partial class WorkspaceOptionWindow : Window
{
    private WorkspaceOptionWindowViewModel _viewModel;

    public WorkspaceOptionWindow()
    {
        InitializeComponent();

        _viewModel = App.ServiceProvider.GetRequiredService<WorkspaceOptionWindowViewModel>();
        DataContext = _viewModel;
    }
}
