using Chronos.Model;
using System;

namespace Chronos.Lib {
    /// <summary>
    /// Native functions for the System.Number class.
    /// </summary>
    public static class NumberNative {
        /// <summary>
        /// Converts a char to a string.
        /// </summary>
        /// <param name="value">The char value to convert</param>
        /// <returns>The converted string</returns>
        public static IntPtr CharToString(char value) {
            return ObjectModel.AllocateStringFromLiteral(value.ToString());
        }

        /// <summary>
        /// Converts a sbyte to a string.
        /// </summary>
        /// <param name="value">The sbyte value to convert</param>
        /// <returns>The converted string</returns>
        public static IntPtr SByteToString(sbyte value) {
            return ObjectModel.AllocateStringFromLiteral(value.ToString());
        }

        /// <summary>
        /// Converts a byte to a string.
        /// </summary>
        /// <param name="value">The byte value to convert</param>
        /// <returns>The converted string</returns>
        public static IntPtr ByteToString(byte value) {
            return ObjectModel.AllocateStringFromLiteral(value.ToString());
        }

        /// <summary>
        /// Converts a short to a string.
        /// </summary>
        /// <param name="value">The short value to convert</param>
        /// <returns>The converted string</returns>
        public static IntPtr Int16ToString(short value) {
            return ObjectModel.AllocateStringFromLiteral(value.ToString());
        }

        /// <summary>
        /// Converts a ushort to a string.
        /// </summary>
        /// <param name="value">The ushort value to convert</param>
        /// <returns>The converted string</returns>
        public static IntPtr UInt16ToString(ushort value) {
            return ObjectModel.AllocateStringFromLiteral(value.ToString());
        }

        /// <summary>
        /// Converts a int to a string.
        /// </summary>
        /// <param name="value">The int value to convert</param>
        /// <returns>The converted string</returns>
        public static IntPtr Int32ToString(int value) {
            return ObjectModel.AllocateStringFromLiteral(value.ToString());
        }

        /// <summary>
        /// Converts a uint to a string.
        /// </summary>
        /// <param name="value">The uint value to convert</param>
        /// <returns>The converted string</returns>
        public static IntPtr UInt32ToString(uint value) {
            return ObjectModel.AllocateStringFromLiteral(value.ToString());
        }

        /// <summary>
        /// Converts a long to a string.
        /// </summary>
        /// <param name="value">The long value to convert</param>
        /// <returns>The converted string</returns>
        public static IntPtr Int64ToString(long value) {
            return ObjectModel.AllocateStringFromLiteral(value.ToString());
        }

        /// <summary>
        /// Converts a ulong to a string.
        /// </summary>
        /// <param name="value">The ulong value to convert</param>
        /// <returns>The converted string</returns>
        public static IntPtr UInt64ToString(ulong value) {
            return ObjectModel.AllocateStringFromLiteral(value.ToString());
        }

        /// <summary>
        /// Converts a IntPtr to a string.
        /// </summary>
        /// <param name="value">The IntPtr value to convert</param>
        /// <returns>The converted string</returns>
        public static IntPtr IntPtrToString(IntPtr value) {
            return ObjectModel.AllocateStringFromLiteral(value.ToString());
        }

        /// <summary>
        /// Converts a UIntPtr to a string.
        /// </summary>
        /// <param name="value">The UIntPtr value to convert</param>
        /// <returns>The converted string</returns>
        public static IntPtr UIntPtrToString(UIntPtr value) {
            return ObjectModel.AllocateStringFromLiteral(value.ToString());
        }
    }
}
