using System;
using System.Drawing;
using System.Windows.Media.Imaging;
/*Biblioteka z internetu Accord.Imaging.Converters
http://accord-framework.net/docs/html/R_Project_Accord_NET.htm
*/
using Accord.Imaging.Converters;

namespace Service
{
    /// <summary>
    /// Przetwarzanie obrazu
    /// </summary>
    public class ImageConvert
    {
        /// <summary>
        /// Przetworzenie macierzy do Bitmap
        /// </summary>
        /// <param name="rawImage">Macierz obrazu</param>
        /// <returns>Bitmap z obrazem</returns>
        public BitmapSource ArrayToBitmapImage(double[,] rawImage)
        {
            Bitmap imageBitmap;
            MatrixToImage conventer = new MatrixToImage(min: 0, max: 1);
            conventer.Convert(rawImage, out imageBitmap);

            var imageBitmapImage = BitmapToBitmapImage(imageBitmap);

            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(imageBitmapImage));

            //Testowy zapis pliku wynikowego
            //using (var fileStream = new FileStream(@"C:\Users\wilu\Desktop\Filtr medianowy\file.png", FileMode.Create))
            //{
            //    encoder.Save(fileStream);
            //}

            return imageBitmapImage;
        }

        /// <summary>
        /// Metoda konwertująca obraz do Bitmap, a następnie na macierz
        /// </summary>
        /// <param name="localPath">Ścieżka do pliku</param>
        /// <returns>Macierz pixeli</returns>
        public double[,] GetPixelArray(string localPath)
        {
            double[,] result;
            Bitmap image = new Bitmap(localPath);

            ImageToMatrix conventer = new ImageToMatrix(min: 0, max: 1);
            conventer.Convert(image, out result);

            return result;
        }

        /// <summary>
        /// Konwersja Bitmap do BitmapSource
        /// </summary>
        /// <param name="image">Bitmap</param>
        /// <returns>BitmapSource</returns>
        private BitmapSource BitmapToBitmapImage(Bitmap image)
        {
            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                           image.GetHbitmap(),
                           IntPtr.Zero,
                           System.Windows.Int32Rect.Empty,
                           BitmapSizeOptions.FromWidthAndHeight(image.Width, image.Height));
        }
    }
}
