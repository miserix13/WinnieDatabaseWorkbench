using System.Data;
using System.Diagnostics;
using Microsoft.Data.SqlClient;
using Npgsql;
using MySqlConnector;
using FirebirdSql.Data.FirebirdClient;
using MongoDB.Driver;
using LiteDB;
using DuckDB.NET.Data;
using SurrealDb.Net;
using StackExchange.Redis;
using Qdrant.Client;
using WinnieDatabaseWorkbench.Models;

namespace WinnieDatabaseWorkbench.Services;

public class ConnectionTestService : IConnectionTestService
{
    public async Task<ConnectionTestResult> TestConnectionAsync(ConnectionInfo connection)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            string? serverVersion = connection.DatabaseType switch
            {
                DatabaseType.SqlServer => await TestSqlServerAsync(connection),
                DatabaseType.PostgreSQL => await TestPostgreSqlAsync(connection),
                DatabaseType.MySQL => await TestMySqlAsync(connection),
                DatabaseType.Firebird => await TestFirebirdAsync(connection),
                DatabaseType.MongoDB => await TestMongoDbAsync(connection),
                DatabaseType.LiteDB => await TestLiteDbAsync(connection),
                DatabaseType.DuckDB => await TestDuckDbAsync(connection),
                DatabaseType.SurrealDB => await TestSurrealDbAsync(connection),
                DatabaseType.Redis => await TestRedisAsync(connection),
                DatabaseType.Garnet => await TestGarnetAsync(connection),
                DatabaseType.Chroma => await TestChromaAsync(connection),
                DatabaseType.Qdrant => await TestQdrantAsync(connection),
                _ => throw new NotSupportedException($"Database type {connection.DatabaseType} is not supported")
            };

            stopwatch.Stop();
            
            return new ConnectionTestResult
            {
                Success = true,
                Message = "Connection successful!",
                Duration = stopwatch.Elapsed,
                ServerVersion = serverVersion
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            
            return new ConnectionTestResult
            {
                Success = false,
                Message = $"Connection failed: {ex.Message}",
                Duration = stopwatch.Elapsed
            };
        }
    }

    private async Task<string> TestSqlServerAsync(ConnectionInfo connection)
    {
        var builder = new SqlConnectionStringBuilder
        {
            DataSource = $"{connection.Host},{connection.Port}",
            InitialCatalog = connection.Database,
            TrustServerCertificate = true,
            ConnectTimeout = 10
        };

        if (connection.UseWindowsAuth)
        {
            builder.IntegratedSecurity = true;
        }
        else
        {
            builder.UserID = connection.Username;
            builder.Password = connection.Password;
        }

        using var conn = new SqlConnection(builder.ConnectionString);
        await conn.OpenAsync();
        
        using var cmd = new SqlCommand("SELECT @@VERSION", conn);
        var version = await cmd.ExecuteScalarAsync();
        return version?.ToString() ?? "Unknown";
    }

    private async Task<string> TestPostgreSqlAsync(ConnectionInfo connection)
    {
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = connection.Host,
            Port = connection.Port,
            Database = connection.Database,
            Username = connection.Username,
            Password = connection.Password,
            Timeout = 10
        };

        using var conn = new NpgsqlConnection(builder.ConnectionString);
        await conn.OpenAsync();
        
        using var cmd = new NpgsqlCommand("SELECT version()", conn);
        var version = await cmd.ExecuteScalarAsync();
        return version?.ToString() ?? "Unknown";
    }

    private async Task<string> TestMySqlAsync(ConnectionInfo connection)
    {
        var builder = new MySqlConnectionStringBuilder
        {
            Server = connection.Host,
            Port = (uint)connection.Port,
            Database = connection.Database,
            UserID = connection.Username,
            Password = connection.Password,
            ConnectionTimeout = 10
        };

        using var conn = new MySqlConnection(builder.ConnectionString);
        await conn.OpenAsync();
        
        using var cmd = new MySqlCommand("SELECT VERSION()", conn);
        var version = await cmd.ExecuteScalarAsync();
        return version?.ToString() ?? "Unknown";
    }

    private async Task<string> TestFirebirdAsync(ConnectionInfo connection)
    {
        var builder = new FbConnectionStringBuilder
        {
            DataSource = connection.Host,
            Port = connection.Port,
            Database = connection.Database,
            UserID = connection.Username,
            Password = connection.Password,
            ConnectionTimeout = 10
        };

        using var conn = new FbConnection(builder.ConnectionString);
        await conn.OpenAsync();
        
        return $"Firebird {conn.ServerVersion}";
    }

    private async Task<string> TestMongoDbAsync(ConnectionInfo connection)
    {
        var settings = MongoClientSettings.FromConnectionString(
            BuildMongoConnectionString(connection));
        settings.ServerSelectionTimeout = TimeSpan.FromSeconds(10);
        settings.ConnectTimeout = TimeSpan.FromSeconds(10);

        var client = new MongoClient(settings);
        var database = client.GetDatabase(connection.Database);
        
        // Ping the database to verify connection
        await database.RunCommandAsync<MongoDB.Bson.BsonDocument>(
            new MongoDB.Bson.BsonDocument("ping", 1));
        
        var buildInfo = await database.RunCommandAsync<MongoDB.Bson.BsonDocument>(
            new MongoDB.Bson.BsonDocument("buildInfo", 1));
        
        return $"MongoDB {buildInfo["version"]}";
    }

    private string BuildMongoConnectionString(ConnectionInfo connection)
    {
        if (string.IsNullOrEmpty(connection.Username))
        {
            return $"mongodb://{connection.Host}:{connection.Port}";
        }
        else
        {
            return $"mongodb://{connection.Username}:{connection.Password}@{connection.Host}:{connection.Port}";
        }
    }

    private Task<string> TestLiteDbAsync(ConnectionInfo connection)
    {
        // LiteDB is file-based, just try to open the database
        var filePath = connection.Database;
        
        if (!File.Exists(filePath))
        {
            // Try to create it if it doesn't exist
            using var db = new LiteDatabase(filePath);
            // Just opening is enough to test
        }
        else
        {
            using var db = new LiteDatabase(filePath);
            // Verify we can read from it
            var collections = db.GetCollectionNames();
        }
        
        return Task.FromResult("LiteDB v5.0.21");
    }

    private async Task<string> TestDuckDbAsync(ConnectionInfo connection)
    {
        var filePath = connection.Database;
        var connString = string.IsNullOrEmpty(filePath) ? "DataSource=:memory:" : $"DataSource={filePath}";
        
        using var conn = new DuckDBConnection(connString);
        await conn.OpenAsync();
        
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT version()";
        var version = await cmd.ExecuteScalarAsync();
        
        return $"DuckDB {version}";
    }

    private async Task<string> TestSurrealDbAsync(ConnectionInfo connection)
    {
        var endpoint = $"http://{connection.Host}:{connection.Port}";
        
        using var client = new SurrealDbClient(endpoint);
        
        if (!string.IsNullOrEmpty(connection.Username))
        {
            await client.SignIn(new SurrealDb.Net.Models.Auth.RootAuth
            {
                Username = connection.Username,
                Password = connection.Password
            });
        }
        
        await client.Use(connection.Database, connection.Database);
        
        // Test query to verify connection
        var result = await client.RawQuery("SELECT * FROM type::thing('test', 'test') LIMIT 1");
        
        return "SurrealDB";
    }

    private async Task<string> TestRedisAsync(ConnectionInfo connection)
    {
        var configOptions = new ConfigurationOptions
        {
            EndPoints = { $"{connection.Host}:{connection.Port}" },
            ConnectTimeout = 10000,
            AbortOnConnectFail = false
        };
        
        if (!string.IsNullOrEmpty(connection.Password))
        {
            configOptions.Password = connection.Password;
        }
        
        var redis = await ConnectionMultiplexer.ConnectAsync(configOptions);
        
        try
        {
            var db = redis.GetDatabase();
            
            // Ping to verify connection
            var latency = await db.PingAsync();
            
            // Get server info
            var server = redis.GetServer($"{connection.Host}:{connection.Port}");
            var info = await server.InfoAsync("server");
            
            var versionEntry = info.FirstOrDefault(g => g.Key == "Server");
            var version = versionEntry?.FirstOrDefault(kv => kv.Key == "redis_version").Value ?? "Unknown";
            
            return $"Redis {version}";
        }
        finally
        {
            await redis.CloseAsync();
        }
    }

    private async Task<string> TestGarnetAsync(ConnectionInfo connection)
    {
        // Garnet uses Redis protocol, so we use the same client
        var configOptions = new ConfigurationOptions
        {
            EndPoints = { $"{connection.Host}:{connection.Port}" },
            ConnectTimeout = 10000,
            AbortOnConnectFail = false
        };
        
        if (!string.IsNullOrEmpty(connection.Password))
        {
            configOptions.Password = connection.Password;
        }
        
        var redis = await ConnectionMultiplexer.ConnectAsync(configOptions);
        
        try
        {
            var db = redis.GetDatabase();
            
            // Ping to verify connection
            await db.PingAsync();
            
            return "Garnet (Redis-compatible)";
        }
        finally
        {
            await redis.CloseAsync();
        }
    }

    private async Task<string> TestChromaAsync(ConnectionInfo connection)
    {
        var endpoint = $"http://{connection.Host}:{connection.Port}";
        
        // Test connection with a simple HTTP request
        using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
        var response = await httpClient.GetAsync($"{endpoint}/api/v1/heartbeat");
        response.EnsureSuccessStatusCode();
        
        // Try to get version
        try
        {
            var versionResponse = await httpClient.GetAsync($"{endpoint}/api/v1/version");
            if (versionResponse.IsSuccessStatusCode)
            {
                var version = await versionResponse.Content.ReadAsStringAsync();
                return $"Chroma {version.Trim('\"')}";
            }
        }
        catch
        {
            // Version endpoint might not be available
        }
        
        return "Chroma";
    }

    private async Task<string> TestQdrantAsync(ConnectionInfo connection)
    {
        var endpoint = $"http://{connection.Host}:{connection.Port}";
        var client = new QdrantClient(endpoint);
        
        // Test connection by listing collections
        var collections = await client.ListCollectionsAsync();
        
        return "Qdrant";
    }
}
