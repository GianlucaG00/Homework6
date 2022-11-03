using System.Globalization;
using System.Security.Cryptography;
using static System.Windows.Forms.LinkLabel;

namespace Homework_5
{
    public partial class Form1 : Form
    {
        Random r = new Random(); 
        Bitmap b, b2;
        Graphics g, g2;
        Rectangle virtualWindow, virtualWindow2;

        List<string> lines;

        SortedDictionary<double, long> d = new SortedDictionary<double, long>();

        // distribution of the averages of the samples 
        SortedDictionary<double, long> distAvgSamples = new SortedDictionary<double, long>();

        // distribution of the variances of the sample
        SortedDictionary<double, long> distVarSamples = new SortedDictionary<double, long>();

        List<double> sampleAvgList = new List<double>();
        List<double> sampleVarList = new List<double>();

        bool vertical = false;

        List<double> population = new List<double>(); 

        int x_mouse, y_mouse;
        int x_down, y_down;

        int r_width, r_height;

        bool drag = false;
        bool resizing = false;

        bool pictureBox1_MouseWheel_SR;

        Pen penRectangle = new Pen(Color.Green, 0.2f);

        double avg = 0;
        double var = 0;

        int sampleSize = 10;
        int trialCount = 50;

        int interval_avg = 3; 
        int interval_var = 3;   

        public Form1()
        {
            InitializeComponent();
            b = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            g = Graphics.FromImage(b);

            b2 = new Bitmap(pictureBox2.Width, pictureBox2.Height);
            g2 = Graphics.FromImage(b2); 

            pictureBox1.Image = b;
            pictureBox2.Image = b2;
            
            virtualWindow = new Rectangle(20, 20, b.Width - 200, b.Height - 200);
            virtualWindow2 = new Rectangle(20, 20, b2.Width - 200, b2.Height - 200); 

            timer1.Enabled = true;
            timer1.Interval = 16;
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (!virtualWindow.Contains(e.X, e.Y)) return;

            x_mouse = e.X;
            y_mouse = e.Y;

            x_down = virtualWindow.X;
            y_down = virtualWindow.Y;

            r_width = virtualWindow.Width;
            r_height = virtualWindow.Height;

            if (e.Button == MouseButtons.Left)
            {
                drag = true;
            }
            else if (e.Button == MouseButtons.Right)
            {
                resizing = true;
            }
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            drag = false;
            resizing = false;
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (b == null) return;

            int delta_x = e.X - x_mouse;
            int delta_y = e.Y - y_mouse;



            if (drag)
            {
                virtualWindow.X = x_down + delta_x;
                virtualWindow.Y = y_down + delta_y;
                if (virtualWindow.X < 0) virtualWindow.X = 0;
                if (virtualWindow.Y < 0) virtualWindow.Y = 0;
                if (virtualWindow.X > pictureBox1.Width - virtualWindow.Width) virtualWindow.X = pictureBox1.Width - virtualWindow.Width;
                if (virtualWindow.Y > pictureBox1.Height - virtualWindow.Height) virtualWindow.Y = pictureBox1.Height - virtualWindow.Height;
            }
            else if (resizing)
            {

                virtualWindow.Width = r_width + delta_x;
                virtualWindow.Height = r_height + delta_y;
                if (virtualWindow.Width < 100) virtualWindow.Width = 100;
                if (virtualWindow.Height < 100) virtualWindow.Height = 100;
                if (virtualWindow.Width > pictureBox1.Width - 20) virtualWindow.Width = pictureBox1.Width - 20;
                if (virtualWindow.Height > pictureBox1.Height - 20) virtualWindow.Height = pictureBox1.Height - 20;
            }

        }

        private void pictureBox1_MouseWheel(object sender, MouseEventArgs e)
        {
            if (!(ModifierKeys == Keys.Control)) return;
            if (pictureBox1_MouseWheel_SR) return;

            pictureBox1_MouseWheel_SR = true;

            float stepZoom;
            if (ModifierKeys == (Keys.Shift | Keys.Control))
            {
                stepZoom = 0.01F;
            }
            else
            {
                stepZoom = 0.1F;
            }

            virtualWindow.Inflate((int)(e.Delta * stepZoom), (int)(e.Delta * stepZoom));

            if (virtualWindow.Width < 100) virtualWindow.Width = 100;
            if (virtualWindow.Height < 100) virtualWindow.Height = 100;
            if (virtualWindow.Width > pictureBox1.Width - 20) virtualWindow.Width = pictureBox1.Width - 20;
            if (virtualWindow.Height > pictureBox1.Height - 20) virtualWindow.Height = pictureBox1.Height - 20;
            pictureBox1_MouseWheel_SR = false;

        }

        private void generateHistogram(Rectangle r, SortedDictionary<double, long> d, Graphics g, int interval, bool vertical = false)
        {

            if (d == null || d.Count == 0) return;
            int n = d.Count;


            double maxKey = d.Keys.Max();
            double maxValue = d.Values.Max();
            for (int i = 0; i < n; i++)
            {
                Rectangle rr;
                int left, top, right, bottom;
                if (vertical)
                {
                    // vertical histogram
                    left = fromXRealToXVirtual(0, 0, maxValue, r.Left, r.Width);
                    top = fromYRealToYVirtual(i + 1, 0, n, r.Top, r.Height);
                    right = fromXRealToXVirtual(d.ElementAt(i).Value, 0, maxValue, r.Left, r.Width);
                    bottom = fromYRealToYVirtual(i, 0, n, r.Top, r.Height);
                }
                else
                {
                    //horizontal histogram
                    left = fromXRealToXVirtual(i, 0, n, r.Left, r.Width);
                    top = fromYRealToYVirtual(d.ElementAt(i).Value, 0, maxValue, r.Top, r.Height);
                    right = fromXRealToXVirtual(i + 1, 0, n, r.Left, r.Width);
                    bottom = fromYRealToYVirtual(0, 0, maxValue, r.Top, r.Height);
                }
                rr = Rectangle.FromLTRB(left, top, right, bottom);

                g.DrawRectangle(penRectangle, rr);
                g.FillRectangle(new SolidBrush(Color.FromArgb(180, 200, 89, 0)), rr);

                g.DrawString(vertical ? (d.ElementAt(i).Key*interval).ToString() : d.ElementAt(i).Value.ToString(), DefaultFont, Brushes.Black, r.Right, vertical ? top : top);
                g.DrawString(vertical ? d.ElementAt(i).Value.ToString() : (d.ElementAt(i).Key*interval).ToString(), DefaultFont, Brushes.Black, vertical ? right : left, r.Bottom);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            redraw();
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            interval_avg = trackBar1.Value;
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            interval_var = trackBar2.Value;
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            vertical = checkBox1.Checked;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            population = new List<double>();
            sampleAvgList = new List<double>();
            sampleVarList = new List<double>();
            distAvgSamples = new SortedDictionary<double, long>();
            distVarSamples = new SortedDictionary<double, long>();

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Open a CSV file";
            openFileDialog.Filter = "CSV file|*.csv";
            DialogResult result = openFileDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                string path = openFileDialog.FileName;
                string filename = openFileDialog.SafeFileName;
                textBox1.Text = filename;
                try
                {
                    lines = File.ReadLines(path).ToList();
                    button1.Enabled = true;
                    int i = 0; 
                    foreach (var line in lines.Skip(1))
                    {
                        
                        i++; 
                        var list = line.Split(',');
                        int col = 3;

                        // observed value
                        double key = Double.Parse(list[col], CultureInfo.InvariantCulture);

                        population.Add(key);

                        //double n = (lines.Count() - 1);
                        // avg = avg + (1.0 / i) * (key - avg); 
                        // mean and variance of the population
                        avg = population.Average();
                        var = population.Average(x => Math.Pow(x - avg, 2));


                    }
                }
                catch (IOException)
                {
                }

                for(int j=0; j<trialCount; j++)
                {
                    // take sampleSize elements from the population
                    List<double> sample = population.OrderBy(x => r.Next()).Take(sampleSize).ToList();


                    // we calculate the AVERAGE of the sample
                    double sample_avg = sample.Average();
                    sampleAvgList.Add(sample_avg);
                    int sample_avg_key = ((int)sample_avg) / interval_avg; 
                    // create the key for the mean
                    if (distAvgSamples.ContainsKey(sample_avg_key))
                    {
                        distAvgSamples[sample_avg_key]++; 
                    }
                    else
                    {
                        distAvgSamples.Add(sample_avg_key, 1); 
                    }


                    // we calculate the VARIANCE of the sample
                    double sample_var = sample.Average(x => Math.Pow(x - sample_avg, 2));
                    sampleVarList.Add(sample_var);
                    int sample_var_key = ((int)sample_var) / interval_var;
                    // create the key for the variance
                    if (distVarSamples.ContainsKey(sample_var_key))
                    {
                        distVarSamples[sample_var_key]++;
                    }
                    else
                    {
                        distVarSamples.Add(sample_var_key, 1);
                    }

                }

                double avgOfAvgs = sampleAvgList.Average();
                double avgOfVars = sampleVarList.Average();
                double varOfAvgs = sampleAvgList.Average(x => Math.Pow(x - avgOfAvgs, 2));
                double varOfVars = sampleVarList.Average(x => Math.Pow(x - avgOfVars, 2));


                richTextBox1.AppendText(
                    "AVERAGE OF THE POPULATION: " + avg + Environment.NewLine +
                    "VARIANCE OF THE POPULATION: " + var + Environment.NewLine +
                    "AVEREGE OF THE AVERAGES: " + avgOfAvgs + Environment.NewLine +
                    "AVERAGE OF THE VARIANCES: " + avgOfVars + Environment.NewLine +
                    "VARIANCE OF THE AVERAGES: " + varOfAvgs + Environment.NewLine + 
                    "VARIANCE OF THE VARIANCES: " + varOfVars + Environment.NewLine 
                    ); 
            }
        }

        private int fromXRealToXVirtual(double x, double minX, double maxX, int left, int w)
        {
            return left + (int)(w * (x - minX) / (maxX - minX));
        }

        private int fromYRealToYVirtual(double y, double minY, double maxY, int top, int h)
        {
            return top + (int)(h - h * (y - minY) / (maxY - minY));
        }

        private void redraw()
        {

            g.Clear(BackColor);
            generateHistogram(virtualWindow, distAvgSamples, g, interval_avg,vertical);
            g.DrawRectangle(Pens.DarkSlateGray, virtualWindow);

            g2.Clear(BackColor);
            generateHistogram(virtualWindow2, distVarSamples, g2, interval_var, vertical);
            g2.DrawRectangle(Pens.DarkSlateGray, virtualWindow2);

            pictureBox1.Image = b;
            pictureBox2.Image = b2;
        }
    }
}