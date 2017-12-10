using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Xanadu_BRA_Decompress
{

    /// <summary>
    /// Random Utility Methods and Classes.
    /// </summary>
    static class Utilities
    {
        // Set Invalid Path Characters
        static char[] invalid = Path.GetInvalidPathChars().Union(Path.GetInvalidFileNameChars()).ToArray();

        /// <summary>
        /// Returns a subarray of any array.
        /// </summary>
        /// <param name="index">Starting index of the array.</param>
        /// <param name="length">Length of the bytes requested.</param>
        /// <returns>A subarray of the requested array, requested set of bytes.</returns>
        public static T[] SubArray<T>(this T[] data, int index, int length)
        {
            // Allocate requested array bytes.
            T[] subArray = new T[length];

            // Copy into requested array.
            Array.Copy(data, index, subArray, 0, length);

            // Return
            return subArray;
        }

        /// <summary>
        /// Obtains all bytes until 0x00 from current offset.
        /// </summary>
        public static byte[] SubArrayToNullTerminator(this byte[] data, int index)
        {
            // Obtain list of bytes to append.
            List<byte> byteList = new List<byte>();

            // Read byte.
            while ( (data[index] != 0x00) && (data[index] < 128) && (! invalid.Contains((char)data[index])))
            {
                byteList.Add(data[index]);
                index += 1;
            }

            // Return
            return byteList.ToArray();
        }

        /// <summary>
        /// Removes invalid characters from a specified path string.
        /// </summary>
        /// <returns></returns>
        public static string ForceValidFilePath(this string text)
        {
            // Valid path force
            foreach (char c in invalid)
            {
                // Ignore paths
                if (c != '\\') { text = text.Replace(c.ToString(), ""); }  
            }
            return text;
        }
    }
}
