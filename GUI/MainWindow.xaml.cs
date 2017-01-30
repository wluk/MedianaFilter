using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Service;
using MahApps.Metro.Controls;

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
                ImgBase.Source = _inputImage;
                ImgFilter.Source = null;

                TimeSeq.Content = string.Empty;
                TimeSync.Content = string.Empty;
                OneThread.Visibility = Visibility.Hidden;
                MultiThread.Visibility = Visibility.Hidden;
                KeyOneThread.Text = string.Empty;
                KeyMultiThread.Text = string.Empty;
                OneThredCount = 0;
                MultiThreadCount = 0;

                BtnFilter.IsEnabled = true;
                BtnFilterSync.IsEnabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Błąd podczas ładowania obrazu", MessageBoxButton.OK, MessageBoxImage.Error);

                BtnFilter.IsEnabled = false;
                BtnFilterSync.IsEnabled = false;
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
            int filterSize = Convert.ToInt32(SizeMatrix.Text);

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

                ImgFilter.Source = filteredImage;
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
        /// Uruchomienie filtrowania współbieznego
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnFilterSync_Click(object sender, RoutedEventArgs e)
        {
            var swSync = new Stopwatch();
            int filterSize = Convert.ToInt32(SizeMatrix.Text);
            int threadCount = Convert.ToInt32(CountThread.Text);

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

                ImgFilter.Source = filteredImage;
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
            int minusFilterSize = Convert.ToInt32(SizeMatrix.Text) - 3;
            SizeMatrix.Text = minusFilterSize < 3 ? "3" : minusFilterSize.ToString();
        }

        private void btnPlusSize_Click(object sender, RoutedEventArgs e)
        {
            SizeMatrix.Text = (Convert.ToInt32(SizeMatrix.Text) + 2).ToString();
        }

        private void btnMinusThread_Click(object sender, RoutedEventArgs e)
        {
            int minusCountThread = Convert.ToInt32(CountThread.Text) - 1;
            CountThread.Text = minusCountThread < 2 ? "2" : minusCountThread.ToString();
        }

        private void btnPlusThread_Click(object sender, RoutedEventArgs e)
        {
            CountThread.Text = (Convert.ToInt32(CountThread.Text) + 1).ToString();
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

                KeyOneThread.Text = " " + OneThredCount.ToString() + " ms";
                KeyMultiThread.Text = " " + MultiThreadCount.ToString() + " ms";
            }
        }
    }
}