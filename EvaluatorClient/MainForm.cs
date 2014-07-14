using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EvaluatorClient
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void conditionsTabControl_Selected(object sender, TabControlEventArgs e)
        {
            if (e.TabPage == resultPage)
            {
                exportButton.Visible = resultListView.Items.Count > 0;
            }
            else
            {
                exportButton.Visible = false;
            }
        }
    }
}
