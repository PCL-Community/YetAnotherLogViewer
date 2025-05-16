using System;

namespace LogViewer;

public interface ILogParser
{
    public void BeginParse(ILogStream stream, ViewerWindow viewer, Action<LogItem> itemCallback);
}
