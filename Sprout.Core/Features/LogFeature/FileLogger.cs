using Sprout.Core.Common;
using System.IO;
using System.Text;

namespace Sprout.Core.Features.LogFeature;

public class FileLogger : ILogger
{
    private readonly string logPath = Path.Combine(Environment.CurrentDirectory, Const.LogFileName);
    private readonly object _logSync = new();

    public void Log(string message)
    {
        try
        {
            lock (_logSync)
            {
                File.AppendAllText(path: logPath,
                    contents: $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}{Environment.NewLine}",
                    encoding: Encoding.UTF8);
            }
        }
        catch { }
    }
}
