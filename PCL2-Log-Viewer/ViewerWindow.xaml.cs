using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Threading;
using Microsoft.Win32;
using PropertyChanged;
using MessageBox = AdonisUI.Controls.MessageBox;
using MessageBoxButton = AdonisUI.Controls.MessageBoxButton;
using MessageBoxImage = AdonisUI.Controls.MessageBoxImage;
using MessageBoxResult = AdonisUI.Controls.MessageBoxResult;
using R = LogViewer.Properties.R;

namespace LogViewer;

[AddINotifyPropertyChangedInterface]
public partial class ViewerWindow
{
    public ObservableCollection<string> Modules { get; } = [];
    
    public ObservableCollection<LogItem> Logs { get; } = [];
    
    public bool EnableAutoScroll { get; set; } = false;
    
    private readonly ILogStream _stream;
    private readonly ILogParser _parser;
    private readonly string _title;
    
    public string Identifier => _stream.Identifier;

    public ViewerWindow(ILogStream stream, ILogParser parser)
    {
        _stream = stream;
        _parser = parser;
        _title = $"{Identifier} - {R.AppTitle}";
        Title = $"{R.TextLoading} - {R.AppTitle}";
        InitializeComponent();
    }

    public ViewerWindow(ILogStream stream) : this(stream, new DefaultLogParser()) {}

    private ScrollViewer? ScrollViewerLogs => LogContainer.Template.FindName("PART_ScrollViewerLogs", LogContainer) as ScrollViewer;

    private ICollectionView? LogCollectionView => CollectionViewSource.GetDefaultView(LogContainer.ItemsSource);

    private ICollectionView? ModuleTogglesCollectionView => CollectionViewSource.GetDefaultView(ModuleTogglesContainer.ItemsSource);
    
    private bool _saved = true;
    private bool _saving = false;
    private string? _path;
    
    private DispatcherTimer? _pendingLogUpdateTimer;
    private LogItem? _lastLogItem;
    private readonly List<LogItem> _pendingLogs = [];
    
    private void ProcessContentChanged()
    {
        if (_lastLogItem != null) Title = _title;
        if (EnableAutoScroll) ScrollToBottom.Execute();
        // update save state
        if (!_stream.CanSave) return;
        _saved = false;
        Title = $"* {_title}";
        MenuItemSave.IsEnabled = true;
    }

    private void ProcessViewRefresh()
    {
        Dispatcher.BeginInvoke(() => LogCollectionView?.Refresh());
    }

    private void ProcessPendingLogUpdate()
    {
        lock (_pendingLogs)
        {
            if (_pendingLogs.Count == 0) return;
            foreach (var item in _pendingLogs)
            {
                var module = item.Module;
                if (!Modules.Contains(module))
                {
                    Resources[$"ShowModule{module}"] = true;
                    Modules.Add(module);
                }
                Logs.Add(item);
            }
            _pendingLogs.Clear();
            ProcessContentChanged();
        }
    }
    
    public void ResetFilters(bool isLevelOrModule /* true -> Level | false -> Module */ , bool value = false)
    {
        var pfx = isLevelOrModule ? "Level" : "Module";
        pfx = $"Show{pfx}";
        foreach (var k in Resources.Keys) if (k is string s && s.StartsWith(pfx)) Resources[k] = value;
        if (isLevelOrModule) { foreach (UIElement child in LevelTogglesContainer.Children) if (child is ToggleButton button) button.IsChecked = value; }
        else ModuleTogglesCollectionView?.Refresh();
        ProcessViewRefresh();
    }

    public void Save(string path)
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
        MenuItemSaveCopy.IsEnabled = _stream.CanSave;
        LogCollectionView!.Filter = o =>
        {
            if (o is not LogItem item) return false;
            var levelShow = Resources[$"ShowLevel{item.Level}"] as bool?;
            var levelModule = Resources[$"ShowModule{item.Module}"] as bool?;
            return levelShow == true && levelModule == true;
        };
        
        _stream.Reload();
        Task.Run(() => _parser.BeginParse(_stream, this, item =>
        {
            lock (_pendingLogs)
            {
                if (_lastLogItem is {} lastItem && lastItem.SimilarTo(item))
                {
                    if (_pendingLogs.Count > 0) UpdateRepeat();
                    else Dispatcher.BeginInvoke(UpdateRepeat);
                    return;
                    
                    void UpdateRepeat()
                    {
                        lastItem.Repeat++;
                        lastItem.RepeatLastTime = item.Time;
                    }
                }
                
                _pendingLogs.Add(item);
                _lastLogItem = item;
            }
        }));
        
        ProcessPendingLogUpdate();
        _pendingLogUpdateTimer = new DispatcherTimer(
            TimeSpan.FromMilliseconds(50), DispatcherPriority.Background, (_, _) => ProcessPendingLogUpdate(), Dispatcher);
        _pendingLogUpdateTimer.Start();
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
        _pendingLogUpdateTimer?.Stop();
        base.OnClosed(e);
    }

    public SimpleCommand ToggleEnableAutoScroll => new(_ =>
    {
        EnableAutoScroll = !EnableAutoScroll;
        if (EnableAutoScroll) ScrollToBottom.Execute();
    });

    public SimpleCommand ScrollToTop => new(_ => ScrollViewerLogs?.ScrollToTop());

    public SimpleCommand ScrollToBottom => new(_ => ScrollViewerLogs?.ScrollToBottom());

    private void MenuItemSave_OnClick(object sender, RoutedEventArgs e) { ProcessSave(); }

    private void MenuItemSaveCopy_OnClick(object sender, RoutedEventArgs e) { SaveCopy(); }

    private void ToggleButtonLevels_OnHandler(object sender, RoutedEventArgs e)
    {
        if (e.OriginalSource is not ToggleButton button) return;
        Resources[$"ShowLevel{button.Tag}"] = button.IsChecked;
        ProcessViewRefresh();
    }

    private void ToggleButtonModules_OnClick(object sender, RoutedEventArgs e)
    {
        if (e.OriginalSource is not ToggleButton button) return;
        Resources[$"ShowModule{button.Tag}"] = button.IsChecked;
        ProcessViewRefresh();
    }

    private void MenuItemResetFilters_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem { Tag: string parentTag } || e.OriginalSource is not MenuItem { Tag: string tag }) return;
        ResetFilters(parentTag == "L", tag == "S");
    }
}
