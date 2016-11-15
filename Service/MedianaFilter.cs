using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public class MedianaFilter
    {
        public byte[,] Start(byte[,] image, int sizeFilter)
        {
            byte[,] filteredColorMatrix = new byte[,] { };
            List<byte> colorElements = new List<byte>();

            for (int i = 0; i < sizeFilter; i++)
            {
                for (int j = 0; j < sizeFilter; j++)
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
