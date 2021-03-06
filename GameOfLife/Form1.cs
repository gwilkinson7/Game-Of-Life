using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GameOfLife
{
    public partial class Form1 : Form
    {
        // The universe array
        public static bool[,] universe;

        // Type of Universe
        string universeType;

        // seed for randomizing
        public int seed;

        // Drawing colors
        Color gridColor = Color.Black;
        Color cellColor;

        // The Timer class
        Timer timer = new Timer();

        // Generation count
        int generations = 0;

        // living cells
        int livingCells = 0;

        // show neighbors
        bool showNeighbors;

        // show grid
        bool showGrid;

        // Generation speed
        public static int generationSpeed;

        public Form1()
        {
            InitializeComponent();

            // Reading properties
            graphicsPanel1.BackColor = Properties.Settings.Default.PanelColor;
            cellColor = Properties.Settings.Default.CellColor;
            universeType = Properties.Settings.Default.UniverseType;
            universe = new bool[Properties.Settings.Default.XValue, Properties.Settings.Default.YValue];
            showNeighbors = Properties.Settings.Default.ShowNeighbors;
            showGrid = Properties.Settings.Default.ShowGrid;
            generationSpeed = Properties.Settings.Default.GenerationSpeed;

            // Setup the timer
            timer.Interval = generationSpeed; // milliseconds
            timer.Tick += Timer_Tick;
            timer.Enabled = false; // start timer running

            // default checkmarks
            if (universeType == "Finite")
            {
                linearToolStripMenuItem.Checked = true;
                toroidalToolStripMenuItem.Checked = false;
            }
            else
            {
                linearToolStripMenuItem.Checked = false;
                toroidalToolStripMenuItem.Checked = true;
            }

            if (showNeighbors == true)
            {
                neighborCountToolStripMenuItem.Checked = true;
            }
            else
            {
                neighborCountToolStripMenuItem.Checked = false;
            }

            if (showGrid == true)
            {
                gridToolStripMenuItem.Checked = true;
            }
            else
            {
                gridToolStripMenuItem.Checked = false;
            }
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
                    if (universeType == "Finite")
                    {
                        count = CountNeighborsFinite(x, y);
                    }
                    else
                    {
                        count = CountNeighborsToroidal(x, y);
                    }

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
                            scratchUniverse[x, y] = true;
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

            for (int x = 0; x < universe.GetLength(0); x++)
            {
                for (int y = 0; y < universe.GetLength(1); y++)
                {
                    if (universe[x, y] == true)
                    {
                        livingCells += 1;
                    }
                }
            }

            // Add number of living cells to status strip
            toolStripStatusLabelGenerations.Text += "    Living Cells = " + livingCells;

            livingCells = 0;
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

                    if (showGrid == true)
                    {
                        // Outline the cell with a pen
                        e.Graphics.DrawRectangle(gridPen, cellRect.X, cellRect.Y, cellRect.Width, cellRect.Height);
                    }

                    if (showNeighbors == true)
                    {
                        // Center text inside cells
                        Font font = new Font("Arial", 10f);

                        StringFormat stringFormat = new StringFormat();
                        stringFormat.Alignment = StringAlignment.Center;
                        stringFormat.LineAlignment = StringAlignment.Center;

                        int neighbors;

                        if (universeType == "Finite")
                        {
                            neighbors = CountNeighborsFinite(x, y);
                        }
                        else
                        {
                            neighbors = CountNeighborsToroidal(x, y);
                        }

                        e.Graphics.DrawString(neighbors.ToString(), font, Brushes.White, cellRect, stringFormat);
                    }
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
                    if (xCheck < 0)
                    {
                        xCheck = xLen - 1;
                    }
                    if (yCheck < 0)
                    {
                        yCheck = yLen - 1;
                    }

                    // if cell is on the right/top of the universe, set x/yCheck to the left/bottom
                    if (xCheck >= xLen)
                    {
                        xCheck = 0;
                    }
                    if (yCheck >= yLen)
                    {
                        yCheck = 0;
                    }

                    // if within the bounds of the universe, increase the count by any remaining cells
                    if (universe[xCheck, yCheck] == true) count++;
                }
            }

            return count;
        }

        // Play
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            timer.Enabled = true;
        }

        // Pause
        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            timer.Enabled = false;
        }

        // Next
        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            NextGeneration();
        }

        // New
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

        // Background Color
        private void backgroundColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // create color dialog object
            ColorDialog dialog = new ColorDialog();

            // set the dialog objects color equal to the user selection(s)
            dialog.Color = graphicsPanel1.BackColor;

            // run dialog box
            if (DialogResult.OK == dialog.ShowDialog())
            {
                graphicsPanel1.BackColor = dialog.Color;

                graphicsPanel1.Invalidate();
            }
        }

        // Cell Color
        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            // create color dialog object
            ColorDialog dialog = new ColorDialog();

            // set the dialog objects color equal to the user selection(s)
            dialog.Color = graphicsPanel1.BackColor;

            // run dialog box
            if (DialogResult.OK == dialog.ShowDialog())
            {
                cellColor = dialog.Color;

                graphicsPanel1.Invalidate();
            }
        }

        // Universe Type Toroidal
        private void toroidalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toroidalToolStripMenuItem.Checked = true;
            linearToolStripMenuItem.Checked = false;
            universeType = "Toroidal";
        }

        // Universe Type Finite
        private void linearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toroidalToolStripMenuItem.Checked = false;
            linearToolStripMenuItem.Checked = true;
            universeType = "Finite";
        }

        // On Form Close
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            // Update Property
            Properties.Settings.Default.PanelColor = graphicsPanel1.BackColor;
            Properties.Settings.Default.CellColor = cellColor;
            Properties.Settings.Default.UniverseType = universeType;
            Properties.Settings.Default.XValue = universe.GetLength(0);
            Properties.Settings.Default.YValue = universe.GetLength(1);
            Properties.Settings.Default.ShowNeighbors = showNeighbors;
            Properties.Settings.Default.ShowGrid = showGrid;
            Properties.Settings.Default.GenerationSpeed = generationSpeed;

            // Save memory representation of the file
            Properties.Settings.Default.Save();
        }

        // Reset
        private void resetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.Reset();

            // Reading properties
            graphicsPanel1.BackColor = Properties.Settings.Default.PanelColor;
            cellColor = Properties.Settings.Default.CellColor;
            universeType = Properties.Settings.Default.UniverseType;
            universe = new bool[Properties.Settings.Default.XValue, Properties.Settings.Default.YValue];
            showNeighbors = Properties.Settings.Default.ShowNeighbors;
            showGrid = Properties.Settings.Default.ShowGrid;
            generationSpeed = Properties.Settings.Default.GenerationSpeed;

            // reset checkmarks
            neighborCountToolStripMenuItem.Checked = false;
            toroidalToolStripMenuItem.Checked = false;
            linearToolStripMenuItem.Checked = true;
            gridToolStripMenuItem.Checked = true;

            graphicsPanel1.Invalidate();
        }

        // Reload
        private void reloadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.Reload();

            // Reading properties
            graphicsPanel1.BackColor = Properties.Settings.Default.PanelColor;
            cellColor = Properties.Settings.Default.CellColor;
            universeType = Properties.Settings.Default.UniverseType;
            universe = new bool[Properties.Settings.Default.XValue, Properties.Settings.Default.YValue];
            showNeighbors = Properties.Settings.Default.ShowNeighbors;
            showGrid = Properties.Settings.Default.ShowGrid;
            generationSpeed = Properties.Settings.Default.GenerationSpeed;

            // reset checkmarks
            if (showNeighbors == true)
            {
                neighborCountToolStripMenuItem.Checked = true;
            }
            else
            {
                neighborCountToolStripMenuItem.Checked = false;
            }

            if (showGrid == true)
            {
                gridToolStripMenuItem.Checked = true;
            }
            else
            {
                gridToolStripMenuItem.Checked = false;
            }

            if (universeType == "Finite")
            {
                toroidalToolStripMenuItem.Checked = false;
                linearToolStripMenuItem.Checked = true;
            }
            else
            {
                toroidalToolStripMenuItem.Checked = true;
                linearToolStripMenuItem.Checked = false;
            }

            graphicsPanel1.Invalidate();
        }


        // Save File
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "All Files|*.*|Cells|*.cells";
            dlg.FilterIndex = 2; dlg.DefaultExt = "cells";


            if (DialogResult.OK == dlg.ShowDialog())
            {
                StreamWriter writer = new StreamWriter(dlg.FileName);

                // Write any comments you want to include first.
                // Prefix all comment strings with an exclamation point.
                // Use WriteLine to write the strings to the file. 
                // It appends a CRLF for you.
                writer.WriteLine("!This program was by Garrett Wilkinson.");
                writer.WriteLine("!If you're interested in hiring me, I'm looking for a job!");
                writer.WriteLine("!Check out my github: https://github.com/gwilkinson7");
                writer.WriteLine("!You can also reach out to me at garrettwilkinson7@gmail.com.");

                // Iterate through the universe one row at a time.
                for (int y = 0; y < universe.GetLength(1); y++)
                {
                    // Create a string to represent the current row.
                    String currentRow = string.Empty;

                    // Iterate through the current row one cell at a time.
                    for (int x = 0; x < universe.GetLength(0); x++)
                    {
                        // If the universe[x,y] is alive then append 'O' (capital O)
                        // to the row string.
                        if (universe[x, y] == true)
                        {
                            currentRow += 'O';
                        }

                        // Else if the universe[x,y] is dead then append '.' (period)
                        // to the row string.
                        else if (universe[x, y] == false)
                        {
                            currentRow += '.';
                        }
                    }

                    // Once the current row has been read through and the 
                    // string constructed then write it to the file using WriteLine.
                    writer.WriteLine(currentRow);
                }

                // After all rows and columns have been written then close the file.
                writer.Close();
            }
        }

        // Open File
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "All Files|*.*|Cells|*.cells";
            dlg.FilterIndex = 2;

            if (DialogResult.OK == dlg.ShowDialog())
            {
                StreamReader reader = new StreamReader(dlg.FileName);

                // Create a couple variables to calculate the width and height
                // of the data in the file.
                int maxWidth = 0;
                int maxHeight = 0;

                // Iterate through the file once to get its size.
                while (!reader.EndOfStream)
                {
                    // Read one row at a time.
                    string row = reader.ReadLine();

                    // If the row begins with '!' then it is a comment
                    // and should be ignored.
                    if (row.StartsWith("!"))
                    {
                        continue;
                    }

                    // If the row is not a comment then it is a row of cells.
                    // Increment the maxHeight variable for each row read.
                    maxHeight += 1;

                    // Get the length of the current row string
                    // and adjust the maxWidth variable if necessary.
                    maxWidth = row.Length;
                }

                // Resize the current universe and scratchPad
                // to the width and height of the file calculated above.
                universe = new bool[maxWidth, maxHeight];

                // Reset the file pointer back to the beginning of the file.
                reader.BaseStream.Seek(0, SeekOrigin.Begin);

                int col = -1;

                // Iterate through the file again, this time reading in the cells.
                while (!reader.EndOfStream)
                {
                    // Read one row at a time.
                    string row = reader.ReadLine();

                    // If the row begins with '!' then
                    // it is a comment and should be ignored.
                    if (row.StartsWith("!"))
                    {
                        continue;
                    }

                    col++;

                    // If the row is not a comment then 
                    // it is a row of cells and needs to be iterated through.
                    for (int xPos = 0; xPos < row.Length; xPos++)
                    {
                        // If row[xPos] is a 'O' (capital O) then
                        // set the corresponding cell in the universe to alive.
                        if (row[xPos] == 'O')
                        {
                            universe[xPos, col] = true;
                        }

                        // If row[xPos] is a '.' (period) then
                        // set the corresponding cell in the universe to dead.
                        if (row[xPos] == '.')
                        {
                            universe[xPos, col] = false;
                        }
                    }
                }

                graphicsPanel1.Invalidate();

                // Close the file.
                reader.Close();
            }
        }

        // Import
        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "All Files|*.*|Cells|*.cells";
            dlg.FilterIndex = 2;

            if (DialogResult.OK == dlg.ShowDialog())
            {
                StreamReader reader = new StreamReader(dlg.FileName);

                // Create a couple variables to calculate the width and height
                // of the data in the file.
                int maxWidth = 0;
                int maxHeight = 0;

                // Iterate through the file once to get its size.
                while (!reader.EndOfStream)
                {
                    // Read one row at a time.
                    string row = reader.ReadLine();

                    // If the row begins with '!' then it is a comment
                    // and should be ignored.
                    if (row.StartsWith("!"))
                    {
                        continue;
                    }

                    // If the row is not a comment then it is a row of cells.
                    // Increment the maxHeight variable for each row read.
                    maxHeight += 1;

                    // Get the length of the current row string
                    // and adjust the maxWidth variable if necessary.
                    maxWidth = row.Length;
                }

                // check if imported file can exist in the current universe
                if (maxWidth > universe.GetLength(0) || maxHeight > universe.GetLength(1))
                {
                    // custom error dialogue
                    MessageBox.Show("The universe you are trying to import is bigger than the current universe.", "Something Went Wrong.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Reset the file pointer back to the beginning of the file.
                reader.BaseStream.Seek(0, SeekOrigin.Begin);

                int col = -1;

                // Iterate through the file again, this time reading in the cells.
                while (!reader.EndOfStream)
                {
                    // Read one row at a time.
                    string row = reader.ReadLine();

                    // If the row begins with '!' then
                    // it is a comment and should be ignored.
                    if (row.StartsWith("!"))
                    {
                        continue;
                    }

                    col++;

                    // If the row is not a comment then 
                    // it is a row of cells and needs to be iterated through.
                    for (int xPos = 0; xPos < row.Length; xPos++)
                    {
                        // If row[xPos] is a 'O' (capital O) then
                        // set the corresponding cell in the universe to alive.
                        if (row[xPos] == 'O')
                        {
                            universe[xPos, col] = true;
                        }

                        // If row[xPos] is a '.' (period) then
                        // set the corresponding cell in the universe to dead.
                        if (row[xPos] == '.')
                        {
                            universe[xPos, col] = false;
                        }
                    }
                }

                graphicsPanel1.Invalidate();

                // Close the file.
                reader.Close();
            }
        }

        private void universeSizeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ModalDialog dlg = new ModalDialog();

            // set the properties
            dlg.XValue = universe.GetLength(0);
            dlg.YValue = universe.GetLength(1);

            // subscribe to the Apply event
            dlg.Apply += new ApplyEventHandler(dlg_Apply);

            if (DialogResult.OK == dlg.ShowDialog())
            {
                universe = new bool[dlg.XValue, dlg.YValue];
                graphicsPanel1.Invalidate();
            }
        }

        void dlg_Apply(object sender, ApplyEventArgs e)
        {
            // Retrieve the event arguments
            int x = e.XValue;
            int y = e.YValue;
        }

        void dlg_Randomize_Apply(object sender, ApplyRandomizeEventArgs e)
        {
            // retrieve the event arguments
            seed = e.InternalSeed;
        }

        private void randomizeByTimeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Random rand = new Random();
            int temp;

            for (int x = 0; x < universe.GetLength(0); x++)
            {
                for (int y = 0; y < universe.GetLength(1); y++)
                {
                    temp = rand.Next(0, 3);

                    if (temp == 0)
                    {
                        universe[x, y] = true;
                    }
                    else
                    {
                        universe[x, y] = false;
                    }
                }
            }

            graphicsPanel1.Invalidate();
        }

        private void randomizeBySeedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RandomizeModalDialog dlg = new RandomizeModalDialog();

            // set properties
            dlg.InternalSeed = seed;

            // subscribe to the apply event
            dlg.RandomizeApply += new ApplyRandomizeEventHandler(dlg_Randomize_Apply);

            if (DialogResult.OK == dlg.ShowDialog())
            {
                // get properties
                seed = dlg.InternalSeed;

                Random rand = new Random(seed);
                int temp;

                for (int x = 0; x < universe.GetLength(0); x++)
                {
                    for (int y = 0; y < universe.GetLength(1); y++)
                    {
                        temp = rand.Next(0, 3);

                        if (temp == 0)
                        {
                            universe[x, y] = true;
                        }
                        else
                        {
                            universe[x, y] = false;
                        }
                    }
                }
            }

            graphicsPanel1.Invalidate();

        }

        private void neighborCountToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showNeighbors = !showNeighbors;

            neighborCountToolStripMenuItem.Checked = !neighborCountToolStripMenuItem.Checked;

            graphicsPanel1.Invalidate();
        }

        private void gridToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showGrid = !showGrid;

            gridToolStripMenuItem.Checked = !gridToolStripMenuItem.Checked;

            graphicsPanel1.Invalidate();
        }

        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            GenerationSpeedDialog dlg = new GenerationSpeedDialog();

            // set properties
            dlg.GenerationSpeed = generationSpeed;

            if (DialogResult.OK == dlg.ShowDialog())
            {
                // get properties
                generationSpeed = dlg.GenerationSpeed;
                timer.Interval = generationSpeed;
            }
        }
    }
    public class ApplyEventArgs : EventArgs
    {
        int xValue;
        int yValue;

        public int XValue
        {
            get { return xValue; }
            set { xValue = value; }
        }

        public int YValue
        {
            get { return yValue; }
            set { yValue = value; }
        }

        public ApplyEventArgs(int xValue, int yValue)
        {
            this.xValue = xValue;
            this.yValue = yValue;
        }
    }

    public delegate void ApplyEventHandler(object sender, ApplyEventArgs e);

    public class ApplyRandomizeEventArgs : EventArgs
    {
        int seed;

        public int InternalSeed
        {
            get { return seed; }
            set { seed = value; }
        }

        public ApplyRandomizeEventArgs(int seed)
        {
            this.seed = seed;
        }
    }

    public delegate void ApplyRandomizeEventHandler(object sender, ApplyRandomizeEventArgs e);

    public class GenerationSpeedEventArgs : EventArgs
    {
        int genSpeed;

        public int GenerationSpeed
        {
            get { return genSpeed; }
            set { genSpeed = value; }
        }

        public GenerationSpeedEventArgs(int genSpeed)
        {
            this.genSpeed = genSpeed;
        }
    } 

    public delegate void GenerationSpeedEventHandler(object sender, GenerationSpeedEventArgs e);
}
