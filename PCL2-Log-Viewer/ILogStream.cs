namespace LogViewer;

public interface ILogStream
{
    public string Identifier { get; }
    
    public string FileName { get; }
    
    public bool CanSave { get; }

    public string? NextLine();

    public void Close();

    public void Reload();
}
