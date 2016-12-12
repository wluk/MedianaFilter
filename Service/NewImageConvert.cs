using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Service
{
    public class NewImageConvert
    {
        public byte[,] GetImagePixelArray(string filePath)
        {
            Bitmap bitmap = new Bitmap(filePath);
            try
            {
                var btmapData = bitmap.LockBits(
                    new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                    System.Drawing.Imaging.ImageLockMode.ReadOnly,
                    bitmap.PixelFormat
                    );
                var lenght = btmapData.Stride * btmapData.Height;

                byte[] bytes = new byte[lenght];
                Marshal.Copy(btmapData.Scan0, bytes, 0, lenght);
                bitmap.UnlockBits(btmapData);

                var x = ConvertArray(bytes, bitmap.Height, bitmap.Width);
                return x;
            }
            catch (Exception e)
            {

            }
            return null;
        }
        public BitmapImage ToImage(byte[,] input)
        {
            var byteVal = ConvertArray(input);
            BitmapImage myBitmapImage = new BitmapImage();

            try
            {
                MemoryStream strmImg = new MemoryStream(byteVal);
                myBitmapImage.BeginInit();
                myBitmapImage.StreamSource = strmImg;
                myBitmapImage.DecodePixelWidth = 200;
                myBitmapImage.EndInit();

            }
            catch (Exception e)
            {
            }
            return myBitmapImage;
        }

        private byte[,] ConvertArray(byte[] oneDimensionArray, int rowCount, int columnCount)
        {
            byte[,] twoDimensionArray = new byte[rowCount, columnCount];
            for (int i = 0; i < rowCount; i++)
            {
                for (int j = 0; j < columnCount; j++)
                {
                    twoDimensionArray[i, j] = oneDimensionArray[i + j];
                }
            }
            return twoDimensionArray;
        }

        private byte[] ConvertArray(byte[,] twoDimensionArray)
        {
            return twoDimensionArray.Cast<byte>().ToArray();
        }
    }
}
