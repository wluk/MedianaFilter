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

namespace GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private byte[,] colorMatrix;
        public MainWindow()
        {
            InitializeComponent();
            colorMatrix = new byte[,] { { 1, 4, 0, 1, 3, 1 }, { 2, 2, 4, 2, 2, 3 }, { 1, 0, 1, 0, 1, 0 }, { 1, 2, 1, 0, 2, 2 }, { 2, 5, 3, 1, 2, 5 }, { 1, 1, 4, 2, 3, 0 } };
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
                var b1 = new BitmapImage(new Uri(op.FileName));
                var b2 = new BitmapImage(new Uri(op.FileName));


                //Bitmap img = new Bitmap(b1.UriSource.LocalPath);
                //int hight = img.Height;
                //int width = img.Width;

                //Color[][] colorMatrix = new Color[width][];
                //for (int i = 0; i < width; i++)
                //{
                //    colorMatrix[i] = new Color[hight];
                //    for (int j = 0; j < hight; j++)
                //    {
                //        colorMatrix[i][j] = img.GetPixel(i, j);
                //    }
                //}

                imgBase.Source = b1;
                imgFilter.Source = b2;

            }

        }

        private void btnFilterSeq_Click(object sender, RoutedEventArgs e)
        {
            var swSeq = Stopwatch.StartNew();
            //start filtering            
            int filterSize = Convert.ToInt32(sizeMatrix.Text);
            if (filterSize != 0)
            {
                Filtering(colorMatrix, filterSize);
            }
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
            //start filtering            

            swSync.Stop();
            TimeSeq.Content = "Czas filtrowania wielowatkowego " + swSync.ElapsedMilliseconds.ToString() + "ms";
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private byte[,] Filtering(byte[,] image, int sizeFilter)
        {
            byte[,] filteredColorMatrix = new byte[,] { };
            List<byte> colorElements = new List<byte>();
            int x = sizeFilter;
            int y = sizeFilter;

            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    colorElements.Add(image[i, j]);
                }
            }
            var mediana = GetMedian(colorElements);
            colorElements[colorElements.Count / 2] = mediana;

            return filteredColorMatrix;
        }

        private byte GetMedian(IEnumerable<byte> source)
        {
            byte[] temp = source.ToArray();
            Array.Sort(temp);

            int count = temp.Length;
            if (count == 0)
            {
                throw new InvalidOperationException("Empty collection");
            }
            else if (count % 2 == 0)
            {
                byte a = temp[count / 2 - 1];
                byte b = temp[count / 2];
                return Convert.ToByte((a + b) / 2);
            }
            else
            {
                return temp[count / 2];
            }
        }
    }
}
