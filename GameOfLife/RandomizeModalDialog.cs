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
    public partial class RandomizeModalDialog : Form
    {
        public event ApplyRandomizeEventHandler RandomizeApply;
        public RandomizeModalDialog()
        {
            InitializeComponent();
        }

        public int InternalSeed { get; set; }

        private void button3_Click(object sender, EventArgs e)
        {
            // Publish the event if it is not null
            // and pass the information with the custom
            // event arguments class.

            if (Int32.TryParse(textBox1.Text, out int result))
            {
                InternalSeed = result;
            }
            else
            {
                MessageBox.Show("Please enter a valid integer!", "Something Went Wrong.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                RandomizeApply = null;
            }

            if (RandomizeApply != null)
            {
                RandomizeApply(this, new ApplyRandomizeEventArgs(this.InternalSeed));
            }
        }
    }
}
