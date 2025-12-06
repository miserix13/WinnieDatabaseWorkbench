using System.Windows.Forms;
using WinnieDatabaseWorkbench.Services;
using WinnieDatabaseWorkbench.UI;

namespace WinnieDatabaseWorkbench
{
    public class WorkbenchForm : Form
    {
        private readonly IConnectionConfigService _configService;
        private MenuStrip _menuStrip = null!;
        private ToolStripMenuItem _menuFile = null!;
        private ToolStripMenuItem _menuConnection = null!;
        private StatusStrip _statusStrip = null!;
        private ToolStripStatusLabel _statusLabel = null!;

        public WorkbenchForm()
        {
            _configService = new ConnectionConfigService();
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Winnie Database Workbench";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Menu Strip
            _menuStrip = new MenuStrip();

            // File Menu
            _menuFile = new ToolStripMenuItem("&File");
            
            var menuFileExit = new ToolStripMenuItem("E&xit");
            menuFileExit.Click += (s, e) => this.Close();
            
            _menuFile.DropDownItems.Add(menuFileExit);

            // Connection Menu
            _menuConnection = new ToolStripMenuItem("&Connection");
            
            var menuConnectionManager = new ToolStripMenuItem("&Connection Manager...");
            menuConnectionManager.Click += MenuConnectionManager_Click;
            menuConnectionManager.ShortcutKeys = Keys.Control | Keys.M;
            
            var menuConnectionNew = new ToolStripMenuItem("&New Connection...");
            menuConnectionNew.Click += MenuConnectionNew_Click;
            menuConnectionNew.ShortcutKeys = Keys.Control | Keys.N;

            _menuConnection.DropDownItems.Add(menuConnectionManager);
            _menuConnection.DropDownItems.Add(menuConnectionNew);

            _menuStrip.Items.Add(_menuFile);
            _menuStrip.Items.Add(_menuConnection);

            // Status Strip
            _statusStrip = new StatusStrip();
            _statusLabel = new ToolStripStatusLabel("Ready");
            _statusStrip.Items.Add(_statusLabel);

            this.Controls.Add(_menuStrip);
            this.Controls.Add(_statusStrip);
            this.MainMenuStrip = _menuStrip;
        }

        private void MenuConnectionManager_Click(object? sender, EventArgs e)
        {
            var dialog = new ConnectionManagerDialog(_configService);
            if (dialog.ShowDialog(this) == DialogResult.OK && dialog.SelectedConnection != null)
            {
                _statusLabel.Text = $"Connected to: {dialog.SelectedConnection.Name}";
                // TODO: Implement actual connection logic
            }
        }

        private void MenuConnectionNew_Click(object? sender, EventArgs e)
        {
            var editorDialog = new ConnectionEditorDialog();
            if (editorDialog.ShowDialog(this) == DialogResult.OK)
            {
                AddConnectionAsync(editorDialog.Connection);
            }
        }

        private async void AddConnectionAsync(Models.ConnectionInfo connection)
        {
            try
            {
                await _configService.AddConnectionAsync(connection);
                _statusLabel.Text = $"Connection '{connection.Name}' saved successfully";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving connection: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
