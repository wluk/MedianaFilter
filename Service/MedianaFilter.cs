using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public class MedianaFilter
    {
        public int FrameFilterSize { get; set; }
        public byte[,] Image { get; set; }
        public int dimensionX
        {
            get { return Image.GetLength(0); }
            set { }
        }
        public int dimensionY
        {
            get { return Image.GetLength(1); }
            set { }
        }

        public MedianaFilter(byte[,] imageMatrix, int filterSize)
        {
            Image = imageMatrix;
            FrameFilterSize = filterSize;
        }

        public byte[,] AsyncFilter(int countThread)
        {
            //Thread t = new Thread(new ParameterizedThreadStart(myMethod));
            //t.Start (myParameterObject);

            //public Thread StartTheThread(SomeType param1, SomeOtherType param2)
            //{
            //    var t = new Thread(() => RealStart(param1, param2));
            //    t.Start();
            //    return t;
            //}

            //private static void RealStart(SomeType param1, SomeOtherType param2)
            //{
            //  ...
            //}
            for (int i = 0; i < countThread; i++)
            {




            }
            return Frameing();
        }

        public byte[,] SeqStart()
        {
            return Frameing();
            //
        }

        private byte[,] Frameing()
        {
            byte[,] filteredColorMatrix = Image;
            List<byte> colorElements = new List<byte>();

            for (int i = 0; i < dimensionX; i++)
            {
                for (int j = 0; j < dimensionY; j++)
                {
                    colorElements.Add(Image[i, j]);
                    if ((i + 1) % FrameFilterSize == 0 && (j + 1) % FrameFilterSize == 0 && i != 0 && j != 00 && i == j)
                    {
                        var mediana = GetMedian(colorElements);
                        colorElements.Clear();
                        int a = i - 1;
                        int b = j - 1;
                        filteredColorMatrix[a, b] = mediana;
                    }
                }
            }

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
