using System;
using System.IO;
using System.IO.Pipes;
using System.Text;

namespace LogViewer;

// source: JetBrains AI Assistant (Claude 4 Sonnet)
public class PipeLogStream(string pipeName) : ILogStream
{
    private NamedPipeClientStream? _pipeClient;
    private StreamReader? _reader;
    private bool _isConnected;
    private readonly StringBuilder _lineBuffer = new();

    public string Identifier { get; } = pipeName;

    public string FileName { get; } = pipeName.Substring(pipeName.IndexOf('/') + 1);

    public bool CanSave => false;

    public string? NextLine()
    {
        if (!_isConnected || _reader == null) return null;

        try
        {
            int tmp;
            while ((tmp = _reader.Read()) != -1)
            {
                var ch = (char)tmp;
                if (ch == '\n')
                {
                    // found the end marker, return line and clear buffer
                    if (_lineBuffer.Length <= 0) continue;
                    var line = _lineBuffer.ToString();
                    _lineBuffer.Clear();
                    return line;
                }
                _lineBuffer.Append(ch);
            }
            
            // reach the end of the stream, return leftover content
            if (_lineBuffer.Length > 0)
            {
                var line = _lineBuffer.ToString();
                _lineBuffer.Clear();
                return line;
            }
        }
        catch (Exception)
        {
            // connection closed or exception occurred
            _isConnected = false;
            return null;
        }

        return null;
    }

    public void Close()
    {
        try
        {
            _reader?.Close();
            _reader?.Dispose();
            _reader = null;

            if (_pipeClient is { IsConnected: true }) _pipeClient.Close();
            _pipeClient?.Dispose();
            _pipeClient = null;

            _isConnected = false;
        }
        catch (Exception) { /* ignored */ }
    }

    public void Reload()
    {
        Close();
        _lineBuffer.Clear();
        Initialize();
    }

    private void Initialize()
    {
        _pipeClient = new NamedPipeClientStream(".", Identifier, PipeDirection.In);
            
        // try connecting to the pipe
        _pipeClient.Connect(5000); // timeout: 5s
            
        _reader = new StreamReader(_pipeClient, Encoding.UTF8);
        _isConnected = true;
    }
}
