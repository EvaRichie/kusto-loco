namespace lokqlDx.Wpf.Models;

public record Preferences
{
    public static string DefaultFontFamily => "Consolas";

    public string LastWorkspacePath { get; set; } = string.Empty;

    public double FontSize { get; set; } = 20;

    public string FontFamily { get; set; } = DefaultFontFamily;

    public double WindowWidth { get; set; }

    public double WindowHeight { get; set; }

    public double WindowTop { get; set; }

    public double WindowLeft { get; set; }

    public bool AutoSave { get; set; } = false;

    public string StartupScript { get; set; } = string.Empty;

    public IEnumerable<string> RecentProjects { get; set; } = [];

    public bool WordWrap { get; set; } = false;

    public bool ShowLineNumbers { get; set; } = false;
}
