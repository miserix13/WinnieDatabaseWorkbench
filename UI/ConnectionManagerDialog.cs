using System.Windows.Forms;
using WinnieDatabaseWorkbench.Models;
using WinnieDatabaseWorkbench.Services;

namespace WinnieDatabaseWorkbench.UI;

public class ConnectionManagerDialog : Form
{
    private readonly IConnectionConfigService _configService;
    private List<ConnectionInfo> _connections = new();

    private ListView _lvConnections = null!;
    private Button _btnNew = null!;
    private Button _btnEdit = null!;
    private Button _btnDelete = null!;
    private Button _btnConnect = null!;
    private Button _btnClose = null!;

    public ConnectionInfo? SelectedConnection { get; private set; }

    public ConnectionManagerDialog(IConnectionConfigService configService)
    {
        _configService = configService;
        InitializeComponent();
        LoadConnectionsAsync();
    }

    private void InitializeComponent()
    {
        this.Text = "Connection Manager";
        this.Size = new Size(700, 500);
        this.FormBorderStyle = FormBorderStyle.Sizable;
        this.StartPosition = FormStartPosition.CenterParent;
        this.MinimumSize = new Size(600, 400);

        // ListView
        _lvConnections = new ListView
        {
            Location = new Point(20, 20),
            Size = new Size(540, 400),
            View = View.Details,
            FullRowSelect = true,
            GridLines = true,
            MultiSelect = false,
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
        };

        _lvConnections.Columns.Add("Name", 200);
        _lvConnections.Columns.Add("Database Type", 120);
        _lvConnections.Columns.Add("Host", 150);
        _lvConnections.Columns.Add("Database", 150);

        _lvConnections.SelectedIndexChanged += LvConnections_SelectedIndexChanged;
        _lvConnections.DoubleClick += LvConnections_DoubleClick;
        this.Controls.Add(_lvConnections);

        // Buttons
        int buttonX = 580;
        int buttonY = 20;
        const int buttonWidth = 100;
        const int buttonHeight = 30;
        const int buttonSpacing = 40;

        _btnNew = new Button
        {
            Text = "New",
            Location = new Point(buttonX, buttonY),
            Size = new Size(buttonWidth, buttonHeight),
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        _btnNew.Click += BtnNew_Click;
        this.Controls.Add(_btnNew);
        buttonY += buttonSpacing;

        _btnEdit = new Button
        {
            Text = "Edit",
            Location = new Point(buttonX, buttonY),
            Size = new Size(buttonWidth, buttonHeight),
            Enabled = false,
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        _btnEdit.Click += BtnEdit_Click;
        this.Controls.Add(_btnEdit);
        buttonY += buttonSpacing;

        _btnDelete = new Button
        {
            Text = "Delete",
            Location = new Point(buttonX, buttonY),
            Size = new Size(buttonWidth, buttonHeight),
            Enabled = false,
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        _btnDelete.Click += BtnDelete_Click;
        this.Controls.Add(_btnDelete);
        buttonY += buttonSpacing;

        _btnConnect = new Button
        {
            Text = "Connect",
            Location = new Point(buttonX, buttonY),
            Size = new Size(buttonWidth, buttonHeight),
            Enabled = false,
            Anchor = AnchorStyles.Top | AnchorStyles.Right
        };
        _btnConnect.Click += BtnConnect_Click;
        this.Controls.Add(_btnConnect);

        // Bottom buttons
        _btnClose = new Button
        {
            Text = "Close",
            Location = new Point(580, 430),
            Size = new Size(buttonWidth, buttonHeight),
            DialogResult = DialogResult.Cancel,
            Anchor = AnchorStyles.Bottom | AnchorStyles.Right
        };
        this.Controls.Add(_btnClose);

        this.CancelButton = _btnClose;
    }

    private async void LoadConnectionsAsync()
    {
        try
        {
            _connections = await _configService.GetConnectionsAsync();
            RefreshConnectionsList();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading connections: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void RefreshConnectionsList()
    {
        _lvConnections.Items.Clear();

        foreach (var connection in _connections)
        {
            var item = new ListViewItem(connection.Name);
            item.SubItems.Add(connection.DatabaseType.ToString());
            item.SubItems.Add(connection.Host);
            item.SubItems.Add(connection.Database);
            item.Tag = connection;
            _lvConnections.Items.Add(item);
        }

        UpdateButtonStates();
    }

    private void LvConnections_SelectedIndexChanged(object? sender, EventArgs e)
    {
        UpdateButtonStates();
    }

    private void LvConnections_DoubleClick(object? sender, EventArgs e)
    {
        if (_lvConnections.SelectedItems.Count > 0)
        {
            BtnEdit_Click(sender, e);
        }
    }

    private void UpdateButtonStates()
    {
        bool hasSelection = _lvConnections.SelectedItems.Count > 0;
        _btnEdit.Enabled = hasSelection;
        _btnDelete.Enabled = hasSelection;
        _btnConnect.Enabled = hasSelection;
    }

    private void BtnNew_Click(object? sender, EventArgs e)
    {
        var dialog = new ConnectionEditorDialog();
        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            AddConnectionAsync(dialog.Connection);
        }
    }

    private void BtnEdit_Click(object? sender, EventArgs e)
    {
        if (_lvConnections.SelectedItems.Count == 0 || _lvConnections.SelectedItems[0].Tag is not ConnectionInfo connection)
            return;

        var dialog = new ConnectionEditorDialog(connection);
        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            UpdateConnectionAsync(dialog.Connection);
        }
    }

    private void BtnDelete_Click(object? sender, EventArgs e)
    {
        if (_lvConnections.SelectedItems.Count == 0 || _lvConnections.SelectedItems[0].Tag is not ConnectionInfo connection)
            return;

        var result = MessageBox.Show(
            $"Are you sure you want to delete the connection '{connection.Name}'?",
            "Confirm Delete",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (result == DialogResult.Yes)
        {
            DeleteConnectionAsync(connection.Id);
        }
    }

    private void BtnConnect_Click(object? sender, EventArgs e)
    {
        if (_lvConnections.SelectedItems.Count == 0 || _lvConnections.SelectedItems[0].Tag is not ConnectionInfo connection)
            return;

        SelectedConnection = connection;
        this.DialogResult = DialogResult.OK;
        this.Close();
    }

    private async void AddConnectionAsync(ConnectionInfo connection)
    {
        try
        {
            await _configService.AddConnectionAsync(connection);
            _connections = await _configService.GetConnectionsAsync();
            RefreshConnectionsList();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error adding connection: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void UpdateConnectionAsync(ConnectionInfo connection)
    {
        try
        {
            await _configService.UpdateConnectionAsync(connection);
            _connections = await _configService.GetConnectionsAsync();
            RefreshConnectionsList();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error updating connection: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void DeleteConnectionAsync(string connectionId)
    {
        try
        {
            await _configService.DeleteConnectionAsync(connectionId);
            _connections = await _configService.GetConnectionsAsync();
            RefreshConnectionsList();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error deleting connection: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
