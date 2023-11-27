using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using static System.Net.Mime.MediaTypeNames;
using System.ComponentModel;
using AForge;
using AForge.Imaging;
using AForge.Imaging.Filters;
using AForge.Math.Geometry;



namespace imageLab3
{
    abstract class Filters
    {
        public double[] erlang;
        public int Clamp(int value, int min, int max)
        {
            if (value < min)
                return min;
            if (value > max)
                return max;
            return value;
        }
        public Bitmap SegmentImage(Bitmap originalImage, int numSegments, int maxIterations)
        {
            // Создаем список для хранения центроидов (средних цветов) каждого сегмента
            List<Color> centroids = new List<Color>();

            // Инициализация центроидов случайными цветами из изображения
            Random rand = new Random();
            for (int i = 0; i < numSegments; i++)
            {
                int randX = rand.Next(originalImage.Width);
                int randY = rand.Next(originalImage.Height);
                Color pixelColor = originalImage.GetPixel(randX, randY);
                centroids.Add(pixelColor);
            }

            // Применение K-средних для кластеризации пикселей по цветам
            for (int iteration = 0; iteration < maxIterations; iteration++)
            {
                Dictionary<Color, List<Color>> clusters = new Dictionary<Color, List<Color>>();

                // Создаем список для каждого центроида
                for (int i = 0; i < numSegments; i++)
                {
                    if (i >= 0 && i < centroids.Count && !clusters.ContainsKey(centroids[i]))
                    {
                        clusters.Add(centroids[i], new List<Color>());
                    }
                }

                // Проходим по каждому пикселю изображения
                for (int x = 0; x < originalImage.Width; x++)
                {
                    for (int y = 0; y < originalImage.Height; y++)
                    {
                        Color pixelColor = originalImage.GetPixel(x, y);

                        // Находим ближайший центроид для текущего пикселя
                        Color nearestCentroid = FindNearestCentroid(pixelColor, centroids);

                        // Добавляем текущий пиксель в кластер ближайшего центроида
                        clusters[nearestCentroid].Add(pixelColor);
                    }
                }

                // Обновляем центроиды средними значениями цветов в каждом кластере
                centroids.Clear();
                foreach (var cluster in clusters)
                {
                    if (cluster.Value.Count > 0)
                    {
                        int avgR = 0, avgG = 0, avgB = 0;
                        foreach (var color in cluster.Value)
                        {
                            avgR += color.R;
                            avgG += color.G;
                            avgB += color.B;
                        }
                        avgR /= cluster.Value.Count;
                        avgG /= cluster.Value.Count;
                        avgB /= cluster.Value.Count;
                        centroids.Add(Color.FromArgb(avgR, avgG, avgB));
                    }
                    else
                    {
                        centroids.Add(cluster.Key);
                    }
                }
            }

            // Создаем новое изображение с сегментированными цветами
            Bitmap segmentedImg = new Bitmap(originalImage.Width, originalImage.Height);

            // Заполняем пиксели сегментированным цветом
            for (int x = 0; x < originalImage.Width; x++)
            {
                for (int y = 0; y < originalImage.Height; y++)
                {
                    Color pixelColor = originalImage.GetPixel(x, y);
                    Color nearestCentroid = FindNearestCentroid(pixelColor, centroids);
                    segmentedImg.SetPixel(x, y, nearestCentroid);
                }
            }

            return segmentedImg;
        }

        // Функция для поиска ближайшего центроида для заданного цвета
        private Color FindNearestCentroid(Color color, List<Color> centroids)
        {
            double minDistance = double.MaxValue;
            Color nearestCentroid = centroids[0];

            foreach (var centroid in centroids)
            {
                double distance = Math.Sqrt(
                    Math.Pow(color.R - centroid.R, 2) +
                    Math.Pow(color.G - centroid.G, 2) +
                    Math.Pow(color.B - centroid.B, 2));

                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestCentroid = centroid;
                }
            }

            return nearestCentroid;
        }

        public Bitmap houghCircles(Bitmap bmp2)
        {
            var filter = new FiltersSequence(new IFilter[]
            {
                Grayscale.CommonAlgorithms.BT709,
                new Threshold(0x40)
            });
            Bitmap bmp = filter.Apply(bmp2);
            HoughCircleTransformation circleTransform = new HoughCircleTransformation(10);
            circleTransform.ProcessImage(bmp);
            Bitmap houghCirlceImage = circleTransform.ToBitmap();

            HoughCircle[] circles = circleTransform.GetCirclesByRelativeIntensity(0.5);
            int numCircles = circleTransform.CirclesCount;
            foreach (HoughCircle circle in circles)
            {
                Pen redPen = new Pen(Color.Red, 1);
                using (var graphics = Graphics.FromImage(bmp2))
                {
                    graphics.DrawEllipse(redPen, circle.X, circle.Y, circle.Radius, circle.Radius);
                }
            }
            return bmp2;
        }
        protected abstract Color calculateNewPixelColor(Bitmap sourceImage, int x, int y);
        public Bitmap processImage(Bitmap sourceImage)
        {
            Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);
            for (int i = 0; i < sourceImage.Width; i++)
            {
                for (int j = 0; j < sourceImage.Height; j++)
                {
                    resultImage.SetPixel(i, j, calculateNewPixelColor(sourceImage, i, j));
                }
            }
            return resultImage;
        }
        public int calculateIntensity(Color color)
        {
            return (int)((color.R * 0.36) + (color.G * 0.53) + (color.B * 0.11));
        }

    }

    class MatrixFilter : Filters
    {
        protected float[,] kernel = null;
        protected float[,] kernel2 = null;
        protected MatrixFilter() { }
        public MatrixFilter(float[,] kernel)
        {
            this.kernel = kernel;
        }


        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int radiusX = kernel.GetLength(0) / 2;
            int radiusY = kernel.GetLength(1) / 2;
            float resultR = 0;
            float resultG = 0;
            float resultB = 0;
            for (int l = -radiusY; l <= radiusY; l++)
            {
                for (int k = -radiusX; k <= radiusX; k++)
                {
                    int idX = Clamp(x + k, 0, sourceImage.Width - 1);
                    int idY = Clamp(y + l, 0, sourceImage.Height - 1);
                    Color neighborColor = sourceImage.GetPixel(idX, idY);
                    resultR += neighborColor.R * kernel[k + radiusX, l + radiusY];
                    resultG += neighborColor.G * kernel[k + radiusX, l + radiusY];
                    resultB += neighborColor.B * kernel[k + radiusX, l + radiusY];
                }
            }
            return Color.FromArgb(Clamp((int)resultR, 0, 255), Clamp((int)resultG, 0, 255), Clamp((int)resultB, 0, 255));
        }
    }

    class SobelFilter : MatrixFilter
    {
        public SobelFilter()
        {
            int sizeX = 3;
            int sizeY = 3;
            kernel = new float[sizeX, sizeY];

            kernel[0, 0] = -1;
            kernel[0, 1] = 0;
            kernel[0, 2] = 1;
            kernel[1, 0] = -2;
            kernel[1, 1] = 0;
            kernel[1, 2] = 2;
            kernel[2, 0] = -1;
            kernel[2, 1] = 0;
            kernel[2, 2] = 1;

            kernel2 = new float[sizeX, sizeY];

            kernel2[0, 0] = -1;
            kernel2[0, 1] = -2;
            kernel2[0, 2] = -1;
            kernel2[1, 0] = 0;
            kernel2[1, 1] = 0;
            kernel2[1, 2] = 0;
            kernel2[2, 0] = 1;
            kernel2[2, 1] = 2;
            kernel2[2, 2] = 1;
        }

        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int radiusX = kernel.GetLength(0) / 2;
            int radiusY = kernel.GetLength(1) / 2;
            float resultR = 0;
            float resultG = 0;
            float resultB = 0;
            for (int l = -radiusY; l <= radiusY; l++)
            {
                for (int k = -radiusX; k <= radiusX; k++)
                {
                    int idX = Clamp(x + k, 0, sourceImage.Width - 1);
                    int idY = Clamp(y + l, 0, sourceImage.Height - 1);
                    Color neighborColor = sourceImage.GetPixel(idX, idY);
                    resultR += neighborColor.R * kernel[k + radiusX, l + radiusY];
                    resultG += neighborColor.G * kernel[k + radiusX, l + radiusY];
                    resultB += neighborColor.B * kernel[k + radiusX, l + radiusY];
                }
            }
            for (int l = -radiusY; l <= radiusY; l++)
            {
                for (int k = -radiusX; k <= radiusX; k++)
                {
                    int idX = Clamp(x + k, 0, sourceImage.Width - 1);
                    int idY = Clamp(y + l, 0, sourceImage.Height - 1);
                    Color neighborColor = sourceImage.GetPixel(idX, idY);
                    resultR += neighborColor.R * kernel2[k + radiusX, l + radiusY];
                    resultG += neighborColor.G * kernel2[k + radiusX, l + radiusY];
                    resultB += neighborColor.B * kernel2[k + radiusX, l + radiusY];
                }
            }
            return Color.FromArgb(Clamp((int)resultR, 0, 255), Clamp((int)resultG, 0, 255), Clamp((int)resultB, 0, 255));
        }

    }
}
