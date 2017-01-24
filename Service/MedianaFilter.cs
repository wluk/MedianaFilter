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
            var partCount = DimensionX / countThread + 1;
            var parts = new List<ArrayInfo>();

            //Podział obrazu na części
            for (int i = 0; i < countThread; i++)
            {
                if (i == 0)
                {
                    parts.Add(
                    new ArrayInfo()
                    {
                        StartIndex = i * partCount,
                        EndIndex = partCount * (i + 1),
                        Part = Part.First
                    }
                );
                }
                else if (i == countThread - 1)
                {
                    parts.Add(
                    new ArrayInfo()
                    {
                        StartIndex = i * partCount - 1,
                        EndIndex = DimensionX,
                        Part = Part.Last
                    }
                );
                }
                else
                {
                    parts.Add(
                        new ArrayInfo()
                        {
                            StartIndex = i * partCount - 1,
                            EndIndex = partCount * (1 + i),
                            Part = Part.Middle
                        }
                    );
                }
            }
            foreach (var arrayInfo in parts)
            {
                arrayInfo.PartOfImage = DivArray(arrayInfo);
            }

            ProcessedImageAsync = new double[DimensionX,DimensionY];

            foreach (var p in parts)
            {
                switch (p.Part)
                {
                    case Part.First:
                        Merge(p.StartIndex, p.EndIndex - 1, p.PartOfImage);
                        break;
                    case Part.Middle:
                        Merge(p.StartIndex + 1, p.EndIndex - 1, p.PartOfImage);
                        break;
                    case Part.Last:
                        Merge(p.StartIndex + 1, p.EndIndex-1, p.PartOfImage);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }


            return ProcessedImageAsync;
        }

        private void Merge(int startIndex, int endIndex, double[,] imagePart)
        {
            for (int i = 1; i <= endIndex - startIndex; i++)
            {
                for (int j = 1; j < DimensionY - 1; j++)
                {
                    ProcessedImageAsync[i + startIndex, j] = imagePart[i, j];
                }
            }
        }

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

            for (int i = 0; i <= part.EndIndex - part.StartIndex; i++)
            {
                for (int j = 0; j < DimensionY; j++)
                {
                    array[i, j] = Image[i + part.StartIndex, j];
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
