using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.ComponentModel;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Menu;
using System.Windows.Forms.VisualStyles;
using ZedGraph;

namespace imageLab2
{
    public partial class Form1 : Form
    {
        Bitmap image;
        int[] hist;
        public Form1()
        {
            InitializeComponent();
        }

        private void открытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Image files | *.png; *.jpg; *.bmp | All files (*.*) | *.*";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                image  = new Bitmap(dialog.FileName);
                pictureBox1.Image = image;
                pictureBox1.Refresh();
                pictureBox2.Image = image;
                pictureBox2.Refresh();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void сравнитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new Filters();
            Bitmap bmp1 = new Bitmap(pictureBox1.Image);
            bmp1 = filter.ExponentialNoise(bmp1);
            hist = GetHistogramm(bmp1 as Bitmap);
            DrawGraph(filter.erlang, "luminosity", Color.Red);
            pictureBox2.Image = bmp1;
            pictureBox2.Refresh();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void сравнитьToolStripMenuItem1_Click(object sender, EventArgs e)
        {
        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void DrawGraph(double[] values, string text, Color color)
        {
            double[] a_values = new double[values.Length];
            for(int i = 0; i < values.Length; i++)
            {
                a_values[i] = (double)values[i];
            }
            // используем панель для рисования
            GraphPane pane = zedGraphControl1.GraphPane;
            //устанавливаем границы по осям
            pane.XAxis.Scale.Min = 0;
            pane.XAxis.Scale.Max = 255;
            //очищаем список элементов
            pane.CurveList.Clear();
            ZedGraph.BarItem bar = pane.AddBar(text, null, a_values, color);
            pane.BarSettings.MinClusterGap = 0.0f;
            // убираем градиентную заливку
            bar.Bar.Fill.Type = ZedGraph.FillType.Solid;
            // делаем границы столбцов невидимым
            bar.Bar.Border.IsVisible = false;
            //обновляем график
            zedGraphControl1.AxisChange();
            zedGraphControl1.Invalidate();

        }

        private static int[] GetHistogramm(Bitmap image)
        {
            int[] result = new int[256];
            for (int x = 0; x < image.Width; x++)
                for (int y = 0; y < image.Height; y++)
                {
                    int i = (int)(255 * image.GetPixel(x, y).GetBrightness());
                    result[i]++;
                }

            return result;
        }

        private void гармоническоеСреднееToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filters = new Filters();
            Bitmap tempImage = new Bitmap(pictureBox2.Image);
            pictureBox2.Image = filters.HarmonicMean(tempImage);
            pictureBox2.Refresh();
        }

        private void фильтрСреднейТочкиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filters = new Filters();
            Bitmap tempImage = new Bitmap(pictureBox2.Image);
            pictureBox2.Image = filters.FilterMidpoint(tempImage);
            pictureBox2.Refresh();
        }
    }
}
