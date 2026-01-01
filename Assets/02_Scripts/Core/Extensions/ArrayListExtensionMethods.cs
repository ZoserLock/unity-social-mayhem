using System.Collections.Generic;
using System;

namespace Zen.Core
{
    public static class ArrayListExtensionMethods
    {
        static readonly Random sRand = new Random();
        
        public static void Shuffle<T>(this T[] array)
        {
            var n = array.Length;
            while (n > 1)
            {
                var k = sRand.Next(n--);
                T temp = array[n];
                array[n] = array[k];
                array[k] = temp;
            }
        }

        public static void Shuffle<T>(this List<T> list)
        {
            var n = list.Count;
            while (n > 1)
            {
                int k = sRand.Next(n--);
                T temp = list[n];
                list[n] = list[k];
                list[k] = temp;
            }
        }
        
        // Swap an object with the end object
        public static bool SwapToEndAndRemove<T>(this List<T> list, int element)
        {
            var n = list.Count;
            if (n > 0 && element < n)
            {
                T temp = list[element];
                list[element] = list[n - 1];
                list[n - 1] = temp;
                list.RemoveAt(n - 1);
                return true;
            }
            return false;
        }

        // Get a random element from the list
        public static T RandomElement<T>(this List<T> list)
        {
            return list[sRand.Next(0, list.Count)];
        }

        // Get a random element from the array
        public static T RandomElement<T>(this T[] array)
        {
            return array[sRand.Next(0, array.Length)];
        }

        // Get the grid element x,y from a grid of w,h cells.
        public static T GridElement<T>(this List<T> list, int x, int y, int w, int h)
        {
            if (x < 0 || y < 0)
            {
                return default(T);
            }

            if (y >= h || x >= w)
            {
                return default(T);
            }

            var index = x + y * w;

            if (index > 0 && index < list.Count)
            {
                return list[index];
            }
            return default(T);
        }

        // Get the grid element x,y from a grid of w,h cells.
        public static T GridElement<T>(this T[] array, int x, int y, int w, int h)
        {
            if (x < 0 || y < 0)
            {
                return default(T);
            }

            if (y >= h || x >= w)
            {
                return default(T);
            }

            var index = x + y * w;

            if (index > 0 && index < array.Length)
            {
                return array[index];
            }
            return default(T);
        }

        // Get the last element of an array
        public static T Last<T>(this T[] list)
        {
            var count = list.Length;

            if (count == 0)
            {
                return default(T);
            }

            return list[count - 1];
        }

        // Get the last element of a list
        public static T Last<T>(this List<T> list)
        {
            var count = list.Count;

            if (count == 0)
            {
                return default(T);
            }

            return list[count - 1];
        }

        // Get the first element of an array
        public static T First<T>(this T[] list)
        {
            var count = list.Length;

            if (count == 0)
            {
                return default(T);
            }

            return list[0];
        }

        // Get the first element of a list
        public static T First<T>(this List<T> list)
        {
            var count = list.Count;

            if (count == 0)
            {
                return default(T);
            }

            return list[0];
        }
    }
}
