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
    public partial class GenerationSpeedDialog : Form
    {
        public event GenerationSpeedEventHandler Apply;
        public GenerationSpeedDialog()
        {
            InitializeComponent();

            textBox1.Text = Form1.generationSpeed.ToString();
        }
        public int GenerationSpeed { get; set; }

        private void OK_Click(object sender, EventArgs e)
        {
            // Publish the event if it is not null
            // and pass the information with the custom
            // event arguments class.

            if (Int32.TryParse(textBox1.Text, out int genSpeed))
            {
                GenerationSpeed = genSpeed;
            }
            else
            {
                MessageBox.Show("Please enter a valid integer!", "Something Went Wrong.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Apply = null;
            }

            if (Apply != null)
            {
                Apply(this, new GenerationSpeedEventArgs(this.GenerationSpeed));
            }
        }
    }
}
