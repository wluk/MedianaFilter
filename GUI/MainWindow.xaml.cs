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

namespace GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private byte[,] colorMatrix;
        private MedianaFilter _medianaFilter;
        private ImageConvert _imageConvert;
        public MainWindow()
        {
            InitializeComponent();
            colorMatrix = new byte[,] { { 1, 4, 0, 1, 3, 1 }, { 2, 2, 4, 2, 2, 3 }, { 1, 0, 1, 0, 1, 0 }, { 1, 2, 1, 0, 2, 2 }, { 2, 5, 3, 1, 2, 5 }, { 1, 1, 4, 2, 3, 0 } };
            _medianaFilter = new MedianaFilter();
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
                var b1 = new BitmapImage(new Uri(op.FileName));
                var b2 = new BitmapImage(new Uri(op.FileName));
                imgBase.Source = b1;
                imgFilter.Source = b2;

                try
                {
                    _imageConvert.sth();
                }
                catch (Exception)
                {

                    throw;
                }
                
            }

        }

        private void btnFilterSeq_Click(object sender, RoutedEventArgs e)
        {
            var swSeq = Stopwatch.StartNew();

            //start filtering single thread
            int filterSize = Convert.ToInt32(sizeMatrix.Text);
            var filtredImage = _medianaFilter.SyncStart();

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
            int thredCount = Convert.ToInt32(countThread.Text);
            var filtredImage = _medianaFilter.AsyncFilter(thredCount);

            swSync.Stop();
            TimeSeq.Content = "Czas filtrowania wielowatkowego " + swSync.ElapsedMilliseconds.ToString() + "ms";
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void btnMinusSize_Click(object sender, RoutedEventArgs e)
        {
            int minusFilterSize = Convert.ToInt32(sizeMatrix.Text)-3;
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
            sizeMatrix.Text = (Convert.ToInt32(sizeMatrix.Text) + 3).ToString();
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
    }
}
