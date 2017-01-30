using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Service
{
    public class MedianaFilter
    {
        /// <summary>
        /// Rozmiar ramki filtrującej
        /// </summary>
        public int FilterSize { get; set; }
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
        /// Obraz po filtracji
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
        /// <param name="filter">Rozmiar kratki filtrującej</param>
        /// <param name="countThread">Liczba wątków</param>
        /// <returns>Macierz (obraz) po zastosowaniu filtru medianowego na wątkach</returns>
        public double[,] AsyncFilter(int filter, int countThread)
        {
            FilterSize = filter;
            var partCount = DimensionX / countThread + (FilterSize/2);
            var parts = new List<ArrayInfo>
            {
                new ArrayInfo()
                {
                    StartIndex = 0,
                    EndIndex = partCount,
                    PartOfImage = CreatePartTab(partCount + FilterSize/2)
                }
            };

            //Podział obrazu na kawałki
            for (int i = 1; i < countThread; i++)
            {
                var sieze = parts.LastOrDefault().EndIndex - FilterSize / 2;
                if (i != countThread - 1)
                    parts.Add(new ArrayInfo()
                    {
                        StartIndex = sieze,
                        EndIndex = sieze + partCount,
                        PartOfImage = CreatePartTab(parts.LastOrDefault().EndIndex + partCount - sieze)
                    });
                else
                    parts.Add(new ArrayInfo()
                    {
                        StartIndex = sieze,
                        EndIndex = DimensionX,
                        PartOfImage = CreatePartTab(DimensionX - sieze)
                    });
            }

            ProcessedImageAsync = Image;

            //Wykonanie filtracji
            List<Thread> threads = new List<Thread>();
            foreach (var sector in parts)
            {
                Thread t = new Thread(() =>
                {
                    sector.PartOfImage = Filtrowanie(DivArray(sector));
                });

                t.IsBackground = true;
                threads.Add(t);
                t.Start();
            }

            foreach (var thread in threads)
            {
                thread.Join();
            }

            //Złączenie części obrazu
            foreach (var sector in parts)
            {
                Merge(sector.StartIndex + 1, sector.PartOfImage);
            }

            return ProcessedImageAsync;
        }

        private double[,] CreatePartTab(int x)
        {
            return new double[x, DimensionY];
        }

        private double[,] DivArray(ArrayInfo part)
        {
            var result = part.PartOfImage;

            for (int i = 0; i < part.PartOfImage.GetLength(0); i++)
            {
                for (int j = 0; j < DimensionY; j++)
                {
                    result[i, j] = Image[i + part.StartIndex, j];
                }
            }

            return result;
        }

        private double[,] Filtrowanie(double[,] imageArray)
        {
            var result = new double[imageArray.GetLength(0), imageArray.GetLength(1)];
            for (int i = 0; i < result.GetLength(0); i++)
            {
                for (int j = 0; j < result.GetLength(1); j++)
                {
                    result[i, j] = -1;
                }
            }

            //Utworzenie macierzy z median dla zadanej macierzy obrazu
            for (int i = 0; i < imageArray.GetLength(0); i++)
            {
                for (int j = 0; j < imageArray.GetLength(1); j++)
                {
                    //Ograniczenie końca macierzy obrazu
                    if (j <= imageArray.GetLength(1) - FilterSize && i <= imageArray.GetLength(0) - FilterSize)
                    {
                        var colorElements = new List<double>();

                        for (int k = i; k <= i + (FilterSize - FilterSize / 2); k++)
                        {
                            for (int l = j; l <= j + (FilterSize - FilterSize / 2); l++)
                            {
                                colorElements.Add(imageArray[k, l]);
                            }
                        }

                        var mediana = GetMedian(colorElements);
                        int a = (i + i + (FilterSize - FilterSize / 2)) / 2;
                        int b = (j + j + (FilterSize - FilterSize / 2)) / 2;
                        result[a, b] = mediana;
                    }
                }
            }
            return result;
        }

        private void Merge(int startIndex, double[,] imagePart)
        {
            for (int i = 1; i < imagePart.GetLength(0) - 1; i++)
            {
                for (int j = 1; j < DimensionY - 1; j++)
                {
                    if (imagePart[i, j] != -1)
                    {
                        ProcessedImageAsync[i + startIndex - 1, j] = imagePart[i, j];
                    }
                }
            }
        }


        /// <summary>
        /// Filtrowanie obrazu na jednym wątku
        /// </summary>
        /// <param name="filter">Rozmiar okna próbkującego</param>
        /// <returns>Przetworzony obaz w głównym wątku</returns>
        public double[,] SequenceFiltration(int filter)
        {
            FilterSize = filter;

            //Filtracja medianowa
            for (int i = 0; i < DimensionX; i++)
            {
                for (int j = 0; j < DimensionY; j++)
                {
                    if (j <= DimensionY - FilterSize && i <= DimensionX - FilterSize)
                    {
                        GetPixels(i, j, i + (FilterSize - FilterSize / 2), j + (FilterSize - FilterSize / 2));
                    }
                }
            }

            double[,] processedImage = Image;

            //Naniesienie macierzy medianowej na obraz
            for (int i = 1; i < DimensionX - 1; i++)
            {
                for (int j = 1; j < DimensionY - 1; j++)
                {
                    if (MedianaMatrixSqe[i, j] != -1)
                    {
                        processedImage[i, j] = MedianaMatrixSqe[i, j];
                    }
                }
            }

            return processedImage;
        }

        /// <summary>
        /// Filtrowanie klatkowe
        /// </summary>
        /// <param name="startX">Pozycja startowa X</param>
        /// <param name="startY">Pozycja startowa Y</param>
        /// <param name="stopX">Pozycja końcowa X</param>
        /// <param name="stopY">Pozycja końcowa Y</param>
        private void GetPixels(int startX, int startY, int stopX, int stopY)
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
    }

    internal class ArrayInfo
    {
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
        public double[,] PartOfImage { get; set; }
    }
}