# Winnie Database Workbench - AI Coding Agent Instructions

## Project Overview
This is a Windows Forms desktop application built with .NET 10.0 targeting Windows 8.0+, designed as a multi-database workbench similar to DBeaver or SQL Server Management Studio. The application provides a unified interface for query editing, visual schema design, and database administration across multiple database systems.

## Architecture & Key Components

### Entry Point & Application Structure
- **Program.cs**: Minimal entry point using top-level statements, directly launches `WorkbenchForm`
- **WorkbenchForm.cs**: Main application window (Windows Forms) - currently minimal skeleton

### Multi-Database Support Strategy
The project includes drivers for 8 different database systems (see `WinnieDatabaseWorkbench.csproj`):
- **SQL Server**: `Microsoft.Data.SqlClient` (v6.1.3)
- **PostgreSQL**: `Npgsql` (v10.0.0)
- **MySQL**: `MySqlConnector` (v2.5.0)
- **DuckDB**: `DuckDB.NET.Data.Full` (v1.4.1)
- **Firebird**: `FirebirdSql.Data.FirebirdClient` (v10.3.4)
- **MongoDB**: `MongoDB.Driver` (v3.5.2)
- **LiteDB**: `LiteDB` (v5.0.21) - embedded NoSQL
- **SurrealDB**: `SurrealDb.Net` (v0.9.0)

**Design Pattern**: When adding database functionality, expect to implement provider-agnostic abstractions that can work across both relational (SQL Server, PostgreSQL, MySQL, DuckDB, Firebird) and document-based (MongoDB, LiteDB, SurrealDB) systems.

## Development Conventions

### .NET Configuration
- **Target Framework**: `net10.0-windows8.0` (latest .NET with Windows Forms)
- **Nullable Reference Types**: Enabled - all code should handle nullability explicitly
- **Implicit Usings**: Enabled - common namespaces auto-imported
- **Debug Configuration**: Uses `DebugType=none` and `WarningLevel=9999` - aggressive error detection without debug symbols

### Code Style Patterns (from existing code)
```csharp
// Constructor pattern used in WorkbenchForm
public WorkbenchForm() :
    base()
{
    this.InitializeComponent();
}

// Namespace structure - single file-level namespace
namespace WinnieDatabaseWorkbench
{
    public class WorkbenchForm : Form { }
}
```

### File Organization
- Root level contains main application files (`Program.cs`, `WorkbenchForm.cs`)
- No separate folders for Models/Views/Controllers yet - architecture is still being established
- Standard .NET project structure with `bin/`, `obj/`, `.vs/` ignored via `.gitignore`
- **Connection configs**: JSON files stored in `%APPDATA%\WinnieDatabaseWorkbench\` for user-specific settings

## Architecture Decisions

### Dependency Injection
- Use DI container for database abstraction layer (consider `Microsoft.Extensions.DependencyInjection`)
- Register database providers as services with appropriate lifetimes
- Abstract database operations behind interfaces for testability and multi-provider support

### Connection Management
- Store connection strings in JSON config files in user's AppData folder
- Path: `Environment.GetFolderPath(SpecialFolder.ApplicationData) + "\WinnieDatabaseWorkbench\connections.json"`
- No third-party UI component libraries - use native Windows Forms controls only
- Consider async file I/O for config operations

## Building & Running

### Build Commands
```powershell
# Build the project
dotnet build

# Run the application
dotnet run

# Publish for deployment
dotnet publish -c Release -r win-x64 --self-contained
```

### Prerequisites
- .NET 10.0 SDK (or latest .NET preview supporting net10.0)
- Windows 8.0 or later (required for Windows Forms)

## Planned Features (from README.md)
When implementing new features, align with these core pillars:
1. **Query Editor** - SQL/query interface for all database types
2. **Visual Designer** - Graphical data definition and schema management
3. **Management/Admin Tooling** - Database administration capabilities

## Key Development Considerations

### When Adding Database Features
- Consider whether the feature applies to relational DBs only, NoSQL only, or both
- Check if ADO.NET patterns apply (SQL Server, PostgreSQL, MySQL, DuckDB, Firebird) vs. native drivers (MongoDB, SurrealDB)
- DuckDB is embedded analytical DB - may need special handling for in-process scenarios

### Windows Forms Best Practices for This Project
- Use constructor-based initialization as shown in `WorkbenchForm`
- Separate `InitializeComponent()` for designer compatibility
- Keep form logic focused - extract database operations to separate classes
- Use native Windows Forms controls only (no third-party libraries like DevExpress/Telerik)
- Leverage async/await for database operations to keep UI responsive

### Async Patterns
- Use `async/await` for all database operations (queries, schema operations, connection tests)
- Update UI on main thread using `Control.Invoke()` or `BeginInvoke()` when needed
- Consider `IProgress<T>` for long-running operations with progress feedback

### Project is in Early Stage
- Current codebase is minimal (Program.cs + basic WorkbenchForm)
- Expect to create foundational architecture for connection management, query execution, and UI components
- Use DI-based database abstraction layer with provider-specific implementations
- Plan for async file I/O when reading/writing connection configs from AppData
