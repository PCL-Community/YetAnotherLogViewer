using System.IO;

namespace LogViewer;

public class FileLogStream : ILogStream
{
    public string Identifier { get; }

    public string FileName => Path.GetFileName(Identifier);

    public bool CanSave => false;

    private StreamReader? _stream;

    public FileLogStream(string path)
    {
        Identifier = Path.GetFullPath(path);
    }

    public void Close()
    {
        _stream?.Close();
    }

    public string? NextLine()
    {
        return _stream?.ReadLine();
    }

    public void Reload()
    {
        _stream = new StreamReader(Identifier);
    }
}
