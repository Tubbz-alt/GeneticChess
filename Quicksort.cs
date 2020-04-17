using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticChess
{
    static class Quicksort
    {
        private static void Swap<T>(T[] array, int a, int b)
        {
            var intermediary = array[a];
            array[a] = array[b];
            array[b] = intermediary;
        }
        //Pushes the better networks further down the array
        private static int Partition(NN[] array, int min, int max)
        {
            Random r = new Random();
            NN pivot = array[(min + (max - min) / 2)];
            int i = min - 1;
            int j = max + 1;
            do
            {
                //Randomly assign color
                bool isW = r.Next(0, 2) == 1 ? true : false;
                //While pivot wins to array[i]
                do { i++; } while (i < j && (pivot.SetColor(isW)).Vs(array[i].SetColor(!isW)));
                //And loses to array[j]
                do { j--; } while (j > i && !(pivot.SetColor(isW)).Vs(array[j].SetColor(!isW)));
                if (i > j) { return j; }
                Swap(array, i, j);
            } while (true);
        }
        public static void Quick(NN[] array, int min, int max)
        {
            if (min < max)
            {
                int pivot = Partition(array, min, max);
                Quick(array, min, pivot);
                Quick(array, pivot + 1, max);
            }
        }
    }
}
