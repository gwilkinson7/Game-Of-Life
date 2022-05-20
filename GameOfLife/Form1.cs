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
    public partial class Form1 : Form
    {
        // The universe array
        static bool[,] universe = new bool[10, 10];

        // Drawing colors
        Color gridColor = Color.Black;
        Color cellColor = Color.Gray;

        // The Timer class
        Timer timer = new Timer();

        // Generation count
        int generations = 0;

        public Form1()
        {
            InitializeComponent();

            // Reading properties
            graphicsPanel1.BackColor = Properties.Settings.Default.PanelColor;

            // Setup the timer
            timer.Interval = 100; // milliseconds
            timer.Tick += Timer_Tick;
            timer.Enabled = false; // start timer running
        }

        // Calculate the next generation of cells
        private void NextGeneration()
        {
            bool[,] scratchUniverse = (bool[,])universe.Clone();

            int xLen = universe.GetLength(0);
            int yLen = universe.GetLength(1);

            int count;

            // iterate through universe to find cells that are "alive" and count neighbors
            for (int x = 0; x < xLen; x++)
            {
                for (int y = 0; y < yLen; y++)
                {
                    count = CountNeighborsFinite(x, y);

                    // if living
                    if (universe[x, y] == true)
                    {
                        if (count < 2)
                        {
                            scratchUniverse[x, y] = false;
                        }
                        else if (count > 3)
                        {
                            scratchUniverse[x, y] = false;
                        }
                        else if (count == 2 || count == 3)
                        {
                            continue;
                        }
                    }

                    // if dead
                    else if (universe[x, y] == false)
                    {
                        if (count == 3)
                        {
                            scratchUniverse[x, y] = true;
                        }
                    }
                }
            }

            universe = scratchUniverse;

            // repaint
            graphicsPanel1.Invalidate();

            // Increment generation count
            generations++;

            // Update status strip generations
            toolStripStatusLabelGenerations.Text = "Generations = " + generations.ToString();
        }

        // The event called by the timer every Interval milliseconds.
        private void Timer_Tick(object sender, EventArgs e)
        {
            NextGeneration();
        }

        private void graphicsPanel1_Paint(object sender, PaintEventArgs e)
        {
            // Calculate the width and height of each cell in pixels
            // CELL WIDTH = WINDOW WIDTH / NUMBER OF CELLS IN X
            int cellWidth = graphicsPanel1.ClientSize.Width / universe.GetLength(0);
            // CELL HEIGHT = WINDOW HEIGHT / NUMBER OF CELLS IN Y
            int cellHeight = graphicsPanel1.ClientSize.Height / universe.GetLength(1);

            // A Pen for drawing the grid lines (color, width)
            Pen gridPen = new Pen(gridColor, 1);

            // A Brush for filling living cells interiors (color)
            Brush cellBrush = new SolidBrush(cellColor);

            // Iterate through the universe in the y, top to bottom
            for (int y = 0; y < universe.GetLength(1); y++)
            {
                // Iterate through the universe in the x, left to right
                for (int x = 0; x < universe.GetLength(0); x++)
                {
                    // A rectangle to represent each cell in pixels
                    Rectangle cellRect = Rectangle.Empty;
                    cellRect.X = x * cellWidth;
                    cellRect.Y = y * cellHeight;
                    cellRect.Width = cellWidth;
                    cellRect.Height = cellHeight;

                    // Fill the cell with a brush if alive
                    if (universe[x, y] == true)
                    {
                        e.Graphics.FillRectangle(cellBrush, cellRect);
                    }

                    // Outline the cell with a pen
                    e.Graphics.DrawRectangle(gridPen, cellRect.X, cellRect.Y, cellRect.Width, cellRect.Height);
                }
            }

            // Cleaning up pens and brushes
            gridPen.Dispose();
            cellBrush.Dispose();
        }

        private void graphicsPanel1_MouseClick(object sender, MouseEventArgs e)
        {
            // If the left mouse button was clicked
            if (e.Button == MouseButtons.Left)
            {
                // Calculate the width and height of each cell in pixels
                int cellWidth = graphicsPanel1.ClientSize.Width / universe.GetLength(0);
                int cellHeight = graphicsPanel1.ClientSize.Height / universe.GetLength(1);

                // Calculate the cell that was clicked in
                // CELL X = MOUSE X / CELL WIDTH
                int x = e.X / cellWidth;
                // CELL Y = MOUSE Y / CELL HEIGHT
                int y = e.Y / cellHeight;

                // Toggle the cell's state
                universe[x, y] = !universe[x, y];

                // Tell Windows you need to repaint
                graphicsPanel1.Invalidate();
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private int CountNeighborsFinite(int x, int y)
        {
            int count = 0;

            // get number of cells in the universe
            int xLen = universe.GetLength(0);
            int yLen = universe.GetLength(1);


            // check all cells surrounding the current cell
            for (int yOffset = -1; yOffset <= 1; yOffset++)
            {
                for (int xOffset = -1; xOffset <= 1; xOffset++)
                {
                    int xCheck = x + xOffset;
                    int yCheck = y + yOffset;

                    // skip current cell
                    if (xOffset == 0 && yOffset == 0)
                    {
                        continue;
                    }

                    // skip cells not neighboring the current cell
                    else if (xCheck < 0)
                    {
                        continue;
                    }
                    else if (yCheck < 0)
                    {
                        continue;
                    }

                    // skip cells outside of the universe
                    else if (xCheck >= xLen)
                    {
                        continue;
                    }
                    else if (yCheck >= yLen)
                    {
                        continue;
                    }

                    // if within the bounds of the universe, increase the count by any remaining cells
                    if (universe[xCheck, yCheck] == true) count++;
                }
            }

            return count;
        }

        private int CountNeighborsToroidal(int x, int y)
        {
            int count = 0;
            int xLen = universe.GetLength(0);
            int yLen = universe.GetLength(1);

            for (int yOffset = -1; yOffset <= 1; yOffset++)
            {
                for (int xOffset = -1; xOffset <= 1; xOffset++)
                {
                    int xCheck = x + xOffset;
                    int yCheck = y + yOffset;

                    // skip current cell
                    if (xOffset == 0 && yOffset == 0)
                    {
                        continue;
                    }

                    // if cell is on the left/bottom of the universe, set x/yCheck to the right/top
                    else if (xCheck < 0)
                    {
                        xCheck = xLen - 1;
                    }
                    else if (yCheck < 0)
                    {
                        yCheck = yLen - 1;
                    }

                    // if cell is on the right/top of the universe, set x/yCheck to the left/bottom
                    else if (xCheck >= xLen)
                    {
                        xCheck = 0;
                    }
                    else if (yCheck >= yLen)
                    {
                        yCheck = 0;
                    }

                    // if within the bounds of the universe, increase the count by any remaining cells
                    if (universe[xCheck, yCheck] == true) count++;
                }
            }

            return count;
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            timer.Enabled = true;
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            timer.Enabled = false;
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            NextGeneration();
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int xLen = universe.GetLength(0);
            int yLen = universe.GetLength(1);

            for (int x = 0; x < xLen; x++)
            {
                for (int y = 0; y < yLen; y++)
                {
                    universe[x, y] = false;
                }
            }

            graphicsPanel1.Invalidate();
        }

        private void backgroundColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // create color dialog object
            ColorDialog dialog = new ColorDialog();

            // set the dialog objects color equal to the user selection(s)
            dialog.Color = graphicsPanel1.BackColor;

            // run dialog box
            if(DialogResult.OK == dialog.ShowDialog())
            {
                graphicsPanel1.BackColor = dialog.Color;

                graphicsPanel1.Invalidate();
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Update Property
            Properties.Settings.Default.PanelColor = graphicsPanel1.BackColor;

            // Save memory representation of the file
            Properties.Settings.Default.Save();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            // Update Property
            Properties.Settings.Default.PanelColor = graphicsPanel1.BackColor;

            // Save memory representation of the file
            Properties.Settings.Default.Save();
        }
    }
}
