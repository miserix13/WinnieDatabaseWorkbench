namespace WinnieDatabaseWorkbench.Models;

public class ConnectionInfo
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public DatabaseType DatabaseType { get; set; }
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public string Database { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool UseWindowsAuth { get; set; } = false;
    public Dictionary<string, string> AdditionalProperties { get; set; } = new();
    
    public ConnectionInfo Clone()
    {
        return new ConnectionInfo
        {
            Id = this.Id,
            Name = this.Name,
            DatabaseType = this.DatabaseType,
            Host = this.Host,
            Port = this.Port,
            Database = this.Database,
            Username = this.Username,
            Password = this.Password,
            UseWindowsAuth = this.UseWindowsAuth,
            AdditionalProperties = new Dictionary<string, string>(this.AdditionalProperties)
        };
    }
}
