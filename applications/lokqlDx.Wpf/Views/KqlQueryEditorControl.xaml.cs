using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using Lokql.Engine;
using Lokql.Engine.Commands;
using Microsoft.Extensions.DependencyInjection;
using NotNullStrings;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;

namespace lokqlDx.Wpf.Views;

/// <summary>
/// Interaction logic for KqlQueryEditorControl.xaml
/// </summary>
public partial class KqlQueryEditorControl : UserControl
{
    private readonly EditorHelper _editorHelper;
    private readonly SchemaIntellisenseProvider _schemaIntellisenseProvider = new();

    private CompletionWindow? _completionWindow;

    private IEnumerable<IntellisenseEntry> _internalCommands = [];
    private IEnumerable<IntellisenseEntry> _settingNames = [];
    private IEnumerable<IntellisenseEntry> _kqlFunctionEntries = [];
    private IEnumerable<IntellisenseEntry> _kqlOperatorEntries = [];

    public KqlQueryEditorControl()
    {
        InitializeComponent();

        _internalCommands = App.ServiceProvider.GetRequiredService<CommandProcessor>().GetVerbs().Select(v => new IntellisenseEntry(v.Key, v.Value, string.Empty));
        _editorHelper = new EditorHelper(Query);
        Query.TextArea.TextEntering += TextArea_TextEntering;
        Query.TextArea.TextEntered += TextArea_TextEntered;
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        using var s = SafeGetResourceStream("SyntaxHighlighting.xml");
        using var reader = new XmlTextReader(s);
        Query.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
        using var functions = SafeGetResourceStream("IntellisenseFunctions.json");
        _kqlFunctionEntries = JsonSerializer.Deserialize<IntellisenseEntry[]>(functions!)!;
        using var ops = SafeGetResourceStream("IntellisenseOperators.json");
        _kqlOperatorEntries = JsonSerializer.Deserialize<IntellisenseEntry[]>(ops!)!;
    }

    /// <summary>
    ///     Gets a resource name independent of namespace
    /// </summary>
    /// <remarks>
    ///     For some reason dotnet publish decides to lower-case the
    ///     namespace in the resource name. In any case, we really don't want to trust
    ///     that the namespace won't change so do a match against the filename
    /// </remarks>
    private static Stream SafeGetResourceStream(string substring)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var availableResources = assembly.GetManifestResourceNames();
        var wanted =
            availableResources.Single(name => name.Contains(substring, StringComparison.CurrentCultureIgnoreCase));
        return assembly.GetManifestResourceStream(wanted)!;
    }

    private void TextArea_TextEntered(object sender, TextCompositionEventArgs e)
    {
        if (_completionWindow != null && !_completionWindow.CompletionList.ListBox.HasItems)
        {
            _completionWindow.Close();
            return;
        }

        if (e.Text == ".")
        {
            //only show completions if we are at the start of a line
            var textToLeft = _editorHelper.TextToLeftOfCaret();
            if (textToLeft.TrimStart() == ".") ShowCompletions(_internalCommands, string.Empty, 0);
            return;
        }

        if (e.Text == "|")
        {
            ShowCompletions(_kqlOperatorEntries, " ", 0);
            return;
        }

        if (e.Text == "@")
        {
            var blockText = GetTextAroundCursor();
            var columns = _schemaIntellisenseProvider.GetColumns(blockText);
            ShowCompletions(columns, string.Empty, 1);
            return;
        }

        if (e.Text == "[")
        {
            var blockText = GetTextAroundCursor();
            var tables = _schemaIntellisenseProvider.GetTables(blockText);
            ShowCompletions(tables, string.Empty, 1);
            return;
        }

        if (e.Text == "$")
        {
            ShowCompletions(_settingNames, string.Empty, 0);
            return;
        }

        if (e.Text == "?")
            ShowCompletions(_kqlFunctionEntries, string.Empty, 1);
    }

    private void TextArea_TextEntering(object sender, TextCompositionEventArgs e)
    {
        if (e.Text.Length > 0 && _completionWindow != null)
            if (!char.IsLetterOrDigit(e.Text[0]))
                // Whenever a non-letter is typed while the completion window is open,
                // insert the currently selected element.
                _completionWindow.CompletionList.RequestInsertion(e);
        // Do not set e.Handled=true.
        // We still want to insert the character that was typed.
    }

    private void Editor_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
            return;

        if (Keyboard.IsKeyDown(Key.RightShift) || Keyboard.IsKeyDown(Key.LeftShift))
        {
            e.Handled = true;
            var query = GetTextAroundCursor();
            if (query.Length > 0)
            {
                RunKqlCommand?.Execute(query);
                RunEvent?.Invoke(this, new QueryEditorRunEventArgs(query));
            }
        }
    }

    private void Editor_PreviewDragEnter(object sender, DragEventArgs e)
    {
        e.Handled = true;
        // Check that the data being dragged is a file
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            // Get an array with the filenames of the files being dragged
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);

            e.Effects = files.Length > 0 ? DragDropEffects.Move : DragDropEffects.None;
        }
        else
        {
            e.Effects = DragDropEffects.None;
        }
    }

    private void Editor_PreviewDragOver(object sender, DragEventArgs e)
    {
        if (IsLoading)
        {
            e.Handled = false;
            return;
        }

        if (e.Data.GetDataPresent(DataFormats.FileDrop))
            e.Handled = true;
    }

    private void Editor_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);

            var newString = files.Select(f => $".{VerbFromExtension(f)} \"{f}\"").JoinAsLines();
            Query.Document.Insert(Query.CaretOffset, newString);
        }

        e.Handled = true;

        string VerbFromExtension(string f)
        {
            return f.EndsWith(".csl")
                ? "run"
                : "load";
        }
    }

    private void ShowCompletions(IEnumerable<IntellisenseEntry> completions, string prefix, int rewind)
    {
        if (!completions.Any())
            return;

        _completionWindow = new CompletionWindow(Query.TextArea)
        {
            CloseWhenCaretAtBeginning = true
        };
        IList<ICompletionData> data = _completionWindow.CompletionList.CompletionData;
        foreach (var k in completions.OrderBy(k => k.Name))
            data.Add(new MyCompletionData(k, prefix, rewind));
        _completionWindow.Show();
        _completionWindow.Closed += delegate { _completionWindow = null; };
    }

    /// <summary>
    ///     searches for lines around the cursor that contain text
    /// </summary>
    /// <remarks>
    ///     This allows us to easily run multi-line queries
    /// </remarks>
    private string GetTextAroundCursor()
    {
        if (Query.SelectionLength > 0)
            return Query.SelectedText.Trim();

        var i = _editorHelper.LineAtCaret().LineNumber;

        var sb = new StringBuilder();

        while (i > 1 && _editorHelper.TextInLine(i - 1).Trim().Length > 0)
            i--;
        while (i <= Query.LineCount && _editorHelper.TextInLine(i).Trim().Length > 0)
        {
            sb.AppendLine(_editorHelper.TextInLine(i));
            i++;
        }

        return sb.ToString().Trim();
    }
}

// For Dependency Property!
public partial class KqlQueryEditorControl
{
    public string QueryText
    {
        get { return (string)GetValue(QueryTextProperty); }
        set { SetValue(QueryTextProperty, value); }
    }

    // Using a DependencyProperty as the backing store for QueryText.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty QueryTextProperty =
        DependencyProperty.Register(nameof(QueryText), typeof(string), typeof(KqlQueryEditorControl), new PropertyMetadata());

    public bool IsLoading
    {
        get { return (bool)GetValue(IsLoadingProperty); }
        set { SetValue(IsLoadingProperty, value); }
    }

    // Using a DependencyProperty as the backing store for IsLoading.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty IsLoadingProperty =
        DependencyProperty.Register(nameof(IsLoading), typeof(bool), typeof(KqlQueryEditorControl), new PropertyMetadata(false));

    public bool EditorUseWordWrap
    {
        get { return (bool)GetValue(UseWordWrapProperty); }
        set { SetValue(UseWordWrapProperty, value); }
    }

    // Using a DependencyProperty as the backing store for UseWordWrap.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty UseWordWrapProperty =
        DependencyProperty.Register(nameof(EditorUseWordWrap), typeof(bool), typeof(KqlQueryEditorControl), new PropertyMetadata(false));

    public bool EditorShowLineNumbers
    {
        get { return (bool)GetValue(ShowLineNumbersProperty); }
        set { SetValue(ShowLineNumbersProperty, value); }
    }

    // Using a DependencyProperty as the backing store for ShowLineNumbers.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty ShowLineNumbersProperty =
        DependencyProperty.Register(nameof(EditorShowLineNumbers), typeof(bool), typeof(KqlQueryEditorControl), new PropertyMetadata(false));

    public double EditorFontSize
    {
        get { return (double)GetValue(EditorFontSizeProperty); }
        set { SetValue(EditorFontSizeProperty, value); }
    }

    // Using a DependencyProperty as the backing store for EditorFontSize.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty EditorFontSizeProperty =
        DependencyProperty.Register(nameof(EditorFontSize), typeof(double), typeof(KqlQueryEditorControl), new PropertyMetadata(20d));

    public FontFamily EditorFontFamily
    {
        get { return (FontFamily)GetValue(EditorFontFamilyProperty); }
        set { SetValue(EditorFontFamilyProperty, value); }
    }

    // Using a DependencyProperty as the backing store for EditorFontFamily.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty EditorFontFamilyProperty =
        DependencyProperty.Register(nameof(EditorFontFamily), typeof(FontFamily), typeof(KqlQueryEditorControl), new PropertyMetadata());

    public ICommand RunKqlCommand
    {
        get { return (ICommand)GetValue(RunKqlCommandProperty); }
        set { SetValue(RunKqlCommandProperty, value); }
    }

    // Using a DependencyProperty as the backing store for RunKqlCommand.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty RunKqlCommandProperty =
        DependencyProperty.Register(nameof(RunKqlCommand), typeof(ICommand), typeof(KqlQueryEditorControl), new PropertyMetadata());


    public event EventHandler<QueryEditorRunEventArgs>? RunEvent;
}


public partial class KqlQueryEditorControl
{
    private class EditorHelper(TextEditor query)
    {
        public TextEditor Query { get; set; } = query;


        public string GetText(DocumentLine line)
        {
            return Query.Document.GetText(line.Offset, line.Length);
        }

        public string TextInLine(int line)
        {
            return GetText(Query.Document.GetLineByNumber(line));
        }

        public DocumentLine LineAtCaret()
        {
            return Query.Document.GetLineByOffset(Query.CaretOffset);
        }

        public string TextToLeftOfCaret()
        {
            var line = LineAtCaret();
            return Query.Document.GetText(line.Offset, Query.CaretOffset - line.Offset);
        }
    }

    private class SchemaIntellisenseProvider
    {
        private readonly SchemaLine[] _dynamicSchema = [];
        private SchemaLine[] _schemaLines = [];

        private IEnumerable<SchemaLine> AllSchemaLines()
        {
            return _schemaLines.Concat(_dynamicSchema);
        }


        private string[] TablesForCommand(string command)
        {
            return AllSchemaLines().Where(s => s.Command == command)
                .Select(s => s.Table)
                .Distinct()
                .ToArray();
        }

        private string[] AllCommands()
        {
            //it's important to order by length so that the longest commands
            //are matched first since the "dynamic" command have an empty string
            //as the command
            return AllSchemaLines().Select(s => s.Command).Distinct()
                .OrderByDescending(s => s.Length)
                .ToArray();
        }

        public IntellisenseEntry[] GetTables(string blockText)
        {
            foreach (var command in AllCommands())
                if (blockText.Contains(command))
                    return TablesForCommand(command)
                        .Select(t => new IntellisenseEntry(t, $"{command} table", ""))
                        .ToArray();

            //no command found so return empty
            return [];
        }

        public IntellisenseEntry[] GetColumns(string blockText)
        {
            foreach (var command in AllCommands())
                if (blockText.Contains(command))
                {
                    var tables = TablesForCommand(command);
                    //TODO this is a bit fuzzy since short table names could be substrings
                    //of longer ones or even keywords.  We should probably use a regex
                    var matchingTables = tables.Where(blockText.Contains).ToArray();
                    var columns = AllSchemaLines().Where(s => s.Command == command)
                        .Where(s => matchingTables.Contains(s.Table))
                        .Select(c => new IntellisenseEntry(c.Column, $"{c.Command} column for {c.Table}", ""))
                        .ToArray();
                    return columns;
                }

            //no command found so return empty
            return [];
        }

        public void SetSchema(SchemaLine[] schema)
        {
            _schemaLines = schema;
        }
    }


    /// Implements AvalonEdit ICompletionData interface to provide the entries in the
    /// completion drop down.
    private class MyCompletionData : ICompletionData
    {
        private readonly IntellisenseEntry _entry;
        private readonly string _prefix;
        private readonly int _rewind;

        public MyCompletionData(IntellisenseEntry entry, string prefix, int rewind)
        {
            _entry = entry;
            _prefix = prefix;
            _rewind = rewind;
        }

        public ImageSource? Image => null;

        public string Text => _entry.Name;
        public object Content => Text;

        public object Description => $@"{_entry.Description}{Environment.NewLine}Usage: {_entry.Syntax}";

        public double Priority => 1d;

        public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            var seg = new TextSegment
            {
                StartOffset = completionSegment.Offset - _rewind,
                Length = completionSegment.Length + _rewind
            };
            textArea.Document.Replace(seg, _prefix + Text);
        }
    }

    private readonly record struct IntellisenseEntry(string Name, string Description, string Syntax);
}


public class QueryEditorRunEventArgs : EventArgs
{
    public string Query { get; }

    public QueryEditorRunEventArgs(string query)
    {
        Query = query ?? string.Empty;
    }
}
