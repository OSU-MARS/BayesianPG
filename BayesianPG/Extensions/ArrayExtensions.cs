using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace BayesianPG.Extensions
{
    public static class ArrayExtensions
    {
        public static int FindIndex(this string[] array, string value)
        {
            return Array.FindIndex(array, element => String.Equals(element, value, StringComparison.Ordinal));
        }

        public static T[,] Resize<T>(this T[,] array, int newColumns, int newRows)
        {
            T[,] resizedArray = new T[newColumns, newRows];

            int maxColumnIndex = Math.Min(array.GetLength(0), newColumns);
            int maxRowIndex = Math.Min(array.GetLength(1), newRows);
            for (int columnIndex = 0; columnIndex < maxColumnIndex; ++columnIndex)
            {
                for (int existingRowIndex = 0; existingRowIndex < maxRowIndex; ++existingRowIndex)
                {
                    resizedArray[columnIndex, existingRowIndex] = array[columnIndex, existingRowIndex];
                }
            }
            return resizedArray;
        }

        public static T[,] Resize<T>(this T[,] array, int newRows)
        {
            int columns = array.GetLength(0);
            T[,] resizedArray = new T[columns, newRows];

            int maxRowIndex = Math.Min(array.GetLength(1), newRows);
            for (int columnIndex = 0; columnIndex < columns; ++columnIndex)
            {
                for (int existingRowIndex = 0; existingRowIndex < maxRowIndex; ++existingRowIndex)
                {
                    resizedArray[columnIndex, existingRowIndex] = array[columnIndex, existingRowIndex];
                }
            }
            return resizedArray;
        }

        // reimplement Array.Resize() since properties can't be passed as ref parameters
        // Could work around this restriction by declaring fields for all arrays and wrapping them in properties
        // but that is substantially more code than this extension method.
        public static T[] Resize<T>(this T[] array, int newLength)
        {
            T[] resizedArray = new T[newLength];

            int maxLength = Math.Min(array.Length, newLength);
            Array.Copy(array, resizedArray, maxLength);
            return resizedArray;
        }

        public static Vector128<float> Sum(this Vector128<float>[] array)
        {
            Vector128<float> sum = Vector128<float>.Zero;
            for (int index = 0; index < array.Length; ++index)
            {
                sum = Avx.Add(sum, array[index]);
            }

            return sum;
        }
    }
}
