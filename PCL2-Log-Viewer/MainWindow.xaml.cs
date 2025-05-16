using System;
using System.Windows;

namespace LogViewer;

public partial class MainWindow
{
    public static MainWindow? Instance { get; private set; }

    public static void Create() { Instance ??= new MainWindow(); }

    public static void ShowAndActivate()
    {
        if (Instance == null) return;
        Instance.Visibility = Visibility.Visible;
        Instance.Activate();
    }

    public static void CreateOrActivate() { Create(); ShowAndActivate(); }
    
    public MainWindow()
    {
        InitializeComponent();
    }

    protected override void OnClosed(EventArgs e)
    {
        Instance = null;
        base.OnClosed(e);
    }

    protected override void OnViewerOpened()
    {
        base.OnViewerOpened();
        Close();
    }
}
