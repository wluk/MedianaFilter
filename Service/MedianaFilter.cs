using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public double[,] MedianaMatrix { get; set; }
        /// <summary>
        /// Obraz wynikowy
        /// </summary>
        public double[,] ProcessedImage { get; set; }
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
            MedianaMatrix = new double[dimensionX, dimensionY];
            ProcessedImage = imageMatrix;
        }

        /// <summary>
        /// Filtrowanie na wątkach
        /// </summary>
        /// <param name="countThread">Liczba wątków</param>
        /// <param name="filterSize">Rozmiar kratki filtrującej</param>
        /// <returns>Macierz (obraz) po zastosowaniu filtru medianowego na wątkach</returns>
        public double[,] AsyncFilter(int countThread, int filterSize)
        {
            FrameFilterSize = filterSize;
            //Utworzenie macierzy z median dla zadanej macierzy obrazu
            for (int i = 0; i < dimensionX; i++)
            {
                for (int j = 0; j < dimensionY; j++)
                {
                    //Ograniczenie końca macierzy obrazu
                    if (j <= dimensionY - FrameFilterSize && i <= dimensionX - FrameFilterSize)
                    {
                        Frameing(i, j, i + (FrameFilterSize - FrameFilterSize / 2), j + (FrameFilterSize - FrameFilterSize / 2));
                    }
                }
            }

            //Naniesienie macierzy medianowej na obraz
            for (int i = 1; i < dimensionX - 1; i++)
            {
                for (int j = 1; j < dimensionY - 1; j++)
                {
                    ProcessedImage[i, j] = MedianaMatrix[i, j];
                }
            }

            return ProcessedImage;
        }

        /// <summary>
        /// Filtrowanie sekwencyjne
        /// </summary>
        /// <returns>Macierz (obraz) po zastosowaniu filtru medianowego</returns>
        public double[,] SeqStart(int filterSize)
        {
            FrameFilterSize = filterSize;
            //Utworzenie macierzy z median dla zadanej macierzy obrazu
            for (int i = 0; i < dimensionX; i++)
            {
                for (int j = 0; j < dimensionY; j++)
                {
                    //Ograniczenie końca macierzy obrazu
                    if (j <= dimensionY - FrameFilterSize && i <= dimensionX - FrameFilterSize)
                    {
                        Frameing(i, j, i + (FrameFilterSize - FrameFilterSize / 2), j + (FrameFilterSize - FrameFilterSize / 2));
                    }
                }
            }

            //Naniesienie macierzy medianowej na obraz
            for (int i = 1; i < dimensionX - 1; i++)
            {
                for (int j = 1; j < dimensionY - 1; j++)
                {
                    ProcessedImage[i, j] = MedianaMatrix[i, j];
                }
            }

            return ProcessedImage;
        }

        /// <summary>
        /// Filtrowanie klatkowe
        /// </summary>
        /// <param name="startX">Pozycja startowa X</param>
        /// <param name="startY">Pozycja startowa Y</param>
        /// <param name="stopX">Pozycja końcowa X</param>
        /// <param name="stopY">Pozycja końcowa Y</param>
        private void Frameing(int startX, int startY, int stopX, int stopY)
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
            MedianaMatrix[a, b] = mediana;
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
    }
}
