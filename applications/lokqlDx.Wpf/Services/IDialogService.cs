using System.Reflection;
using System.Windows;

namespace lokqlDx.Wpf.Services;

public interface IDialogService
{
    object? DialogInstance { get; }

    object? LastDialogResult { get; }

    bool ShowDialog(string dialogType);

    void SetDialogResult(bool dialogResult);

    void SetDialogResult(object dialogResult);

    void CloseCurrentDialog();
}

public class Win32DialogService : IDialogService
{
    public object? DialogInstance { get; private set; }

    public object? LastDialogResult { get; private set; }

    public bool ShowDialog(string dialogType)
    {
        var viewTypes = Assembly.GetExecutingAssembly().ExportedTypes.Where(static i => i.IsSubclassOf(typeof(Window)));
        var targetWindowType = viewTypes.SingleOrDefault(i => i.Name.StartsWith(dialogType, StringComparison.CurrentCultureIgnoreCase));
        if (targetWindowType is null)
            return false;

        var instance = Activator.CreateInstance(targetWindowType);
        if (instance is Window instanceWindow)
        {
            DialogInstance = instanceWindow;
            instanceWindow.Owner = App.Current.MainWindow;
            return instanceWindow.ShowDialog().GetValueOrDefault();
        }

        return false;
    }

    public void CloseCurrentDialog()
    {
        if (DialogInstance is not Window windowInstance)
            return;

        windowInstance.Close();
        LastDialogResult = null;
    }

    public void SetDialogResult(bool result)
    {
        if (DialogInstance is not Window windowInstance)
            return;

        windowInstance.DialogResult = result;
    }

    public void SetDialogResult(object dialogResult)
    {
        LastDialogResult = dialogResult;
    }
}
