using System.Text.Json;
using WinnieDatabaseWorkbench.Models;

namespace WinnieDatabaseWorkbench.Services;

public interface IConnectionConfigService
{
    Task<List<ConnectionInfo>> GetConnectionsAsync();
    Task SaveConnectionsAsync(List<ConnectionInfo> connections);
    Task AddConnectionAsync(ConnectionInfo connection);
    Task UpdateConnectionAsync(ConnectionInfo connection);
    Task DeleteConnectionAsync(string connectionId);
}

public class ConnectionConfigService : IConnectionConfigService
{
    private readonly string _configDirectory;
    private readonly string _configFilePath;
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true
    };

    public ConnectionConfigService()
    {
        _configDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "WinnieDatabaseWorkbench");
        _configFilePath = Path.Combine(_configDirectory, "connections.json");
    }

    public async Task<List<ConnectionInfo>> GetConnectionsAsync()
    {
        if (!Directory.Exists(_configDirectory))
        {
            Directory.CreateDirectory(_configDirectory);
        }

        if (!File.Exists(_configFilePath))
        {
            return new List<ConnectionInfo>();
        }

        try
        {
            var json = await File.ReadAllTextAsync(_configFilePath);
            var config = JsonSerializer.Deserialize<ConnectionsConfig>(json);
            return config?.Connections ?? new List<ConnectionInfo>();
        }
        catch
        {
            return new List<ConnectionInfo>();
        }
    }

    public async Task SaveConnectionsAsync(List<ConnectionInfo> connections)
    {
        if (!Directory.Exists(_configDirectory))
        {
            Directory.CreateDirectory(_configDirectory);
        }

        var config = new ConnectionsConfig { Connections = connections };
        var json = JsonSerializer.Serialize(config, _jsonOptions);
        await File.WriteAllTextAsync(_configFilePath, json);
    }

    public async Task AddConnectionAsync(ConnectionInfo connection)
    {
        var connections = await GetConnectionsAsync();
        connections.Add(connection);
        await SaveConnectionsAsync(connections);
    }

    public async Task UpdateConnectionAsync(ConnectionInfo connection)
    {
        var connections = await GetConnectionsAsync();
        var index = connections.FindIndex(c => c.Id == connection.Id);
        if (index >= 0)
        {
            connections[index] = connection;
            await SaveConnectionsAsync(connections);
        }
    }

    public async Task DeleteConnectionAsync(string connectionId)
    {
        var connections = await GetConnectionsAsync();
        connections.RemoveAll(c => c.Id == connectionId);
        await SaveConnectionsAsync(connections);
    }
}
