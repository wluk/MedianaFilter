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

        public byte[,] SyncStart()
        {
            return Frameing();

        }

        private byte[,] Frameing()
        {
            byte[,] filteredColorMatrix = new byte[,] { };
            List<byte> colorElements = new List<byte>();

            for (int i = 0; i < FrameFilterSize; i++)
            {
                for (int j = 0; j < FrameFilterSize; j++)
                {
                    colorElements.Add(Image[i, j]);
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
