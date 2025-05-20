using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using MessageBox = AdonisUI.Controls.MessageBox;
using MessageBoxButton = AdonisUI.Controls.MessageBoxButton;
using MessageBoxImage = AdonisUI.Controls.MessageBoxImage;
using MessageBoxResult = AdonisUI.Controls.MessageBoxResult;
using R = LogViewer.Properties.R;

namespace LogViewer;

public partial class ViewerWindow
{
    public ObservableCollection<string> Modules { get; } = [];
    
    public ObservableCollection<LogItem> Logs { get; } = new();
    
    private readonly ILogStream _stream;
    private readonly ILogParser _parser;
    private readonly string _title;
    
    public string Identifier => _stream.Identifier;

    public ViewerWindow(ILogStream stream, ILogParser parser)
    {
        _stream = stream;
        _parser = parser;
        _title = $"{Identifier} - {R.AppTitle}";
        InitializeComponent();
    }

    public ViewerWindow(ILogStream stream) : this(stream, new DefaultLogParser()) {}
    
    private bool _saved = true;
    private bool _saving = false;
    private string? _path;
    
    private void ProcessContentChanged()
    {
        if (!_stream.CanSave) return;
        _saved = false;
        Title = $"* {_title}";
        MenuItemSave.IsEnabled = true;
    }

    private void Save(string path)
    {
        Task.Run(() =>
        {
            // TODO
            Dispatcher.BeginInvoke(() =>
            {
                Title = _title;
                MenuItemSave.IsEnabled = false;
                _saved = true;
            });
        });
    }

    private bool SaveCopy()
    {
        var dialog = new SaveFileDialog
        {
            FileName = _stream.FileName,
            AddExtension = true,
            DefaultExt = ".log",
            Filter = $"{R.TextLogFileType}|*.log"
        };
        var result = dialog.ShowDialog();
        if (result != true) return false;
        Save(_path = dialog.FileName);
        return true;
    }

    public bool ProcessSave()
    {
        if (!_stream.CanSave) return false;
        if (_saved || _saving) return false;
        _saving = true;
        if (_path == null)
        {
            var result = SaveCopy();
            if (!result) return false;
        }
        else
        {
            Save(_path);
        }
        _saving = false;
        return true;
    }

    protected override void OnInitialized(EventArgs e)
    {
        base.OnInitialized(e);
        Title = _title;
        MenuItemSaveCopy.IsEnabled = _stream.CanSave;
        _stream.Reload();
        _parser.BeginParse(_stream, this, item => Dispatcher.BeginInvoke(() =>
        {
            LogItem tmp;
            var count = Logs.Count;
            if (count > 0 && (tmp = Logs.Last()).SimilarTo(item))
            {
                tmp.Repeat++;
                tmp.RepeatLastTime = item.Time;
                Logs.RemoveAt(count - 1);
                item = tmp;
            }
            Logs.Add(item);
            ProcessContentChanged();
            if (Modules.Contains(item.Module)) return;
            Modules.Add(item.Module);
        }));
    }

    protected override void OnActivated(EventArgs e)
    {
        base.OnActivated(e);
        App.ActiveIdentifier = Identifier;
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        base.OnClosing(e);
        if (!_saved)
        {
            var result = MessageBox.Show(this, $"{R.TextSaveFile} {_path ?? _stream.FileName}?", R.AppTitle,
                MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.Cancel);
            if (result == MessageBoxResult.Yes && !ProcessSave() || result == MessageBoxResult.Cancel) e.Cancel = true;
        }
        if (App.ViewerWindows.Count == 1) MainWindow.CreateOrActivate();
    }

    protected override void OnClosed(EventArgs e)
    {
        App.RemoveViewer(Identifier);
        _stream.Close();
        base.OnClosed(e);
    }

    private void MenuItemSave_OnClick(object sender, RoutedEventArgs e) { ProcessSave(); }

    private void MenuItemSaveCopy_OnClick(object sender, RoutedEventArgs e) { SaveCopy(); }
}
