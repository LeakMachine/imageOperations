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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace imageLab3
{
    public partial class Form1 : Form
    {
        Bitmap image;
        Hough alg;
        public Form1()
        {
            InitializeComponent();
            alg = new Hough();
        }

        private void открытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Title = "Open Image";
            dlg.Filter = "Image files (*.bmp , *.jpg , *.png, *.gif )|*.bmp;*.jpg;*.png;*.gif";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                pictureBox1.Image = new Bitmap(dlg.FileName);
                pictureBox2.Image = alg.Sobel(new Bitmap(pictureBox1.Image));
            }

            dlg.Dispose();
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

        private void сегментироватьИзображениеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            KMeansImageSegmentation seg = new KMeansImageSegmentation();
            Bitmap tempImage = new Bitmap(pictureBox1.Image);
            pictureBox2.Image = seg.Main(tempImage);
            pictureBox2.Refresh();
        }

        private void алгоритмДляПоискаОкружностейToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image == null) return;

            int r = Convert.ToInt32(textBox2.Text);
            int tr = Convert.ToInt32(textBox1.Text);

            pictureBox3.Image = alg.TransformCircle(new Bitmap(pictureBox2.Image), tr, r);
            pictureBox4.Image = new Bitmap(pictureBox1.Image);

            Bitmap img = new Bitmap(pictureBox3.Image);
            Graphics g = Graphics.FromImage(pictureBox4.Image);
            Pen pen = new Pen(Color.Red, 3);

            Point Size = new Point(pictureBox1.Image.Width, pictureBox1.Image.Height);
            while (true)
            {
                Point pt = alg.SearchCircle(Size, tr);
                if (pt.X == -1) break;
                g.DrawEllipse(pen, pt.X - r, pt.Y - r, r + r, r + r);
            }
            pictureBox4.Refresh();
        }
    }
}
