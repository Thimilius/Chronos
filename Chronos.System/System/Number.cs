using System.Runtime.CompilerServices;

namespace System {
    internal static class Number {
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern string CharToString(char value);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern string SByteToString(sbyte value);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern string ByteToString(byte value);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern string Int16ToString(short value);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern string UInt16ToString(ushort value);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern string Int32ToString(int value);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern string UInt32ToString(uint value);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern string Int64ToString(long value);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern string UInt64ToString(ulong value);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern string IntPtrToString(IntPtr value);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern string UIntPtrToString(UIntPtr value);
    }
}
