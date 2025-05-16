namespace LogViewer;

public record LogItem(string Time, string Module, string Level, string Content)
{
    public int Repeat { get; set; } = 1;
    public string RepeatLastTime { get; set; } = "";
    
    public bool SimilarTo(LogItem another)
    {
        return Module == another.Module && Level == another.Level && Content == another.Content;
    }
}
