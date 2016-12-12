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


namespace GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private MedianaFilter _medianaFilter;
        private ImageConvert _imageConvert;
        BitmapImage inputImage;

        public MainWindow()
        {
            InitializeComponent();
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
                try
                {
                    inputImage = new BitmapImage(new Uri(op.FileName));
                    imgBase.Source = inputImage;

                    btnFilter.IsEnabled = true;
                    btnFilterSync.IsEnabled = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Błąd podczas ładowania obrazu", MessageBoxButton.OK, MessageBoxImage.Error);
                    btnFilter.IsEnabled = false;
                    btnFilterSync.IsEnabled = false;
                }
            }
        }

        private void btnFilterSeq_Click(object sender, RoutedEventArgs e)
        {
            var swSeq = Stopwatch.StartNew();
            int filterSize = Convert.ToInt32(sizeMatrix.Text);

            try
            {
                //Image to array
                var ImageMatrix = _imageConvert.GetPixelArray(inputImage.UriSource.LocalPath);

                _medianaFilter = new MedianaFilter(ImageMatrix);

                //Odfiltrowanie obrazu
                var filteredArrayImage = _medianaFilter.SeqStart(filterSize);

                //Array to image
                var filteredImage = _imageConvert.ArrayToBitmapImage(filteredArrayImage);

                
                imgFilter.Source = filteredImage;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Błąd podczas ładowania obrazu", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            swSeq.Stop();
            TimeSeq.Content = "Czas filtrowania sekwencyjnego " + swSeq.ElapsedMilliseconds.ToString() + "ms";
        }

        private void btnFilterSync_Click(object sender, RoutedEventArgs e)
        {
            var swSync = Stopwatch.StartNew();
            int filterSize = Convert.ToInt32(sizeMatrix.Text);

            try
            {
                ////BitmapImage to bitmap
                //var bitmaapImage = _imageConvert.BitmapImageToBitmap(inputImage);
                ////Bitmap to array
                //var ImageMatrix = _imageConvert.BitmapToArray(bitmaapImage);
                ////Inicjalizacja metody filtrowania
                //_medianaFilter = new MedianaFilter(ImageMatrix);
                ////Odfiltrowanie obrazu
                //var filteredArrayImage = _medianaFilter.SeqStart(filterSize);
                ////Array to bitmap
                //var bitmapFilteredImage = _imageConvert.ArrayToBitmap(filteredArrayImage);
                ////Bitmap to BitmapImage
                //var filteredImage = _imageConvert.BitmapToBitmapImage(bitmapFilteredImage);

                ////filteredImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                //imgFilter.Source = filteredImage;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Błąd podczas ładowania obrazu", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            swSync.Stop();
            TimeSync.Content = "Czas filtrowania wielowatkowego " + swSync.ElapsedMilliseconds.ToString() + "ms";
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
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

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
