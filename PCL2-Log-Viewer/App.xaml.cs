using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
using R = LogViewer.Properties.R;

namespace LogViewer;

public partial class App
{
    public App()
    {
        InitializeComponent();
        // new TestWindow().Show();
        LogViewer.MainWindow.CreateOrActivate();
    }

    private void App_OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        ShowException(e.Exception, R.TextReportUnhandledException);
        e.Handled = true;
    }
    
    #region Utils

    public static void ShowException(Exception e, string? msg = null, Window? owner = null)
    {
        var text = msg == null ? e.ToString() : $"{msg}\n\n{e}";
        var caption = $"{R.TextExceptionThrown} - {R.AppTitle}";
        ShowMessage(text, owner, MessageBoxImage.Error, caption);
    }

    public static void ShowMessage(string text, Window? owner = null, MessageBoxImage icon = MessageBoxImage.Information, string? caption = null)
    {
        caption ??= R.AppTitle;
        if (owner == null) MessageBox.Show(text, caption, MessageBoxButton.OK, icon);
        else MessageBox.Show(owner, text, caption, MessageBoxButton.OK, icon);
    }

    public static void OpenUrl(string url, Window? owner = null)
    {
        try
        {
            Process.Start(url);
        }
        catch (Exception e)
        {
            var text = $"{R.ErrorFailedOpenUrl}: {url}";
            if (e is Win32Exception { ErrorCode: -2147467259 }) ShowMessage($"{text}\n{R.ErrorNoDefaultBrowser}", owner, MessageBoxImage.Error);
            else ShowException(e, text, owner);
        }
    }
    
    #endregion

    #region Viewer Window Management

    private static readonly Dictionary<string, ViewerWindow> Viewers = new();
    public static readonly ObservableCollection<string> ViewerIdentifiers = new();
    
    public static Dictionary<string, ViewerWindow>.ValueCollection ViewerWindows => Viewers.Values;
    
    public static string? ActiveIdentifier { get; internal set; }
    
    internal static void RemoveViewer(string identifier)
    {
        Viewers.Remove(identifier);
        ViewerIdentifiers.Remove(identifier);
    }
    
    internal static void ActivateViewer(string identifier)
    {
        if (Viewers.TryGetValue(identifier, out var v)) v.Activate();
    }
    
    public static bool OpenViewer(Window owner, Func<ILogStream?> streamInitializer)
    {
        try
        {
            var stream = streamInitializer();
            if (stream == null) return false;
            var id = stream.Identifier;
            // if existent: activate the window
            if (Viewers.TryGetValue(id, out var v)) { v.Activate(); return true; }
            // else: create new window & add to dictionary
            var viewer = new ViewerWindow(stream);
            viewer.Show();
            Viewers[id] = viewer;
            ViewerIdentifiers.Add(id);
        }
        catch (Exception e)
        {
            ShowException(e, R.TextReportUnhandledException, owner);
            return false;
        }
        return true;
    }

    #endregion

    #region Global Process

    public static void CheckAllClosedAndExit()
    {
        var allClosed = true;
        foreach (var w in Current.Windows)
            if (w is Window { Visibility: Visibility.Visible or Visibility.Collapsed }) allClosed = false;
        if (allClosed) Current.Shutdown();
    }

    public static void CloseAll()
    {
        LogViewer.MainWindow.Create();
        foreach (var w in new List<ViewerWindow>(ViewerWindows)) w.Close();
    }
    
    public static void CloseAllAndShowMain()
    {
        CloseAll();
        LogViewer.MainWindow.ShowAndActivate();
    }
    
    public static void SaveAll()
    {
        foreach (var w in ViewerWindows) w.ProcessSave();
    }

    public static void ExitApplication()
    {
        CloseAll();
        LogViewer.MainWindow.Instance?.Close();
        CheckAllClosedAndExit();
    }
    
    #endregion
    
}
