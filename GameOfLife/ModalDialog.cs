using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GameOfLife
{
    public partial class ModalDialog : Form
    {
        public event ApplyEventHandler Apply;

        public ModalDialog()
        {
            InitializeComponent();

            textBox1.Text = Form1.universe.GetLength(0).ToString();
            textBox2.Text = Form1.universe.GetLength(1).ToString();
        }

        public int XValue { get; set; }
        public int YValue { get; set; }

        private void OK_Click(object sender, EventArgs e)
        {
            // Publish the event if it is not null
            // and pass the information with the custom
            // event arguments class.

            if (Int32.TryParse(textBox1.Text, out int xResult) && Int32.TryParse(textBox2.Text, out int yResult))
            {
                XValue = xResult;
                YValue = yResult;
            }
            else
            {
                MessageBox.Show("Please enter a valid integer!", "Something Went Wrong.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Apply = null;
            }

            if (Apply != null)
            {
                Apply(this, new ApplyEventArgs(this.XValue, this.YValue));
            }
        }
    }
}
