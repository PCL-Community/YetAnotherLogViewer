using System;
using System.IO;
using System.Security;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using MessageBoxImage = AdonisUI.Controls.MessageBoxImage;
using R = LogViewer.Properties.R;

namespace LogViewer;

public abstract class GeneralWindow : Window
{
    protected virtual void OnViewerOpened() {}

    protected void MenuItemShowMainWindow_OnClick(object sender, RoutedEventArgs e) { MainWindow.CreateOrActivate(); }
    protected void MenuItemExit_OnClick(object sender, RoutedEventArgs e) { App.ExitApplication(); }
    protected void MenuItemMaximize_OnClick(object sender, RoutedEventArgs e) { WindowState = WindowState.Maximized; }
    protected void MenuItemMinimize_OnClick(object sender, RoutedEventArgs e) { WindowState = WindowState.Minimized; }
    protected void MenuItemCloseCurrentWindow_OnClick(object sender, RoutedEventArgs e) { Close(); }
    protected void MenuItemCloseAllWindow_OnClick(object sender, RoutedEventArgs e) { App.CloseAllAndShowMain(); }
    protected void MenuItemSaveAll_OnClick(object sender, RoutedEventArgs e) { App.SaveAll(); }
    protected void MenuItemBrowseRepository_OnClick(object sender, RoutedEventArgs e) { App.OpenUrl(R.AppRepository); }

    protected void MenuItemShowAbout_OnClick(object sender, RoutedEventArgs e)
    {
        throw new NotImplementedException();
    }

    protected void MenuItemOpenFile_OnClick(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            DefaultExt = ".log",
            Filter = $"{R.TextLogFileType}|*.log"
        };
        if (dialog.ShowDialog() != true) return;
        var path = dialog.FileName;
        var result = App.OpenViewer(this, () =>
        {
            try
            {
                return new FileLogStream(path);
            }
            catch (Exception ex)
            {
                if (ex is SecurityException or IOException)
                    App.ShowMessage($"{R.ErrorFailedReadFile}: {path}\n{ex.Message}", this, MessageBoxImage.Error);
                else throw;
            }
            return null;
        });
        if (result) OnViewerOpened();
    }

    protected void MenuItemConnect_OnClick(object sender, RoutedEventArgs e)
    {
        // TODO
        
        // OnViewerOpened();
    }
    
    protected void MenuItemOpenSettings_OnClick(object sender, RoutedEventArgs e)
    {
        throw new NotImplementedException();
    }
    
    protected void MenuItemAllWindows_OnClick(object sender, RoutedEventArgs e)
    {
        if (e.OriginalSource is not MenuItem { Header: string header }) return;
        App.ActivateViewer(header);
    }
    
    protected void MenuItemOpenRecent_OnClick(object sender, RoutedEventArgs e)
    {
        throw new NotImplementedException();
    }
    
}
