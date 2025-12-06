using WinnieDatabaseWorkbench.Models;

namespace WinnieDatabaseWorkbench.Services;

public interface IConnectionTestService
{
    Task<ConnectionTestResult> TestConnectionAsync(ConnectionInfo connection);
}

public class ConnectionTestResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
    public string? ServerVersion { get; set; }
}
