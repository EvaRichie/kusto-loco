using lokqlDx.Wpf.Models;
using NotNullStrings;
using System.IO;
using System.Text.Json;

namespace lokqlDx.Wpf.Services;

public interface IAppPreferenceService
{
    Preferences CurrentPreference { get; }

    void EnsureDefaultFolderExists();

    Task SavePreferenceAsync();

    Task LoadPreferenceAsync();
}

public class Win32AppPreferenceService : IAppPreferenceService
{
    private readonly JsonSerializerOptions _options = new() { WriteIndented = true };

    public Preferences CurrentPreference { get; private set; } = new();

    public Win32AppPreferenceService()
    {
        EnsureDefaultFolderExists();
    }

    public void EnsureDefaultFolderExists()
    {
        var rootPath = RootPath();
        if (Directory.Exists(rootPath))
            return;

        Directory.CreateDirectory(RootPath());
    }

    private static string RootPath()
    {
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "lokql");
    }

    private static string PreferencesPath()
    {
        return Path.Combine(RootPath(), "preferences.json");
    }

    public async Task SavePreferenceAsync()
    {
        EnsureDefaultFolderExists();
        try
        {
            using var fileStream = File.Create(PreferencesPath());
            await JsonSerializer.SerializeAsync<Preferences>(fileStream, CurrentPreference, _options);
            await fileStream.FlushAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }

    public async Task LoadPreferenceAsync()
    {
        try
        {
            using var fileStream = File.Open(PreferencesPath(), FileMode.Open, FileAccess.Read);
            var preference = await JsonSerializer.DeserializeAsync<Preferences>(fileStream, _options);
            if (preference is not null)
            {
                CurrentPreference = preference;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }


        if (CurrentPreference.FontSize <= 0)
            CurrentPreference.FontSize = 12;
        if (CurrentPreference.FontFamily.IsBlank())
            CurrentPreference.FontFamily = "Consolas";
    }
}
