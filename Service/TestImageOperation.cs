using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Service
{
    public class TestImageOperation
    {
        public byte[,] GetImagePixelArray(string filePath)
        {
            Bitmap bitmap = new Bitmap(filePath);
            // Lock the bitmap's bits.  
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            System.Drawing.Imaging.BitmapData bmpData =
                bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite,
                bmp.PixelFormat);

            // Get the address of the first line.
            IntPtr ptr = bmpData.Scan0;

            // Declare an array to hold the bytes of the bitmap.
            int bytes = Math.Abs(bmpData.Stride) * bmp.Height;
            byte[] rgbValues = new byte[bytes];

            // Copy the RGB values into the array.
            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);

            // Set every third value to 255. A 24bpp bitmap will look red.  
            for (int counter = 2; counter < rgbValues.Length; counter += 3)
                rgbValues[counter] = 255;

            // Copy the RGB values back to the bitmap
            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, bytes);

            // Unlock the bits.
            bmp.UnlockBits(bmpData);

            return rgbValues;


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
