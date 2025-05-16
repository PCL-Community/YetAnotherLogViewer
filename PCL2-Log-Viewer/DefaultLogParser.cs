using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LogViewer;

public class DefaultLogParser : ILogParser
{
    // regular expressions (really thankful for Claude)
    private static readonly Regex PatternStartWithTime = new(@"^\[\d{2}:\d{2}:\d{2}\.\d{3}\].*$");
    private static readonly Regex PatternStartWithTag = new(@"^\[[\S]+\].*$");
    private static readonly Regex PatternErrorOrFailed = new(@"错误|异常|失败");

    private static readonly char[] SplitSpace = { ' ' };

    private static string[] SplitBySpace(string str)
    {
        return str.Split(SplitSpace, 2, StringSplitOptions.None);
    }

    private static string GetTag(string sp)
    {
        return sp.Substring(1, sp.Length - 2);
    }
    
    public void BeginParse(ILogStream stream, ViewerWindow viewer, Action<LogItem> itemCallback)
    {
        Task.Run(() =>
        {
            string? module = null;
            string? level = null;
            var time = "";
            var content = new StringBuilder();

            var firstLine = true;

            while (stream.NextLine() is { } line)
            {
                try
                {
                    if (PatternStartWithTime.IsMatch(line))
                    {
                        // callback & clean
                        if (firstLine) firstLine = false;
                        else InvokeCallback();
                        
                        // analyze new item
                        var sp = SplitBySpace(line);
                        time = GetTag(sp[0]);
                        if (PatternStartWithTag.IsMatch(sp[1]))
                        {
                            sp = SplitBySpace(sp[1]);
                            module = GetTag(sp[0]);
                        }
                        if (PatternStartWithTag.IsMatch(sp[1]))
                        {
                            sp = SplitBySpace(sp[1]);
                            level = GetTag(sp[0]);
                        }
                        content.Append(sp[1]);
                    }
                    else content.Append('\n').Append(line);
                }
                catch (Exception e)
                {
                    // TODO localization
                    var l = line;
                    viewer.Dispatcher.BeginInvoke(() => App.ShowException(e, $"Failed to parse line: {l}", viewer));
                }
            }
            
            if (!firstLine) InvokeCallback();

            return;

            void InvokeCallback()
            {
                var str = content.ToString();
                if (level == null && PatternErrorOrFailed.IsMatch(str)) level = "Assert";
                itemCallback(new LogItem(time, module ?? "Default", level ?? "Developer", str));
                module = null;
                level = null;
                content.Clear();
            }
        });
    }
}
