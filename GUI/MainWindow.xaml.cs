using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;
using System.IO;
using Service;
using MahApps.Metro.Controls;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Accord.Imaging.Converters;

namespace GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private double[,] colorMatrix;
        private MedianaFilter _medianaFilter;
        private ImageConvert _imageConvert;
        private BitmapImage InputImage;
        public MainWindow()
        {
            InitializeComponent();
            colorMatrix = new double[,] { { 1, 4, 0, 1, 3, 1 }, { 2, 2, 4, 2, 2, 3 }, { 1, 0, 1, 0, 1, 0 }, { 1, 2, 1, 0, 2, 2 }, { 2, 5, 3, 1, 2, 5 }, { 1, 1, 4, 2, 3, 0 } };
            _imageConvert = new ImageConvert();
        }

        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog();
            op.Title = "Select a picture";
            op.Filter = "All supported graphics|*.jpg;*.jpeg;*.png;*.bmp|" +
              "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
              "Portable Network Graphic (*.png)|*.png";
            if (op.ShowDialog() == true)
            {
                InputImage = new BitmapImage(new Uri(op.FileName));
                imgBase.Source = InputImage;

                try
                {
                    _imageConvert.sth();

                    //var imagesss = _medianaFilter.SeqStart();
                    btnFilter.IsEnabled = true;
                    btnFilterSync.IsEnabled = true;  
                    //
                }
                catch (Exception ex)
                {
                    btnFilter.IsEnabled = false;
                    btnFilterSync.IsEnabled = false;
                    throw;
                }

            }


        }

        public byte[,] function(Bitmap image)
        {
            byte[] arr1D;

            BitmapData data = image.LockBits(new System.Drawing.Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, image.PixelFormat);
            try
            {
                IntPtr ptr = data.Scan0;
                int bytes = Math.Abs(data.Stride) * image.Height;
                byte[] rgbValues = new byte[bytes];
                arr1D = rgbValues;
                Marshal.Copy(ptr, rgbValues, 0, bytes);
            }
            finally
            {
                image.UnlockBits(data);
            }
            var b1 = ConvertArray(arr1D, image.Width);
            return b1;
        }

        //
        private unsafe Bitmap ToBitmap(double[,] rawImage)
        {
            int width = rawImage.GetLength(1);
            int height = rawImage.GetLength(0);

            Bitmap Image = new Bitmap(width, height);
            BitmapData bitmapData = Image.LockBits(
                new System.Drawing.Rectangle(0, 0, width, height),
                ImageLockMode.ReadWrite,
                PixelFormat.Format32bppArgb
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

            Image.UnlockBits(bitmapData);
            return Image;
        }

        public struct ColorARGB
        {
            public byte B;
            public byte G;
            public byte R;
            public byte A;

            public ColorARGB(Color color)
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

            public Color ToColor()
            {
                return Color.FromArgb(A, R, G, B);
            }
        }
        //

        public byte[,] ConvertArray(byte[] Input, int size)
        {
            byte[,] Output = new byte[(byte)(Input.Length / size), size];
            for (int i = 0; i < Input.Length; i += size)
            {
                for (int j = 0; j < size; j++)
                {
                    Output[(int)(i / size), j] = Input[i + j];
                }
            }
            return Output;
        }


        private Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage)
        {
            // BitmapImage bitmapImage = new BitmapImage(new Uri("../Images/test.png", UriKind.Relative));

            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(outStream);

                return new Bitmap(bitmap);
            }
        }

        public int[,] Array2D(Bitmap bitmap)
        {
            int[,] imgary = new int[bitmap.Width, bitmap.Height];

            int x, y;

            for (x = 0; x < bitmap.Width; x++)
            {
                for (y = 0; y < bitmap.Height; y++)
                {
                    //imgary[x, y] = bitmap.GetPixel(x, y);
                }
            }
            return imgary;
        }

        public BitmapImage ImageFromBuffer(Byte[] bytes)
        {
            MemoryStream stream = new MemoryStream(bytes);
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.StreamSource = stream;
            image.EndInit();
            return image;
        }

        public Byte[] BufferFromImage(BitmapImage imageSource)
        {
            Stream stream = imageSource.StreamSource;
            Byte[] buffer = null;
            if (stream != null && stream.Length > 0)
            {
                using (BinaryReader br = new BinaryReader(stream))
                {
                    buffer = br.ReadBytes((Int32)stream.Length);
                }
            }

            return buffer;
        }

        private void btnFilterSeq_Click(object sender, RoutedEventArgs e)
        {
            var swSeq = Stopwatch.StartNew();

            //start filtering single thread
            int filterSize = Convert.ToInt32(sizeMatrix.Text);

            //
            //img to array
            var a1 = BitmapImage2Bitmap(InputImage);
            ImageToMatrix conv = new ImageToMatrix(min: 0, max: 1);
            double[,] matrix;
            conv.Convert(a1, out matrix);
            _medianaFilter = new MedianaFilter(matrix, filterSize);
            var a8 = _medianaFilter.SeqStart();
            var a9 = ToBitmap(a8);
            BitmapImage bitmapImage = new BitmapImage();
            using (MemoryStream memory = new MemoryStream())
            {
                a9.Save(memory, ImageFormat.Png);
                memory.Position = 0;

                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
            }
            imgFilter.Source = bitmapImage;
            //

            string str = string.Empty;
            for (int dimension = 1; dimension <= colorMatrix.Rank; dimension++)
                str += dimension + " ";

            TimeSync.Content = str;
            swSeq.Stop();
            TimeSeq.Content = "Czas filtrowania sekwencyjnego " + swSeq.ElapsedMilliseconds.ToString() + "ms";
        }

        private void btnFilterSync_Click(object sender, RoutedEventArgs e)
        {
            var swSync = Stopwatch.StartNew();

            //start filtering multithreading
            var filtredImage = _medianaFilter.AsyncFilter(Convert.ToInt32(countThread.Text));

            swSync.Stop();
            TimeSync.Content = "Czas filtrowania wielowatkowego " + swSync.ElapsedMilliseconds.ToString() + "ms";
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void btnMinusSize_Click(object sender, RoutedEventArgs e)
        {
            int minusFilterSize = Convert.ToInt32(sizeMatrix.Text) - 3;
            if (minusFilterSize < 3)
            {
                sizeMatrix.Text = "3";
            }
            else
            {
                sizeMatrix.Text = minusFilterSize.ToString();
            }
        }

        private void btnPlusSize_Click(object sender, RoutedEventArgs e)
        {
            sizeMatrix.Text = (Convert.ToInt32(sizeMatrix.Text) + 2).ToString();
        }

        private void btnMinusThread_Click(object sender, RoutedEventArgs e)
        {
            int minusCountThread = Convert.ToInt32(countThread.Text) - 1;
            if (minusCountThread < 1)
            {
                countThread.Text = "1";
            }
            else
            {
                countThread.Text = minusCountThread.ToString();
            }
        }

        private void btnPlusThread_Click(object sender, RoutedEventArgs e)
        {
            countThread.Text = (Convert.ToInt32(countThread.Text) + 1).ToString();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
