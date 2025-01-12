using KustoLoco.Core.Settings;
using Lokql.Engine;
using lokqlDx.Wpf.Models;
using NotNullStrings;
using System.IO;
using System.Text.Json;

namespace lokqlDx.Wpf.Services;

public class WorkspaceManager
{
    public const string Extension = "lokql";

    private Workspace _workspace = new();

    public string FilePath = string.Empty;

    public Workspace Workspace => _workspace;

    public KustoSettingsProvider Settings { get; } = new();

    private void EnsureWorkspacePopulated()
    {
        var userText = _workspace.Text;
        if (!userText.IsBlank())
            return;
        userText = @"
# move the cursor over a block of lines and press SHIFT-ENTER to run

# load a CSV file into a table called 'data'

.load c:\data\mydata.csv data

# gets the distribution of values in the 'Name' column

data 
| summarize count() by Name 
| render barchart

#save the results to a parquet file
.save namecount.parqet

";
    }

    public void Save(string filePath, Workspace workspace)
    {
        FilePath = filePath;
        try
        {
            var json = JsonSerializer.Serialize(workspace);
            File.WriteAllText(FilePath, json);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error saving workspace: {e.Message}");
        }
    }

    public void Load(string path)
    {
        if (path.IsBlank())
        {
            CreateNew();
            return;
        }

        var rootSettingFolderPath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "kustoloco");

        if (!Directory.Exists(rootSettingFolderPath))
            Directory.CreateDirectory(rootSettingFolderPath);

        _workspace = new Workspace();
        Settings.Reset();
        FilePath = path;
        SetWorkingPaths();
        if (path.IsNotBlank())
        {
            try
            {
                var json = File.ReadAllText(FilePath);
                _workspace = JsonSerializer.Deserialize<Workspace>(json)!;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error loading workspace: {e.Message}");
            }
        }

        EnsureWorkspacePopulated();
    }

    public void CreateNew()
    {
        _workspace = new Workspace();
        Settings.Reset();
        FilePath = string.Empty;
    }

    public bool IsDirty(Workspace workspace)
    {
        return workspace != _workspace;
    }

    public string ContainingFolder()
    {
        return Path.GetDirectoryName(FilePath).NullToEmpty();
    }

    public void SetWorkingPaths()
    {
        if (FilePath.IsBlank())
            return;

        var containingFolder = ContainingFolder();
        Settings.Set(StandardFormatAdaptor.Settings.KustoDataPath.Name, containingFolder);
        Settings.Set(LokqlSettings.ScriptPath.Name, containingFolder);
        Settings.Set(LokqlSettings.QueryPath.Name, containingFolder);
    }
}
