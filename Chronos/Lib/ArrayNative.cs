using Chronos.Model;
using System;

namespace Chronos.Lib {
    /// <summary>
    /// Native functions for the System.Array class.
    /// </summary>
    public static class ArrayNative {
        /// <summary>
        /// Gets the length of a dimension for a MD array.
        /// </summary>
        /// <param name="this">The 'this' pointer to the array</param>
        /// <param name="dimension">The dimension to get the length for</param>
        /// <returns>The length of the dimension</returns>
        public static unsafe int GetLengthHelper(IntPtr @this, int dimension) {
            // We already checked for a SD array and an invalid dimension.

            MDArrayObject* array = (MDArrayObject*)@this;
            int* dimensionLengths = &array->FirstLength;

            return dimensionLengths[dimension];
        }
    }
}
