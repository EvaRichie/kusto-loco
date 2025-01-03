using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace lokqlDx.Wpf.Services;

public interface IDialogService
{
    bool ShowDialog(string dialogType);
}

public class Win32DialogService : IDialogService
{
    public bool ShowDialog(string dialogType)
    {
        var viewTypes = Assembly.GetExecutingAssembly().ExportedTypes.Where(static i => i.IsSubclassOf(typeof(Window)));
        var targetWindowType = viewTypes.SingleOrDefault(i => i.Name.StartsWith(dialogType, StringComparison.CurrentCultureIgnoreCase));
        if (targetWindowType is null)
            return false;

        var instance = Activator.CreateInstance(targetWindowType);
        if (instance is Window instanceWindow)
        {
            instanceWindow.Owner = App.Current.MainWindow;
            return instanceWindow.ShowDialog().GetValueOrDefault();
        }

        return false;
    }
}
