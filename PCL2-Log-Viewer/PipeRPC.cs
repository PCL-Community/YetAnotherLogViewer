using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading.Tasks;

namespace LogViewer;

// ReSharper disable InconsistentNaming

public enum RPCRequestType { GET, SET, REQ }

public enum RPCResponseStatus { SUCCESS, FAILURE, ERR }

public enum RPCResponseType { EMPTY, TEXT, JSON, BASE64 }

public record RPCRequest(RPCRequestType Type, string Argument, string? Content = null)
{
    public static RPCRequest Get(string prop) => new(RPCRequestType.GET, prop);
    public static RPCRequest Set(string prop, string value) => new(RPCRequestType.SET, prop, value);
    public static RPCRequest Req(string func, string argument, string content) => new(RPCRequestType.REQ, $"{func} {argument}", content);

    public void Request(StreamWriter writer)
    {
        writer.WriteLine($"{Type} {Argument}");
        if (Content != null) writer.WriteLine(Content);
    }

    public async Task RequestAsync(StreamWriter writer)
    {
        await writer.WriteLineAsync($"{Type} {Argument}");
        if (Content != null) await writer.WriteLineAsync(Content);
    }
}

public record RPCResponse(RPCResponseStatus Status, RPCResponseType Type, string? Name = null, string? Content = null)
{
    public static RPCResponse Read(StreamReader reader)
    {
        var header = reader.ReadLine();
        if (header == null) throw new ArgumentException("Failed to read response header");
        var args = header.Split([' '], 3);
        if (args.Length < 2) throw new ArgumentException("Invalid response header");
        
        Enum.TryParse(args[0], true, out RPCResponseStatus status);
        Enum.TryParse(args[1], true, out RPCResponseType type);
        string? name = null;
        if (args.Length > 2) name = args[2];
        
        var buffer = new StringBuilder();
        int tmp;
        while ((tmp = reader.Read()) != 27) buffer.Append((char)tmp);
        var content = buffer.Length == 0 ? null : buffer.ToString();
        
        return new RPCResponse(status, type, name, content);
    }

    public override string ToString()
    {
        var name = Name == null ? "" : $" {Name}";
        var content = Content ?? "";
        var str = $"{Status} {Type.ToString().ToLowerInvariant()}{name}\n{content}";
        return str;
    }
}

public class PipeRPCClient(string pipeName)
{
    private static readonly Encoding PipeEncoding = Encoding.UTF8;
    
    public RPCResponse SendRequest(RPCRequest request)
    {
        var _pipe = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut);
        _pipe.Connect(5000);
        var reader = new StreamReader(_pipe, PipeEncoding);
        var writer = new StreamWriter(_pipe, PipeEncoding);
        request.Request(writer);
        writer.Write((char)27);
        writer.Flush();
        var response = RPCResponse.Read(reader);
        _pipe.Dispose();
        return response;
    }
}
