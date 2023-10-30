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

namespace imageLab1
{
    public partial class Form1 : Form
    {
        Bitmap image;
        Bitmap image2;
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

        private void открытьToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Image files | *.png; *.jpg; *.bmp | All files (*.*) | *.*";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                image2 = new Bitmap(dialog.FileName);
                pictureBox2.Image = image2;
                pictureBox2.Refresh();
            }
        }

        private void сравнитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Functions func = new Functions();
            float res;
            Bitmap bmp1 = new Bitmap(pictureBox1.Image);
            Bitmap bmp2 = new Bitmap(pictureBox2.Image);
            res = func.imageCompareMSE(bmp1, bmp2);
            label1.Text = res.ToString();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void сравнитьToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Functions func = new Functions();
            float res;
            Bitmap bmp1 = new Bitmap(pictureBox1.Image);
            Bitmap bmp2 = new Bitmap(pictureBox2.Image);
            res = func.imageCompareUIQ(bmp1, bmp2);
            label1.Text = res.ToString();
        }

        public static List<Bitmap> SplitBitmap(int w, int h, Bitmap inputImage)
        {
            int blockWidth = inputImage.Width / w;
            int blockHeight = inputImage.Height / h;

            List<Bitmap> outputImages = new List<Bitmap>();

            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    Rectangle destRect = new Rectangle(0, 0, blockWidth, blockHeight);
                    Bitmap block = new Bitmap(blockWidth, blockHeight);
                    using (Graphics g = Graphics.FromImage(block))
                    {
                        Rectangle srcRect = new Rectangle(j * blockWidth, i * blockHeight, blockWidth, blockHeight);
                        g.DrawImage(inputImage, destRect, srcRect, GraphicsUnit.Pixel);
                    }
                    outputImages.Add(block);
                }
            }

            return outputImages;
        }

        private void сравнитьUIQСреднееToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Functions func = new Functions();
            int h = 3;
            int w = 3;
            List<Bitmap> listIMG1 = SplitBitmap(w, h, image);
            List<Bitmap> listIMG2 = SplitBitmap(w, h, image2);
            double sum = 0;
            for (int i = 0; i < w * h; i++)
            {
                sum += func.imageCompareUIQ(listIMG1[i], listIMG2[i]);
            }

            label1.Text = (sum / (w * h)).ToString();
        }
    }
}
