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
using System.Reflection.Emit;
using imageLab3;
using System.Drawing.Imaging;

namespace imageLab3
{
    public partial class Form1 : Form
    {
        Bitmap image;
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
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void сравнитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filters = new SobelFilter();
            Bitmap tempImage = new Bitmap(pictureBox1.Image);
            pictureBox1.Image = filters.processImage(tempImage);
            pictureBox1.Refresh();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void сравнитьToolStripMenuItem1_Click(object sender, EventArgs e)
        {

        }

        private void сравнитьUIQСреднееToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        public static Bitmap ConvertToFormat(Image image, PixelFormat format)
        {
            Bitmap copy = new Bitmap(image.Width, image.Height, format);
            using (Graphics gr = Graphics.FromImage(copy))
            {
                gr.DrawImage(image, new Rectangle(0, 0, copy.Width, copy.Height));
            }
            return copy;
        }

        private void алгоритДляДвухВложенныхОкружностейToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filters = new SobelFilter();
            Bitmap  tempImage = new Bitmap(pictureBox1.Image);
            pictureBox2.Image = filters.houghCircles(tempImage);
            pictureBox2.Refresh();
        }

        private void сегментироватьИзображениеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            KMeansImageSegmentation seg = new KMeansImageSegmentation();
            Bitmap tempImage = new Bitmap(pictureBox1.Image);
            pictureBox2.Image = seg.Main(tempImage);
            pictureBox2.Refresh();
        }
    }
}
