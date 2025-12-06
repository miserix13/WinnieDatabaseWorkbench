using System.Windows.Forms;

namespace WinnieDatabaseWorkbench
{
    public class WorkbenchForm : Form
    {
        private void InitializeComponent()
        {
            this.Text = "Database Workbench";
        }

        public WorkbenchForm() :
            base()
        {
            this.InitializeComponent();
        }
    }
}
