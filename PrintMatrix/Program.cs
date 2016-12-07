using Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrintMatrix
{
    class Program
    {
        /// <summary>
        /// Macierz z pdf'a Image Filtering_2up strona 3
        /// </summary>
        static byte[,] colorMatrix = new byte[,] {
            { 1, 4, 0, 1, 3, 1 },
            { 2, 2, 4, 2, 2, 3 },
            { 1, 0, 1, 0, 1, 0 },
            { 1, 2, 1, 0, 2, 2 },
            { 2, 5, 3, 1, 2, 5 },
            { 1, 1, 4, 2, 3, 0 } };

        static void Main(string[] args)
        {
            Console.Write("Podaj wielkosc filtra: ");
            int sizeMatrix = Convert.ToInt32(Console.ReadLine());
            //MedianaFilter medianaFilter = new MedianaFilter(colorMatrix, Convert.ToInt32(sizeMatrix));
            //var a = medianaFilter.SeqStart();

            Console.Clear();
            Console.WriteLine("Macierz wejsciowa");
            PrintMatrix(colorMatrix);
            Console.WriteLine("\nMacierz z medianami");
            //PrintMatrix(medianaFilter.MedianaMatrix);
            Console.WriteLine("\nMacierz wynikowa");
            //PrintMatrix(a);

            Console.ReadLine();
        }

        /// <summary>
        /// Wyświetlenie macierzy w układzie tabelarycznym
        /// </summary>
        /// <param name="matrix">Macierz</param>
        static private void PrintMatrix(byte[,] matrix)
        {
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    Console.Write(matrix[i, j] + "\t");
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }

        /// <summary>
        /// Podział macierzy na ramki do filtracji
        /// </summary>
        /// <param name="startX">Pozycja startowa X</param>
        /// <param name="startY">Pozycja startowa Y</param>
        /// <param name="stopX">Pozycja końcowa X</param>
        /// <param name="stopY">Pozycja końcowa Y</param>
        static void DivMatrix(int startX, int startY, int stopX, int stopY)
        {
            for (int i = startX; i <= stopX; i++)
            {
                for (int j = startY; j <= stopY; j++)
                {
                    Console.Write(colorMatrix[i, j] + "\t");
                }
                Console.WriteLine();
            }
        }
    }
}
