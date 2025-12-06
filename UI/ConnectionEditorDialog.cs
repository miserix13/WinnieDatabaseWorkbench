using System.Windows.Forms;
using WinnieDatabaseWorkbench.Models;
using WinnieDatabaseWorkbench.Services;

namespace WinnieDatabaseWorkbench.UI;

public class ConnectionEditorDialog : Form
{
    private readonly ConnectionInfo _connection;
    private readonly bool _isNewConnection;

    private Label _lblName = null!;
    private TextBox _txtName = null!;
    private Label _lblDatabaseType = null!;
    private ComboBox _cmbDatabaseType = null!;
    private Label _lblHost = null!;
    private TextBox _txtHost = null!;
    private Label _lblPort = null!;
    private NumericUpDown _numPort = null!;
    private Label _lblDatabase = null!;
    private TextBox _txtDatabase = null!;
    private Label _lblUsername = null!;
    private TextBox _txtUsername = null!;
    private Label _lblPassword = null!;
    private TextBox _txtPassword = null!;
    private Button _btnOk = null!;
    private Button _btnCancel = null!;
    private Button _btnTestConnection = null!;

    public ConnectionInfo Connection => _connection;

    public ConnectionEditorDialog(ConnectionInfo? connection = null)
    {
        _connection = connection?.Clone() ?? new ConnectionInfo();
        _isNewConnection = connection == null;
        InitializeComponent();
        LoadConnectionData();
    }

    private void InitializeComponent()
    {
        this.Text = _isNewConnection ? "New Connection" : "Edit Connection";
        this.Size = new Size(500, 450);
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.StartPosition = FormStartPosition.CenterParent;

        int yPos = 20;
        const int labelWidth = 100;
        const int controlWidth = 350;
        const int spacing = 35;

        // Name
        _lblName = new Label { Text = "Name:", Location = new Point(20, yPos), Size = new Size(labelWidth, 20) };
        _txtName = new TextBox { Location = new Point(130, yPos), Size = new Size(controlWidth, 20) };
        this.Controls.Add(_lblName);
        this.Controls.Add(_txtName);
        yPos += spacing;

        // Database Type
        _lblDatabaseType = new Label { Text = "Database Type:", Location = new Point(20, yPos), Size = new Size(labelWidth, 20) };
        _cmbDatabaseType = new ComboBox 
        { 
            Location = new Point(130, yPos), 
            Size = new Size(controlWidth, 20),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        _cmbDatabaseType.Items.AddRange(Enum.GetNames(typeof(DatabaseType)));
        _cmbDatabaseType.SelectedIndexChanged += CmbDatabaseType_SelectedIndexChanged;
        this.Controls.Add(_lblDatabaseType);
        this.Controls.Add(_cmbDatabaseType);
        yPos += spacing;

        // Host
        _lblHost = new Label { Text = "Host:", Location = new Point(20, yPos), Size = new Size(labelWidth, 20) };
        _txtHost = new TextBox { Location = new Point(130, yPos), Size = new Size(controlWidth, 20) };
        this.Controls.Add(_lblHost);
        this.Controls.Add(_txtHost);
        yPos += spacing;

        // Port
        _lblPort = new Label { Text = "Port:", Location = new Point(20, yPos), Size = new Size(labelWidth, 20) };
        _numPort = new NumericUpDown 
        { 
            Location = new Point(130, yPos), 
            Size = new Size(controlWidth, 20),
            Minimum = 1,
            Maximum = 65535
        };
        this.Controls.Add(_lblPort);
        this.Controls.Add(_numPort);
        yPos += spacing;

        // Database
        _lblDatabase = new Label { Text = "Database:", Location = new Point(20, yPos), Size = new Size(labelWidth, 20) };
        _txtDatabase = new TextBox { Location = new Point(130, yPos), Size = new Size(controlWidth, 20) };
        this.Controls.Add(_lblDatabase);
        this.Controls.Add(_txtDatabase);
        yPos += spacing;

        // Username
        _lblUsername = new Label { Text = "Username:", Location = new Point(20, yPos), Size = new Size(labelWidth, 20) };
        _txtUsername = new TextBox { Location = new Point(130, yPos), Size = new Size(controlWidth, 20) };
        this.Controls.Add(_lblUsername);
        this.Controls.Add(_txtUsername);
        yPos += spacing;

        // Password
        _lblPassword = new Label { Text = "Password:", Location = new Point(20, yPos), Size = new Size(labelWidth, 20) };
        _txtPassword = new TextBox 
        { 
            Location = new Point(130, yPos), 
            Size = new Size(controlWidth, 20),
            UseSystemPasswordChar = true
        };
        this.Controls.Add(_lblPassword);
        this.Controls.Add(_txtPassword);
        yPos += spacing + 20;

        // Buttons
        _btnTestConnection = new Button 
        { 
            Text = "Test Connection", 
            Location = new Point(20, yPos), 
            Size = new Size(120, 30) 
        };
        _btnTestConnection.Click += BtnTestConnection_Click;
        this.Controls.Add(_btnTestConnection);

        _btnOk = new Button 
        { 
            Text = "OK", 
            Location = new Point(280, yPos), 
            Size = new Size(90, 30),
            DialogResult = DialogResult.OK
        };
        _btnOk.Click += BtnOk_Click;
        this.Controls.Add(_btnOk);

        _btnCancel = new Button 
        { 
            Text = "Cancel", 
            Location = new Point(380, yPos), 
            Size = new Size(90, 30),
            DialogResult = DialogResult.Cancel
        };
        this.Controls.Add(_btnCancel);

        this.AcceptButton = _btnOk;
        this.CancelButton = _btnCancel;
    }

    private void LoadConnectionData()
    {
        _txtName.Text = _connection.Name;
        _cmbDatabaseType.SelectedIndex = (int)_connection.DatabaseType;
        _txtHost.Text = _connection.Host;
        _numPort.Value = _connection.Port > 0 ? _connection.Port : GetDefaultPort(_connection.DatabaseType);
        _txtDatabase.Text = _connection.Database;
        _txtUsername.Text = _connection.Username;
        _txtPassword.Text = _connection.Password;
    }

    private void CmbDatabaseType_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (_cmbDatabaseType.SelectedIndex >= 0)
        {
            var dbType = (DatabaseType)_cmbDatabaseType.SelectedIndex;
            _numPort.Value = GetDefaultPort(dbType);
            UpdateFieldVisibility(dbType);
        }
    }

    private void UpdateFieldVisibility(DatabaseType dbType)
    {
        // LiteDB is file-based, hide host/port/username/password
        bool isFileBased = dbType == DatabaseType.LiteDB;
        
        _lblHost.Visible = !isFileBased;
        _txtHost.Visible = !isFileBased;
        _lblPort.Visible = !isFileBased;
        _numPort.Visible = !isFileBased;
        _lblUsername.Visible = !isFileBased;
        _txtUsername.Visible = !isFileBased;
        _lblPassword.Visible = !isFileBased;
        _txtPassword.Visible = !isFileBased;

        if (isFileBased)
        {
            _lblDatabase.Text = "File Path:";
        }
        else
        {
            _lblDatabase.Text = "Database:";
        }
    }

    private int GetDefaultPort(DatabaseType dbType)
    {
        return dbType switch
        {
            DatabaseType.SqlServer => 1433,
            DatabaseType.PostgreSQL => 5432,
            DatabaseType.MySQL => 3306,
            DatabaseType.DuckDB => 0,
            DatabaseType.Firebird => 3050,
            DatabaseType.MongoDB => 27017,
            DatabaseType.LiteDB => 0,
            DatabaseType.SurrealDB => 8000,
            _ => 0
        };
    }

    private void BtnOk_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_txtName.Text))
        {
            MessageBox.Show("Please enter a connection name.", "Validation Error", 
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            DialogResult = DialogResult.None;
            return;
        }

        _connection.Name = _txtName.Text.Trim();
        _connection.DatabaseType = (DatabaseType)_cmbDatabaseType.SelectedIndex;
        _connection.Host = _txtHost.Text.Trim();
        _connection.Port = (int)_numPort.Value;
        _connection.Database = _txtDatabase.Text.Trim();
        _connection.Username = _txtUsername.Text.Trim();
        _connection.Password = _txtPassword.Text;
    }

    private async void BtnTestConnection_Click(object? sender, EventArgs e)
    {
        _btnTestConnection.Enabled = false;
        _btnTestConnection.Text = "Testing...";

        try
        {
            await Task.Delay(500); // Placeholder for actual connection test
            MessageBox.Show("Connection test functionality will be implemented soon.", 
                "Test Connection", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        finally
        {
            _btnTestConnection.Enabled = true;
            _btnTestConnection.Text = "Test Connection";
        }
    }
}
