namespace LogViewer;

public interface ILogStream
{
    public string Identifier { get; }
    
    public string FileName { get; }
    
    public bool CanSave { get; }

    /// <summary>
    /// Get the next line from the stream. <br/>
    /// This method will be called to read a new line.
    /// </summary>
    /// <returns>
    /// The next line depends on <c>\n</c>, or <c>null</c> if no content left.
    /// </returns>
    public string? NextLine();

    /// <summary>
    /// Close the stream. <br/>
    /// This method will be called when the stream needs to be closed.
    /// </summary>
    public void Close();
    
    /// <summary>
    /// Reload the stream. <br/>
    /// This method will be called when initialize or restart the stream.
    /// For example, when targeting a file,
    /// calling this method will open a new stream to read the file from the beginning.
    /// </summary>
    public void Reload();
}
