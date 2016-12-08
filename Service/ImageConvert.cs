using Accord.Imaging.Converters;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace Service
{
    /// <summary>
    /// Przetwarzanie obrazu
    /// </summary>
    public class ImageConvert
    {
        /// <summary>
        /// Przetworzenie obrazu do macierzy
        /// </summary>
        /// <param name="image">Obraz zczytany</param>
        public double[,] BitmapToArray(Bitmap image)
        {
            double[,] result;
            ImageToMatrix conventer = new ImageToMatrix(min: 0, max: 1);
            conventer.Convert(image, out result);

            return result;
        }

        /// <summary>
        /// Przetworzenie macierzy do bitmap
        /// </summary>
        /// <param name="rawImage">Macierz obrazu</param>
        /// <returns>Bitmapa z obrazem</returns>
        public unsafe Bitmap ArrayToBitmap(double[,] rawImage)
        {
            int width = rawImage.GetLength(1);
            int height = rawImage.GetLength(0);

            Bitmap resul_timage = new Bitmap(width, height);
            BitmapData bitmapData = resul_timage.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.ReadWrite,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb
            );

            ColorARGB* startingPosition = (ColorARGB*)bitmapData.Scan0;

            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                {
                    double color = rawImage[i, j];
                    byte rgb = (byte)(color * 255);

                    ColorARGB* position = startingPosition + j + i * width;
                    position->A = 255;
                    position->R = rgb;
                    position->G = rgb;
                    position->B = rgb;
                }

            resul_timage.UnlockBits(bitmapData);
            return resul_timage;
        }

        private struct ColorARGB
        {
            public byte B;
            public byte G;
            public byte R;
            public byte A;

            public ColorARGB(System.Drawing.Color color)
            {
                A = color.A;
                R = color.R;
                G = color.G;
                B = color.B;
            }

            public ColorARGB(byte a, byte r, byte g, byte b)
            {
                A = a;
                R = r;
                G = g;
                B = b;
            }

            public System.Drawing.Color ToColor()
            {
                return System.Drawing.Color.FromArgb(A, R, G, B);
            }
        }

        /// <summary>
        /// BitmapImage to Bitmap
        /// </summary>
        /// <param name="bitmapImage">BitmapImage</param>
        /// <returns>Bitmap</returns>
        public Bitmap BitmapImageToBitmap(BitmapImage bitmapImage)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                Bitmap bitmap = new Bitmap(outStream);

                return new Bitmap(bitmap);
            }
        }

        /// <summary>
        /// Konwersja bitmap do bitmapImage
        /// </summary>
        /// <param name="image">Bitmap</param>
        /// <returns>BitmapImage</returns>
        public BitmapImage BitmapToBitmapImage(Bitmap image)
        {
            BitmapImage result = new BitmapImage();
            using (MemoryStream memory = new MemoryStream())
            {
                image.Save(memory, ImageFormat.Png);
                memory.Position = 0;

                result.BeginInit();
                result.StreamSource = memory;
                result.CacheOption = BitmapCacheOption.OnLoad;
                result.EndInit();
            }

            return result;
        }
    }
}
