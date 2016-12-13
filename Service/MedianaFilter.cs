using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Service
{
    public class MedianaFilter
    {
        /// <summary>
        /// Rozmiar ramki filtrującej
        /// </summary>
        public int FrameFilterSize { get; set; }
        /// <summary>
        /// Obraz do filtracji
        /// </summary>
        public double[,] Image { get; set; }
        /// <summary>
        /// Macierz z medianami
        /// </summary>
        public double[,] MedianaMatrixSqe { get; set; }
        /// <summary>
        /// Wymiar macierzy obrazu do filtrowania
        /// </summary>
        public int dimensionX
        {
            get { return Image.GetLength(0); }
            set { }
        }
        /// <summary>
        /// Wymiar macierzy obrazu do filtrowania
        /// </summary>
        public int dimensionY
        {
            get { return Image.GetLength(1); }
            set { }
        }

        public MedianaFilter(double[,] imageMatrix)
        {
            Image = imageMatrix;
            MedianaMatrixSqe = new double[dimensionX, dimensionY];
        }

        /// <summary>
        /// Filtrowanie na wątkach
        /// </summary>
        /// <param name="countThread">Liczba wątków</param>
        /// <param name="filterSize">Rozmiar kratki filtrującej</param>
        /// <returns>Macierz (obraz) po zastosowaniu filtru medianowego na wątkach</returns>
        public double[,] AsyncFilter(int countThread, int filterSize)
        {
            var partCount = dimensionX / countThread + (dimensionX % countThread > 0 ? 1 : 0);
            var parts = new List<ArrayInfo>();
            List<double[,]> partsmedianaCollection = new List<double[,]>();

            //Określenie części obrazu
            for (int i = 0; i < countThread; i++)
            {
                if (i != 0 && i != countThread - 1)
                {
                    var current = new ArrayInfo
                    {
                        StartIndex = partCount * i,
                        EndIndex = (partCount * i) + partCount + 1
                    };
                    parts.Add(current);
                }
                else if (i == countThread - 1)
                {
                    var current = new ArrayInfo
                    {
                        StartIndex = partCount * i,
                        EndIndex = dimensionX
                    };
                    parts.Add(current);
                }
                else
                {
                    var current = new ArrayInfo
                    {
                        StartIndex = 0,
                        EndIndex = partCount + 1
                    };
                    parts.Add(current);
                }
            }
            //Podział obrazu na części
            List<Thread> threads = new List<Thread>();
            List<ArrayInfo> resultMediana = new List<ArrayInfo>();
            //foreach (var p in parts)
            //{
            //    double[,] imagePart = DivArray(p.StartIndex, p.EndIndex);

            //    Thread t = new Thread(() =>
            //    {
            //        resultMediana.Add(new ArrayInfo()
            //        {
            //            StartIndex = p.StartIndex,
            //            EndIndex = p.EndIndex,
            //            PartOfImage = Filtrowanie(filterSize, imagePart)
            //        });
            //    });
            //    t.IsBackground = true;

            //    threads.Add(t);
            //    t.Start();
            //}
            Parallel.ForEach(
                parts,
                new ParallelOptions { MaxDegreeOfParallelism = countThread },
                p =>
            {
                double[,] imagePart = DivArray(p.StartIndex, p.EndIndex);
                p.PartOfImage = Filtrowanie(filterSize, imagePart);
            }
            );
            Task.WaitAny();
            //threads.LastOrDefault().Join();

            double[,] processedImageSync = new double[dimensionX, dimensionX];
            foreach (var p in parts)
            {
                for (int i = p.StartIndex; i < p.EndIndex - 1; i++)
                {
                    for (int j = 0; j < dimensionY - 1; j++)
                    {
                        processedImageSync[i, j] = p.PartOfImage[i - p.StartIndex, j];
                    }
                }
            }

            return processedImageSync;
        }

        public double[,] FilterShow(int countThread, int filterSize)
        {
            var partCount = dimensionX / countThread + (dimensionX % countThread > 0 ? 1 : 0);
            var parts = new Dictionary<ArrayInfo, double[,]>();

            //Określenie części obrazu
            for (int i = 0; i < countThread; i++)
            {
                if (i != 0 && i != countThread - 1)
                {
                    var current = new ArrayInfo
                    {
                        StartIndex = partCount * i,
                        EndIndex = (partCount * i) + partCount + 1
                    };
                    parts.Add(current, null);
                }
                else if (i == countThread - 1)
                {
                    var current = new ArrayInfo
                    {
                        StartIndex = partCount * i,
                        EndIndex = dimensionX
                    };
                    parts.Add(current, null);
                }
                else
                {
                    var current = new ArrayInfo
                    {
                        StartIndex = 0,
                        EndIndex = partCount + 1
                    };
                    parts.Add(current, null);
                }
            }

            var abc = new Dictionary<ArrayInfo, double[,]>();
            foreach (var p in parts)
            {
                double[,] imagePart = DivArray(p.Key.StartIndex, p.Key.EndIndex);
                double[,] imaageFilter = (Filtrowanie(filterSize, imagePart));
                abc.Add(p.Key, imaageFilter);
            }

            double[,] processedImageSync = new double[dimensionX, dimensionX];
            foreach (var p in abc)
            {
                for (int i = p.Key.StartIndex; i < p.Key.EndIndex - 1; i++)
                {
                    for (int j = 0; j < dimensionY - 1; j++)
                    {
                        processedImageSync[i, j] = p.Value[i - p.Key.StartIndex, j];
                    }
                }
            }

            return processedImageSync;
        }

        private double[,] Filtrowanie(int filterSize, double[,] imageArray)
        {
            FrameFilterSize = filterSize;
            var result = new double[imageArray.GetLength(0), imageArray.GetLength(1)];
            //Utworzenie macierzy z median dla zadanej macierzy obrazu
            for (int i = 0; i < imageArray.GetLength(0); i++)
            {
                for (int j = 0; j < imageArray.GetLength(1); j++)
                {
                    //Ograniczenie końca macierzy obrazu
                    if (j <= imageArray.GetLength(1) - FrameFilterSize && i <= imageArray.GetLength(0) - FrameFilterSize)
                    {
                        //FrameingSeq(i, j, i + (FrameFilterSize - FrameFilterSize / 2), j + (FrameFilterSize - FrameFilterSize / 2));
                        List<double> colorElements = new List<double>();

                        for (int k = i; k <= i + (FrameFilterSize - FrameFilterSize / 2); k++)
                        {
                            for (int l = j; l <= j + (FrameFilterSize - FrameFilterSize / 2); l++)
                            {
                                colorElements.Add(imageArray[k, l]);
                            }
                        }

                        var mediana = GetMedian(colorElements);
                        int a = (i + i + (FrameFilterSize - FrameFilterSize / 2)) / 2;
                        int b = (j + j + (FrameFilterSize - FrameFilterSize / 2)) / 2;
                        result[a, b] = mediana;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Filtrowanie obrazu na jednym wątku
        /// </summary>
        /// <param name="filterSize">Rozmiar okna próbkującego</param>
        /// <returns>Przetworzony obaz w głównym wątku</returns>
        public double[,] SequenceFiltration(int filterSize)
        {
            FrameFilterSize = filterSize;
            Thread t1 = new Thread(Sequence);
            t1.Start();
            t1.Join();

            //Naniesienie macierzy medianowej na obraz
            double[,] processedImage = Image;
            for (int i = 1; i < dimensionX - 1; i++)
            {
                for (int j = 1; j < dimensionY - 1; j++)
                {
                    processedImage[i, j] = MedianaMatrixSqe[i, j];
                }
            }

            return MedianaMatrixSqe;
        }

        private void Sequence()
        {
            for (int i = 0; i < dimensionX; i++)
            {
                for (int j = 0; j < dimensionY; j++)
                {
                    if (j <= dimensionY - FrameFilterSize && i <= dimensionX - FrameFilterSize)
                    {
                        FrameingSeq(i, j, i + (FrameFilterSize - FrameFilterSize / 2), j + (FrameFilterSize - FrameFilterSize / 2));
                    }
                }
            }
        }

        /// <summary>
        /// Filtrowanie klatkowe
        /// </summary>
        /// <param name="startX">Pozycja startowa X</param>
        /// <param name="startY">Pozycja startowa Y</param>
        /// <param name="stopX">Pozycja końcowa X</param>
        /// <param name="stopY">Pozycja końcowa Y</param>
        private void FrameingSeq(int startX, int startY, int stopX, int stopY)
        {
            List<double> colorElements = new List<double>();

            for (int i = startX; i <= stopX; i++)
            {
                for (int j = startY; j <= stopY; j++)
                {
                    colorElements.Add(Image[i, j]);
                }
            }

            var mediana = GetMedian(colorElements);
            int a = (startX + stopX) / 2;
            int b = (startY + stopY) / 2;
            MedianaMatrixSqe[a, b] = mediana;
        }
        private double[,] FrameingSync(int startX, int stopX)
        {
            List<double> colorElements = new List<double>();
            var result = new double[stopX - startX, dimensionY];
            for (int i = startX; i <= stopX; i++)
            {
                for (int j = 0; j <= dimensionY; j++)
                {
                    colorElements.Add(Image[i, j]);
                }
            }

            var mediana = GetMedian(colorElements);
            int a = (startX + stopX) / 2;
            int b = (0 + dimensionY) / 2;
            result[a, b] = mediana;

            return result;
        }

        /// <summary>
        /// Mediana ze zbioru
        /// </summary>
        /// <param name="source">Zbiór liczb</param>
        /// <returns>Liczba będąca medianą z podanego zbioru</returns>
        public double GetMedian(IEnumerable<double> source)
        {
            double[] temp = source.ToArray();
            Array.Sort(temp);

            int count = temp.Length;
            if (count == 0)
            {
                throw new InvalidOperationException("Empty collection");
            }
            else if (count % 2 == 0)
            {
                double a = temp[count / 2 - 1];
                double b = temp[count / 2];
                return Convert.ToDouble((a + b) / 2);
            }
            else
            {
                return temp[count / 2];
            }
        }

        static private List<string[,]> _processedArray = new List<string[,]>();

        private static string[,] InThread(int a, int b)
        {
            var newArray = new string[b - a, 520];
            for (int i = 0; i < b - a; i++)
            {
                for (int j = 0; j < newArray.GetLength(1); j++)
                {
                    newArray[i, j] = Thread.CurrentThread.Name;
                }
            }
            return newArray;
        }

        private double[,] DivArray(int start, int stop)
        {
            var array = new double[stop - start, dimensionY];

            for (int i = start; i < stop; i++)
            {
                for (int j = 0; j < dimensionY; j++)
                {
                    array[i - start, j] = Image[i, j];
                }
            }

            return array;
        }
    }

    class ArrayInfo
    {
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
        public double[,] PartOfImage { get; set; }
    }
}
