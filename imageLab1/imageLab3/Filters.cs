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



namespace imageLab3
{
    class Hough
    {
        public int[,] Accum;

        /**** Преобразование в полутоновое ****/
        public void GrayScale(Bitmap img)
        {
            for (int y = 0; y < img.Height; y++)
                for (int x = 0; x < img.Width; x++)
                {
                    Color c = img.GetPixel(x, y);
                    /* формула расчета */
                    int px = (int)((c.R * 0.3) + (c.G * 0.59) + (c.B * 0.11));
                    img.SetPixel(x, y, Color.FromArgb(c.A, px, px, px));
                }
        }

        /**** Бинаризация изображения ****/
        public Bitmap Binarization(Bitmap img)
        {
            double threshold = 0.7;
            for (int y = 0; y < img.Height; y++)
                for (int x = 0; x < img.Width; x++)
                    img.SetPixel(x, y, img.GetPixel(x, y).GetBrightness() < threshold ? Color.Black : Color.White);
            return img;
        }

        /**** Выделение краев оператором Собеля ****/
        public Bitmap Sobel(Bitmap src)
        {
            Bitmap dst = new Bitmap(src.Width, src.Height);
            //оператор Собеля
            int[,] dx = { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } };
            int[,] dy = { { 1, 2, 1 }, { 0, 0, 0 }, { -1, -2, -1 } };

            //Преобразование в полутоновое изображение
            GrayScale(src);

            int sumX, sumY, sum;
            //'цикл прохода по всему изображению
            for (int y = 0; y < src.Height - 1; y++)
                for (int x = 0; x < src.Width - 1; x++)
                {
                    sumX = sumY = 0;
                    if (y == 0 || y == src.Height - 1) sum = 0;
                    else if (x == 0 || x == src.Width - 1) sum = 0;
                    else
                    {
                        //цикл свертки оператором Собеля
                        for (int i = -1; i < 2; i++)
                            for (int j = -1; j < 2; j++)
                            {
                                //взять значение пикселя
                                int c = src.GetPixel(x + i, y + j).R;
                                //найти сумму произведений пикселя на значение из матрицы по X
                                sumX += c * dx[i + 1, j + 1];
                                //и сумму произведений пикселя на значение из матрицы по Y
                                sumY += c * dy[i + 1, j + 1];
                            }
                        //найти приближенное значение величины градиента
                        //sum = Math.Abs(sumX) + Math.Abs(sumY);
                        sum = (int)Math.Sqrt(Math.Pow(sumX, 2) + Math.Pow(sumY, 2));
                    }
                    //провести нормализацию
                    if (sum > 255) sum = 255;
                    else if (sum < 0) sum = 0;
                    //записать результат в выходное изображение
                    dst.SetPixel(x, y, Color.FromArgb(255, sum, sum, sum));
                }
            //Binarization(dst);
            return dst;
        }

        /**** Алгоритмы поиска локальных максимумов ****/

        public Point SearchLine(Point Size, int tr)
        {

            int sum = 0, max = 0;
            Point pt = new Point(0, 0);

            for (int y = 0; y < Size.Y; y++)
                for (int x = 0; x < Size.X; x++)
                {
                    sum = 0;
                    if (max < Accum[y, x])
                    {
                        max = Accum[y, x]; pt.X = x; pt.Y = y;
                    }
                }

            if (max < tr) pt.X = -1;
            else Accum[pt.Y, pt.X] = 0;

            return pt;
        }

        public Point SearchCircle(Point Size, int tr)
        {

            int sum = 0, max = 0;
            Point pt = new Point(0, 0);

            for (int y = 1; y < Size.Y - 1; y++)
                for (int x = 1; x < Size.X - 1; x++)
                {
                    sum = 0;
                    for (int i = -1; i <= 1; i++)
                        for (int j = -1; j <= 1; j++)
                            sum += Accum[y + i, x + j];

                    if (max < sum)
                    {
                        max = sum; pt.X = x; pt.Y = y;
                    }
                }

            if (max / 9 < tr) pt.X = -1;
            else
            {
                for (int i = -1; i <= 1; i++)
                    for (int j = -1; j <= 1; j++)
                        Accum[pt.Y + i, pt.X + j] = 0;
            }

            return pt;
        }

        /**** Максимум в аккумуляторе ****/
        public int AccumMax(Point Size)
        {
            int amax = 0;
            for (int y = 0; y < Size.Y; y++)
                for (int x = 0; x < Size.X; x++)
                    if (Accum[y, x] > amax) amax = Accum[y, x];
            return amax;
        }

        /**** Нормализация в аккумуляторе ****/
        public void Normalize(Point Size, int amax)
        {
            for (int y = 0; y < Size.Y; y++)
                for (int x = 0; x < Size.X; x++)
                {
                    int c = (int)(((double)Accum[y, x] / (double)amax) * 255.0);
                    Accum[y, x] = c;
                }
        }

        public Bitmap TransformLine(Bitmap img, int tr)
        {
            Point Size = new Point();
            int mang = 180;

            Size.Y = (int)Math.Round(Math.Sqrt(Math.Pow(img.Width, 2) + Math.Pow(img.Height, 2)));
            Size.X = 180;
            Accum = new int[(int)Size.Y, mang];

            double dt = Math.PI / 180.0;
            for (int y = 0; y < img.Height; y++)
                for (int x = 0; x < img.Width; x++)
                    if (img.GetPixel(x, y).R == 255)
                    {
                        for (int i = 0; i < mang; i++)
                        {
                            int row = (int)Math.Round(x * Math.Cos(dt * (double)i) + y * Math.Sin(dt * (double)i));
                            if (row < Size.Y && row > 0)
                                Accum[row, i]++;
                        }
                    }
            // Поиск максимума
            int amax = AccumMax(Size);
            // Нормализация 
            if (amax != 0)
            {
                img = new Bitmap(Size.X, Size.Y);
                // Нормализация в аккумулятор
                Normalize(Size, amax);
                for (int y = 0; y < Size.Y; y++)
                    for (int x = 0; x < Size.X; x++)
                    {
                        int c = Accum[y, x];
                        img.SetPixel(x, y, Color.FromArgb(c, c, c));
                    }
            }
            return img;
        }

        public Bitmap TransformCircle(Bitmap img, int tr, int r)
        {
            Point Size = new Point(img.Width, img.Height);
            int mang = 360;

            Accum = new int[Size.Y, Size.X];
            double dt = Math.PI / 180.0;

            for (int y = 0; y < img.Height; y++)
                for (int x = 0; x < img.Width; x++)
                    if (img.GetPixel(x, y).R == 255)
                    {
                        for (int i = 0; i < mang; i++)
                        {
                            int Tx = (int)Math.Round(x - r * Math.Cos(dt * (double)i));
                            int Ty = (int)Math.Round(y + r * Math.Sin(dt * (double)i));
                            if ((Tx < Size.X) && (Tx > 0) && (Ty < Size.Y) && (Ty > 0)) Accum[Ty, Tx]++;
                        }
                    }
            // Поиск максимума
            int amax = AccumMax(Size);
            // Нормализация 
            if (amax != 0)
            {
                img = new Bitmap(Size.X, Size.Y);
                // Нормализация в аккумулятор
                Normalize(Size, amax);
                for (int y = 0; y < Size.Y; y++)
                    for (int x = 0; x < Size.X; x++)
                    {
                        int c = Accum[y, x];
                        img.SetPixel(x, y, Color.FromArgb(c, c, c));
                    }
            }
            return img;
        }
    }
    class KMeansImageSegmentation
    {
        public Bitmap Main(Bitmap image)
        {
            int k = 15; // количество сегментов (центроидов)
            int maxIterations = 20; // максимальное количество итераций
            double threshold = 1.0; // пороговое значение изменения центров кластеров

            List<Color> centroids = InitializeCentroids(image, k);
            Dictionary<Color, List<Point>> clusters = new Dictionary<Color, List<Point>>();

            for (int iteration = 0; iteration < maxIterations; iteration++)
            {
                clusters = AssignPixelsToClusters(image, centroids);

                List<Color> oldCentroids = new List<Color>(centroids);
                centroids = UpdateCentroids(image, clusters);

                double maxShift = MaxShift(oldCentroids, centroids);
                if (maxShift < threshold)
                    break;
            }

            ApplyClustersToImage(image, clusters);

            return image;
        }

        static List<Color> InitializeCentroids(Bitmap image, int k)
        {
            List<Color> centroids = new List<Color>();
            Random random = new Random();

            for (int i = 0; i < k; i++)
            {
                int x = random.Next(image.Width);
                int y = random.Next(image.Height);
                centroids.Add(image.GetPixel(x, y));
            }

            return centroids;
        }

        static Dictionary<Color, List<Point>> AssignPixelsToClusters(Bitmap image, List<Color> centroids)
        {
            Dictionary<Color, List<Point>> clusters = new Dictionary<Color, List<Point>>();

            foreach (Color centroid in centroids)
            {
                clusters[centroid] = new List<Point>();
            }

            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    Color pixel = image.GetPixel(x, y);
                    Color closestCentroid = FindClosestCentroid(pixel, centroids);
                    clusters[closestCentroid].Add(new Point(x, y));
                }
            }

            return clusters;
        }

        static Color FindClosestCentroid(Color pixel, List<Color> centroids)
        {
            double minDistance = double.MaxValue;
            Color closest = Color.Empty;

            foreach (Color centroid in centroids)
            {
                double distance = CalculateColorDistance(pixel, centroid);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closest = centroid;
                }
            }

            return closest;
        }

        static double CalculateColorDistance(Color a, Color b)
        {
            double redDiff = a.R - b.R;
            double greenDiff = a.G - b.G;
            double blueDiff = a.B - b.B;

            return Math.Sqrt(redDiff * redDiff + greenDiff * greenDiff + blueDiff * blueDiff);
        }

        static List<Color> UpdateCentroids(Bitmap image, Dictionary<Color, List<Point>> clusters)
        {
            List<Color> centroids = new List<Color>();

            foreach (var cluster in clusters)
            {
                Color newCentroid = CalculateClusterMean(image, cluster.Value);
                centroids.Add(newCentroid);
            }

            return centroids;
        }

        static Color CalculateClusterMean(Bitmap image, List<Point> cluster)
        {
            int totalRed = 0, totalGreen = 0, totalBlue = 0, totalX = 0, totalY = 0;

            foreach (Point point in cluster)
            {
                Color pixelColor = GetPixelColorFromPoint(image, point);
                totalRed += pixelColor.R;
                totalGreen += pixelColor.G;
                totalBlue += pixelColor.B;
                totalX += point.X;
                totalY += point.Y;
            }

            int meanRed = totalRed / cluster.Count;
            int meanGreen = totalGreen / cluster.Count;
            int meanBlue = totalBlue / cluster.Count;
            int meanX = totalX / cluster.Count;
            int meanY = totalY / cluster.Count;

            return Color.FromArgb(meanRed, meanGreen, meanBlue);
        }

        static double MaxShift(List<Color> oldCentroids, List<Color> newCentroids)
        {
            double maxShift = 0;

            int minCount = Math.Min(oldCentroids.Count, newCentroids.Count);

            for (int i = 0; i < minCount; i++)
            {
                double distance = CalculateColorDistance(oldCentroids[i], newCentroids[i]);
                if (distance > maxShift)
                    maxShift = distance;
            }

            return maxShift;
        }

        static void ApplyClustersToImage(Bitmap image, Dictionary<Color, List<Point>> clusters)
        {
            foreach (var cluster in clusters)
            {
                Color clusterColor = cluster.Key;

                foreach (Point point in cluster.Value)
                {
                    image.SetPixel(point.X, point.Y, clusterColor);
                }
            }
        }

        static Color GetPixelColorFromPoint(Bitmap image, Point point)
        {
            if (point.X >= 0 && point.X < image.Width && point.Y >= 0 && point.Y < image.Height)
            {
                return image.GetPixel(point.X, point.Y);
            }
            else
            {
                // Возвращаем пустой цвет в случае выхода за границы изображения
                return Color.Empty;
            }
        }
    }
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
