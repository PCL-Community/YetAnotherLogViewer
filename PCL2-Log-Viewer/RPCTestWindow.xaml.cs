using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace LogViewer;

public partial class RPCTestWindow
{
    private static RPCTestWindow? _current;

    public static void CreateOrActivate()
    {
        if (_current == null)
        {
            _current = new RPCTestWindow();
            _current.Show();
        }
        else _current.Activate();
    }
    
    public RPCTestWindow()
    {
        InitializeComponent();
    }

    protected override void OnClosed(EventArgs e)
    {
        _current = null;
        base.OnClosed(e);
    }

    private void ButtonSendRequest_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button) return;
        TextBoxResponse.Text = "Waiting for reply...";
        button.IsEnabled = false;
        var pipeClient = new PipeRPCClient(TextBoxPipeName.Text);
        Enum.TryParse(ComboBoxType.Text, out RPCRequestType type);
        var argument = TextBoxArgument.Text;
        var content = TextBoxContent.Text;
        if (content.Length == 0) content = null;
        Task.Run(() =>
        {
            string responseText;
            try
            {
                var request = new RPCRequest(type, argument, content);
                var response = pipeClient.SendRequest(request);
                responseText = response.ToString();
            }
            catch (Exception ex)
            {
                responseText = ex.ToString();
            }
            _ = Dispatcher.BeginInvoke(() =>
            {
                TextBoxResponse.Text = responseText;
                button.IsEnabled = true;
            });
        });
    }
}
