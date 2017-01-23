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
        public int filterSize { get; set; }
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
        public int DimensionX => Image.GetLength(0);
        /// <summary>
        /// Wymiar macierzy obrazu do filtrowania
        /// </summary>
        public int DimensionY => Image.GetLength(1);
        /// <summary>
        /// 
        /// </summary>
        public double[,] ProcessedImageAsync { get; set; }


        public MedianaFilter(double[,] imageMatrix)
        {
            Image = imageMatrix;
            MedianaMatrixSqe = new double[DimensionX, DimensionY];
            for (int i = 0; i < DimensionX; i++)
            {
                for (int j = 0; j < DimensionY; j++)
                {
                    MedianaMatrixSqe[i, j] = -1;
                }
            }
        }

        /// <summary>
        /// Filtrowanie na wątkach
        /// </summary>
        /// <param name="filterSize">Rozmiar kratki filtrującej</param>
        /// <param name="countThread">Liczba wątków</param>
        /// <returns>Macierz (obraz) po zastosowaniu filtru medianowego na wątkach</returns>
        public double[,] AsyncFilter(int filterSize, int countThread)
        {
            var partCount = DimensionX / countThread + (DimensionX % countThread > 0 ? 1 : 0);
            var parts = new List<ArrayInfo>();

            //Podział obrazu na części
            for (int i = 0; i < countThread; i++)
            {
                if (i != 0 && i != countThread - 1)
                {
                    //Część środkowa
                    var current = new ArrayInfo
                    {
                        StartIndex = (partCount * i) - 1,
                        EndIndex = (partCount * i - 1) - 1 + partCount,
                        Part = Part.Middle
                    };
                    parts.Add(current);
                }
                else if (i == countThread - 1)
                {
                    //Część dolna
                    var current = new ArrayInfo
                    {
                        StartIndex = partCount - 1,
                        EndIndex = DimensionX - 1,
                        Part = Part.Last
                    };
                    parts.Add(current);
                }
                else
                {
                    //Część górna
                    var current = new ArrayInfo
                    {
                        StartIndex = 0,
                        EndIndex = partCount,
                        Part = Part.First
                    };
                    parts.Add(current);
                }
            }
            //Podział obrazu na części
            //List<Thread> threads = new List<Thread>();
            //List<ArrayInfo> resultMediana = new List<ArrayInfo>();
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
                double[,] imagePart = DivArray(p);
                p.PartOfImage = imagePart;
                //Filtrowanie(filterSize, imagePart);
            }
            );
            Task.WaitAny();
            ////threads.LastOrDefault().Join();
            //ProcessedImageAsync = Image;
            ProcessedImageAsync = new double[DimensionX, DimensionY];
            for (int i = 0; i < DimensionX; i++)
            {
                for (int j = 0; j < DimensionY; j++)
                {
                    ProcessedImageAsync[i, j] = 0;
                }
            }

            foreach (var p in parts)
            {

                if (p.Part != Part.First)
                    Merge(p.StartIndex-1, p.EndIndex, p, 0);
                else
                    Merge(p.StartIndex, p.EndIndex-1, p, 0);
            }

            return ProcessedImageAsync;
        }

        private void Merge(int startIndex, int endIndex, ArrayInfo imagePart, int alignment)
        {
            for (int i = startIndex; i <= endIndex; i++)
            {
                for (int j = 0; j < DimensionY - 1; j++)
                {
                    ProcessedImageAsync[i, j] = imagePart.PartOfImage[i, j];
                }
            }

        }

        //public double[,] FilterShow(int countThread, int filterSize)
        //{
        //    var partCount = DimensionX / countThread + (DimensionX % countThread > 0 ? 1 : 0);
        //    var parts = new Dictionary<ArrayInfo, double[,]>();

        //    //Określenie części obrazu
        //    for (int i = 0; i < countThread; i++)
        //    {
        //        if (i != 0 && i != countThread - 1)
        //        {
        //            var current = new ArrayInfo
        //            {
        //                StartIndex = partCount * i,
        //                EndIndex = (partCount * i) + partCount + 1
        //            };
        //            parts.Add(current, null);
        //        }
        //        else if (i == countThread - 1)
        //        {
        //            var current = new ArrayInfo
        //            {
        //                StartIndex = partCount * i,
        //                EndIndex = DimensionX
        //            };
        //            parts.Add(current, null);
        //        }
        //        else
        //        {
        //            var current = new ArrayInfo
        //            {
        //                StartIndex = 0,
        //                EndIndex = partCount + 1
        //            };
        //            parts.Add(current, null);
        //        }
        //    }

        //    var abc = new Dictionary<ArrayInfo, double[,]>();
        //    foreach (var p in parts)
        //    {
        //        double[,] imagePart = DivArray(p.Key.StartIndex, p.Key.EndIndex);
        //        double[,] imaageFilter = (Filtrowanie(filterSize, imagePart));
        //        abc.Add(p.Key, imaageFilter);
        //    }

        //    double[,] processedImageSync = new double[DimensionX, DimensionX];
        //    foreach (var p in abc)
        //    {
        //        for (int i = p.Key.StartIndex; i < p.Key.EndIndex - 1; i++)
        //        {
        //            for (int j = 0; j < DimensionY - 1; j++)
        //            {
        //                processedImageSync[i, j] = p.Value[i - p.Key.StartIndex, j];
        //            }
        //        }
        //    }

        //    return processedImageSync;
        //}

        private double[,] Filtrowanie(int filterSize, double[,] imageArray)
        {
            this.filterSize = filterSize;
            var result = new double[imageArray.GetLength(0), imageArray.GetLength(1)];
            //Utworzenie macierzy z median dla zadanej macierzy obrazu
            for (int i = 0; i < imageArray.GetLength(0); i++)
            {
                for (int j = 0; j < imageArray.GetLength(1); j++)
                {
                    //Ograniczenie końca macierzy obrazu
                    if (j <= imageArray.GetLength(1) - this.filterSize && i <= imageArray.GetLength(0) - this.filterSize)
                    {
                        //FrameingSeq(i, j, i + (FrameFilterSize - FrameFilterSize / 2), j + (FrameFilterSize - FrameFilterSize / 2));
                        var colorElements = new List<double>();

                        for (int k = i; k <= i + (this.filterSize - this.filterSize / 2); k++)
                        {
                            for (int l = j; l <= j + (this.filterSize - this.filterSize / 2); l++)
                            {
                                colorElements.Add(imageArray[k, l]);
                            }
                        }

                        var mediana = GetMedian(colorElements);
                        int a = (i + i + (this.filterSize - this.filterSize / 2)) / 2;
                        int b = (j + j + (this.filterSize - this.filterSize / 2)) / 2;
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
            Sequence();
            double[,] processedImage = Image;

            //Naniesienie macierzy medianowej na obraz
            for (int i = 1; i < DimensionX - 1; i++)
            {
                for (int j = 1; j < DimensionY - 1; j++)
                {
                    processedImage[i, j] = MedianaMatrixSqe[i, j];
                }
            }

            return processedImage;
        }

        private void Sequence()
        {
            for (int i = 0; i < DimensionX; i++)
            {
                for (int j = 0; j < DimensionY; j++)
                {
                    if (j <= DimensionY - filterSize && i <= DimensionX - filterSize)
                    {
                        FrameingSeq(i, j, i + (filterSize - filterSize / 2), j + (filterSize - filterSize / 2));
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
            var colorElements = new List<double>();

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
        /*
                private double[,] FrameingSync(int startX, int stopX)
                {
                    List<double> colorElements = new List<double>();
                    var result = new double[stopX - startX, DimensionY];
                    for (int i = startX; i <= stopX; i++)
                    {
                        for (int j = 0; j <= DimensionY; j++)
                        {
                            colorElements.Add(Image[i, j]);
                        }
                    }

                    var mediana = GetMedian(colorElements);
                    int a = (startX + stopX) / 2;
                    int b = (0 + DimensionY) / 2;
                    result[a, b] = mediana;

                    return result;
                }
        */

        /// <summary>
        /// Mediana ze zbioru
        /// </summary>
        /// <param name="source">Zbiór liczb</param>
        /// <returns>Liczba będąca medianą z podanego zbioru</returns>
        private static double GetMedian(IEnumerable<double> source)
        {
            double[] temp = source.ToArray();
            Array.Sort(temp);

            int count = temp.Length;

            if (count % 2 == 0)
            {
                double a = temp[count / 2 - 1];
                double b = temp[count / 2];
                return Convert.ToDouble((a + b) / 2);
            }

            if (count == 0)
            {
                throw new InvalidOperationException("Pusty zbiór");
            }

            return temp[count / 2];
        }

        /*
                private static List<string[,]> _processedArray = new List<string[,]>();
        */

        /*
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
        */

        private double[,] DivArray(ArrayInfo part)
        {
            var array = new double[part.EndIndex + 1 - part.StartIndex, DimensionY];

            for (int i = part.StartIndex; i <= part.EndIndex; i++)
            {
                for (int j = 0; j < DimensionY; j++)
                {
                    array[i - part.StartIndex, j] = Image[i, j];
                }
            }

            return array;
        }
    }

    internal class ArrayInfo
    {
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
        public double[,] PartOfImage { get; set; }
        public Part Part { get; set; }
    }

    internal enum Part
    {
        First,
        Middle,
        Last
    }
}
