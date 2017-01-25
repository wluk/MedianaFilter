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
        private readonly ImageConvert _imageConvert;
        private BitmapImage _inputImage;
        private int OneThredCount { get; set; }
        private int MultiThreadCount { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            _imageConvert = new ImageConvert();

            OneThredCount = 0;
            MultiThreadCount = 0;
        }

        /// <summary>
        /// Metoda wczytywania obrazu z dysku
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            var op = new OpenFileDialog
            {
                Title = "Wybierz obraz...",
                Filter = "All supported graphics|*.jpg;*.jpeg;*.png;*.bmp|" +
                         "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
                         "Portable Network Graphic (*.png)|*.png"
            };

            if (op.ShowDialog() != true) return;
            try
            {
                _inputImage = new BitmapImage(new Uri(op.FileName));
                imgBase.Source = _inputImage;
                imgFilter.Source = null;

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

        /// <summary>
        /// Uruchomienie filtrowania sekwencyjnego
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnFilterSeq_Click(object sender, RoutedEventArgs e)
        {
            var swSeq = new Stopwatch();
            int filterSize = Convert.ToInt32(sizeMatrix.Text);

            try
            {
                //Utworzenie macierzy pikseli
                var imageMatrix = _imageConvert.GetPixelArray(_inputImage.UriSource.LocalPath);

                _medianaFilter = new MedianaFilter(imageMatrix);

                //Odfiltrowanie obrazu
                swSeq.Start();
                var filteredArrayImage = _medianaFilter.SequenceFiltration(filterSize);
                swSeq.Stop();

                //Utworzenie obrazu z macierzy pikseli
                var filteredImage = _imageConvert.ArrayToBitmapImage(filteredArrayImage);


                imgFilter.Source = filteredImage;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            
            OneThredCount = (int)swSeq.ElapsedMilliseconds;
            TimeSeq.Content = "Czas filtrowania sekwencyjnego " + OneThredCount + "ms";
            ProgresCheck();
        }

        /// <summary>
        /// Uruchomienie filtrowania współbieżnie
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnFilterSync_Click(object sender, RoutedEventArgs e)
        {
            var swSync = new Stopwatch();
            int filterSize = Convert.ToInt32(sizeMatrix.Text);
            int threadCount = Convert.ToInt32(countThread.Text);

            try
            {
                //Utworzenie macierzy pikseli
                var imageMatrix = _imageConvert.GetPixelArray(_inputImage.UriSource.LocalPath);

                _medianaFilter = new MedianaFilter(imageMatrix);

                //Odfiltrowanie obrazu
                swSync.Start();
                var filteredArrayImage = _medianaFilter.AsyncFilter(filterSize, threadCount);
                swSync.Stop();

                //Utworzenie obrazu z macierzy pikseli
                var filteredImage = _imageConvert.ArrayToBitmapImage(filteredArrayImage);


                imgFilter.Source = filteredImage;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            
            MultiThreadCount = (int)swSync.ElapsedMilliseconds;
            TimeSync.Content = "Czas filtrowania wielowatkowego " + MultiThreadCount + "ms";
            ProgresCheck();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnMinusSize_Click(object sender, RoutedEventArgs e)
        {
            int minusFilterSize = Convert.ToInt32(sizeMatrix.Text) - 3;
            sizeMatrix.Text = minusFilterSize < 3 ? "3" : minusFilterSize.ToString();
        }

        private void btnPlusSize_Click(object sender, RoutedEventArgs e)
        {
            sizeMatrix.Text = (Convert.ToInt32(sizeMatrix.Text) + 2).ToString();
        }

        private void btnMinusThread_Click(object sender, RoutedEventArgs e)
        {
            int minusCountThread = Convert.ToInt32(countThread.Text) - 1;
            countThread.Text = minusCountThread < 2 ? "2" : minusCountThread.ToString();
        }

        private void btnPlusThread_Click(object sender, RoutedEventArgs e)
        {
            countThread.Text = (Convert.ToInt32(countThread.Text) + 1).ToString();
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            var regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
        
        private void ProgresCheck()
        {
            if (OneThredCount != 0 && MultiThreadCount != 0)
            {
                OneThread.Visibility = Visibility.Visible;
                MultiThread.Visibility = Visibility.Visible;

                if (OneThredCount > MultiThreadCount)
                {
                    OneThread.Maximum = OneThredCount;
                    MultiThread.Maximum = OneThredCount;
                }
                else
                {
                    OneThread.Maximum = MultiThreadCount;
                    MultiThread.Maximum = MultiThreadCount;
                }

                OneThread.Value = OneThredCount;
                MultiThread.Value = MultiThreadCount;

                KeyOneThread.Text = " " + OneThredCount.ToString()+" ms";
                KeyMultiThread.Text = " " + MultiThreadCount.ToString()+" ms";
            }
        }
    }
}