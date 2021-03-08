using System;
using System.Runtime.CompilerServices;

namespace Chronos.Execution {
    public partial class Interpreter {
        /// <summary>
        /// Performs a conversion operation to SByte.
        /// 
        /// Stack: ..., value -> ..., value
        /// </summary>
        /// <param name="overflow">Indicator if overflow should be checked</param>
        /// <param name="unsigned">Indicator if operating on unsinged values</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void InterpretConvertToSByte(bool overflow, bool unsigned) {
            StackItemType type = m_Frame.PeekItemType(0);
            sbyte result;

            switch (type) {
                case StackItemType.Int32: {
                    if (unsigned) {
                        uint value = (uint)m_Frame.Pop<int>();
                        result = overflow ? checked((sbyte)value) : (sbyte)value;
                    } else {
                        int value = m_Frame.Pop<int>();
                        result = overflow ? checked((sbyte)value) : (sbyte)value;
                    }
                    break;
                }
                case StackItemType.Int64: {
                    if (unsigned) {
                        ulong value = (ulong)m_Frame.Pop<long>();
                        result = overflow ? checked((sbyte)value) : (sbyte)value;
                    } else {
                        long value = m_Frame.Pop<long>();
                        result = overflow ? checked((sbyte)value) : (sbyte)value;
                    }
                    break;
                }
                case StackItemType.ByReference:
                case StackItemType.NativeInt: {
                    if (unsigned) {
                        UIntPtr value = (UIntPtr)m_Frame.Pop<IntPtr>().ToPointer();
                        result = overflow ? checked((sbyte)value.ToUInt64()) : (sbyte)value;
                    } else {
                        IntPtr value = m_Frame.Pop<IntPtr>();
                        result = overflow ? checked((sbyte)value.ToInt64()) : (sbyte)value;
                    }
                    break;
                }
                case StackItemType.Double: {
                    double value = m_Frame.Pop<double>();
                    result = overflow ? checked((sbyte)value) : (sbyte)value;
                    break;
                }
                case StackItemType.None:
                case StackItemType.ObjectReference:
                case StackItemType.ValueType:
                default:
                    throw new InvalidProgramException();
            }

            m_Frame.Push(RuntimeType.Int32, result);
        }

        /// <summary>
        /// Performs a conversion operation to Byte.
        /// 
        /// Stack: ..., value -> ..., value
        /// </summary>
        /// <param name="overflow">Indicator if overflow should be checked</param>
        /// <param name="unsigned">Indicator if operating on unsinged values</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void InterpretConvertToByte(bool overflow, bool unsigned) {
            StackItemType type = m_Frame.PeekItemType(0);
            byte result;

            switch (type) {
                case StackItemType.Int32: {
                    if (unsigned) {
                        uint value = (uint)m_Frame.Pop<int>();
                        result = overflow ? checked((byte)value) : (byte)value;
                    } else {
                        int value = m_Frame.Pop<int>();
                        result = overflow ? checked((byte)value) : (byte)value;
                    }
                    break;
                }
                case StackItemType.Int64: {
                    if (unsigned) {
                        ulong value = (ulong)m_Frame.Pop<long>();
                        result = overflow ? checked((byte)value) : (byte)value;
                    } else {
                        long value = m_Frame.Pop<long>();
                        result = overflow ? checked((byte)value) : (byte)value;
                    }
                    break;
                }
                case StackItemType.ByReference:
                case StackItemType.NativeInt: {
                    if (unsigned) {
                        UIntPtr value = (UIntPtr)m_Frame.Pop<IntPtr>().ToPointer();
                        result = overflow ? checked((byte)value.ToUInt64()) : (byte)value;
                    } else {
                        IntPtr value = m_Frame.Pop<IntPtr>();
                        result = overflow ? checked((byte)value.ToInt64()) : (byte)value;
                    }
                    break;
                }
                case StackItemType.Double: {
                    double value = m_Frame.Pop<double>();
                    result = overflow ? checked((byte)value) : (byte)value;
                    break;
                }
                case StackItemType.None:
                case StackItemType.ObjectReference:
                case StackItemType.ValueType:
                default:
                    throw new InvalidProgramException();
            }

            m_Frame.Push(RuntimeType.Int32, result);
        }

        /// <summary>
        /// Performs a conversion operation to Int16.
        /// 
        /// Stack: ..., value -> ..., value
        /// </summary>
        /// <param name="overflow">Indicator if overflow should be checked</param>
        /// <param name="unsigned">Indicator if operating on unsinged values</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void InterpretConvertToInt16(bool overflow, bool unsigned) {
            StackItemType type = m_Frame.PeekItemType(0);
            short result;

            switch (type) {
                case StackItemType.Int32: {
                    if (unsigned) {
                        uint value = (uint)m_Frame.Pop<int>();
                        result = overflow ? checked((short)value) : (short)value;
                    } else {
                        int value = m_Frame.Pop<int>();
                        result = overflow ? checked((short)value) : (short)value;
                    }
                    break;
                }
                case StackItemType.Int64: {
                    if (unsigned) {
                        ulong value = (ulong)m_Frame.Pop<long>();
                        result = overflow ? checked((short)value) : (short)value;
                    } else {
                        long value = m_Frame.Pop<long>();
                        result = overflow ? checked((short)value) : (short)value;
                    }
                    break;
                }
                case StackItemType.ByReference:
                case StackItemType.NativeInt: {
                    if (unsigned) {
                        UIntPtr value = (UIntPtr)m_Frame.Pop<IntPtr>().ToPointer();
                        result = overflow ? checked((short)value.ToUInt64()) : (short)value;
                    } else {
                        IntPtr value = m_Frame.Pop<IntPtr>();
                        result = overflow ? checked((short)value.ToInt64()) : (short)value;
                    }
                    break;
                }
                case StackItemType.Double: {
                    double value = m_Frame.Pop<double>();
                    result = overflow ? checked((short)value) : (short)value;
                    break;
                }
                case StackItemType.None:
                case StackItemType.ObjectReference:
                case StackItemType.ValueType:
                default:
                    throw new InvalidProgramException();
            }

            m_Frame.Push(RuntimeType.Int32, result);
        }

        /// <summary>
        /// Performs a conversion operation to UInt16.
        /// 
        /// Stack: ..., value -> ..., value
        /// </summary>
        /// <param name="overflow">Indicator if overflow should be checked</param>
        /// <param name="unsigned">Indicator if operating on unsinged values</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void InterpretConvertToUInt16(bool overflow, bool unsigned) {
            StackItemType type = m_Frame.PeekItemType(0);
            ushort result;

            switch (type) {
                case StackItemType.Int32: {
                    if (unsigned) {
                        uint value = (uint)m_Frame.Pop<int>();
                        result = overflow ? checked((ushort)value) : (ushort)value;
                    } else {
                        int value = m_Frame.Pop<int>();
                        result = overflow ? checked((ushort)value) : (ushort)value;
                    }
                    break;
                }
                case StackItemType.Int64: {
                    if (unsigned) {
                        ulong value = (ulong)m_Frame.Pop<long>();
                        result = overflow ? checked((ushort)value) : (ushort)value;
                    } else {
                        long value = m_Frame.Pop<long>();
                        result = overflow ? checked((ushort)value) : (ushort)value;
                    }
                    break;
                }
                case StackItemType.ByReference:
                case StackItemType.NativeInt: {
                    if (unsigned) {
                        UIntPtr value = (UIntPtr)m_Frame.Pop<IntPtr>().ToPointer();
                        result = overflow ? checked((ushort)value.ToUInt64()) : (ushort)value;
                    } else {
                        IntPtr value = m_Frame.Pop<IntPtr>();
                        result = overflow ? checked((ushort)value.ToInt64()) : (ushort)value;
                    }
                    break;
                }
                case StackItemType.Double: {
                    double value = m_Frame.Pop<double>();
                    result = overflow ? checked((ushort)value) : (ushort)value;
                    break;
                }
                case StackItemType.None:
                case StackItemType.ObjectReference:
                case StackItemType.ValueType:
                default:
                    throw new InvalidProgramException();
            }

            m_Frame.Push(RuntimeType.Int32, result);
        }

        /// <summary>
        /// Performs a conversion operation to Int32.
        /// 
        /// Stack: ..., value -> ..., value
        /// </summary>
        /// <param name="overflow">Indicator if overflow should be checked</param>
        /// <param name="unsigned">Indicator if operating on unsinged values</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void InterpretConvertToInt32(bool overflow, bool unsigned) {
            StackItemType type = m_Frame.PeekItemType(0);
            int result;

            switch (type) {
                case StackItemType.Int32: {
                    if (unsigned) {
                        uint value = (uint)m_Frame.Pop<int>();
                        result = overflow ? checked((int)value) : (int)value;
                    } else {
                        result = m_Frame.Pop<int>();
                    }
                    break;
                }
                case StackItemType.Int64: {
                    if (unsigned) {
                        ulong value = (ulong)m_Frame.Pop<long>();
                        result = overflow ? checked((int)value) : (int)value;
                    } else {
                        long value = m_Frame.Pop<long>();
                        result = overflow ? checked((int)value) : (int)value;
                    }
                    break;
                }
                case StackItemType.ByReference:
                case StackItemType.NativeInt: {
                    if (unsigned) {
                        UIntPtr value = (UIntPtr)m_Frame.Pop<IntPtr>().ToPointer();
                        result = overflow ? checked((int)value.ToUInt64()) : (int)value;
                    } else {
                        IntPtr value = m_Frame.Pop<IntPtr>();
                        result = overflow ? checked((int)value.ToInt64()) : (int)value;
                    }
                    break;
                }
                case StackItemType.Double: {
                    double value = m_Frame.Pop<double>();
                    result = overflow ? checked((int)value) : (int)value;
                    break;
                }
                case StackItemType.None:
                case StackItemType.ObjectReference:
                case StackItemType.ValueType:
                default:
                    throw new InvalidProgramException();
            }

            m_Frame.Push(RuntimeType.Int32, result);
        }

        /// <summary>
        /// Performs a conversion operation to UInt32.
        /// 
        /// Stack: ..., value -> ..., value
        /// </summary>
        /// <param name="overflow">Indicator if overflow should be checked</param>
        /// <param name="unsigned">Indicator if operating on unsinged values</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void InterpretConvertToUInt32(bool overflow, bool unsigned) {
            StackItemType type = m_Frame.PeekItemType(0);
            uint result;

            switch (type) {
                case StackItemType.Int32: {
                    if (unsigned) {
                        result = (uint)m_Frame.Pop<int>();
                    } else {
                        int value = m_Frame.Pop<int>();
                        result = overflow ? checked((uint)value) : (uint)value;
                    }
                    break;
                }
                case StackItemType.Int64: {
                    if (unsigned) {
                        ulong value = (ulong)m_Frame.Pop<long>();
                        result = overflow ? checked((uint)value) : (uint)value;
                    } else {
                        long value = m_Frame.Pop<long>();
                        result = overflow ? checked((uint)value) : (uint)value;
                    }
                    break;
                }
                case StackItemType.ByReference:
                case StackItemType.NativeInt: {
                    if (unsigned) {
                        UIntPtr value = (UIntPtr)m_Frame.Pop<IntPtr>().ToPointer();
                        result = overflow ? checked((uint)value.ToUInt64()) : (uint)value;
                    } else {
                        IntPtr value = m_Frame.Pop<IntPtr>();
                        result = overflow ? checked((uint)value.ToInt64()) : (uint)value;
                    }
                    break;
                }
                case StackItemType.Double: {
                    double value = m_Frame.Pop<double>();
                    result = overflow ? checked((uint)value) : (uint)value;
                    break;
                }
                case StackItemType.None:
                case StackItemType.ObjectReference:
                case StackItemType.ValueType:
                default:
                    throw new InvalidProgramException();
            }

            m_Frame.Push(RuntimeType.Int32, result);
        }

        /// <summary>
        /// Performs a conversion operation to Int64.
        /// 
        /// Stack: ..., value -> ..., value
        /// </summary>
        /// <param name="overflow">Indicator if overflow should be checked</param>
        /// <param name="unsigned">Indicator if operating on unsinged values</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void InterpretConvertToInt64(bool overflow, bool unsigned) {
            StackItemType type = m_Frame.PeekItemType(0);
            long result;

            switch (type) {
                case StackItemType.Int32: {
                    if (unsigned) {
                        result = (uint)m_Frame.Pop<int>();
                    } else {
                        result = m_Frame.Pop<int>();
                    }
                    break;
                }
                case StackItemType.Int64: {
                    if (unsigned) {
                        ulong value = (ulong)m_Frame.Pop<long>();
                        result = overflow ? checked((long)value) : (long)value;
                    } else {
                        result = m_Frame.Pop<long>();
                    }
                    break;
                }
                case StackItemType.ByReference:
                case StackItemType.NativeInt: {
                    if (unsigned) {
                        UIntPtr value = (UIntPtr)m_Frame.Pop<IntPtr>().ToPointer();
                        result = overflow ? checked((long)value.ToUInt64()) : (long)value;
                    } else {
                        result = m_Frame.Pop<IntPtr>().ToInt64();
                    }
                    break;
                }
                case StackItemType.Double: {
                    double value = m_Frame.Pop<double>();
                    result = overflow ? checked((long)value) : (long)value;
                    break;
                }
                case StackItemType.None:
                case StackItemType.ObjectReference:
                case StackItemType.ValueType:
                default:
                    throw new InvalidProgramException();
            }

            m_Frame.Push(RuntimeType.Int64, result);
        }

        /// <summary>
        /// Performs a conversion operation to UInt64.
        /// 
        /// Stack: ..., value -> ..., value
        /// </summary>
        /// <param name="overflow">Indicator if overflow should be checked</param>
        /// <param name="unsigned">Indicator if operating on unsinged values</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void InterpretConvertToUInt64(bool overflow, bool unsigned) {
            StackItemType type = m_Frame.PeekItemType(0);
            ulong result;

            switch (type) {
                case StackItemType.Int32: {
                    if (unsigned) {
                        result = (uint)m_Frame.Pop<int>();
                    } else {
                        uint value = (uint)m_Frame.Pop<int>();
                        result = overflow ? checked(value) : value;
                    }
                    break;
                }
                case StackItemType.Int64: {
                    if (unsigned) {
                        result = (ulong)m_Frame.Pop<long>();
                    } else {
                        long value = m_Frame.Pop<long>();
                        result = overflow ? checked((ulong)value) : (ulong)value;
                    }
                    break;
                }
                case StackItemType.ByReference:
                case StackItemType.NativeInt: {
                    if (unsigned) {
                        UIntPtr value = (UIntPtr)m_Frame.Pop<IntPtr>().ToPointer();
                        result = value.ToUInt64();
                    } else {
                        IntPtr value = m_Frame.Pop<IntPtr>();
                        result = overflow ? checked((ulong)value.ToInt64()) : (ulong)value;
                    }
                    break;
                }
                case StackItemType.Double: {
                    double value = m_Frame.Pop<double>();
                    result = overflow ? checked((ulong)value) : (ulong)value;
                    break;
                }
                case StackItemType.None:
                case StackItemType.ObjectReference:
                case StackItemType.ValueType:
                default:
                    throw new InvalidProgramException();
            }

            m_Frame.Push(RuntimeType.Int64, result);
        }

        /// <summary>
        /// Performs a conversion operation to IntPtr.
        /// 
        /// Stack: ..., value -> ..., value
        /// </summary>
        /// <param name="unsigned">Indicator if operating on unsinged values</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void InterpretConvertToIntPtr(bool unsigned) {
            StackItemType type = m_Frame.PeekItemType(0);
            IntPtr result;

            switch (type) {
                case StackItemType.Int32: {
                    if (unsigned) {
                        uint value = (uint)m_Frame.Pop<int>();
                        result = (IntPtr)value;
                    } else {
                        int value = m_Frame.Pop<int>();
                        result = (IntPtr)value;
                    }
                    break;
                }
                case StackItemType.Int64: {
                    if (unsigned) {
                        ulong value = (ulong)m_Frame.Pop<long>();
                        result = (IntPtr)value;
                    } else {
                        result = (IntPtr)m_Frame.Pop<long>();
                    }
                    break;
                }
                case StackItemType.ByReference:
                case StackItemType.NativeInt:  {
                    if (unsigned) {
                        UIntPtr value = (UIntPtr)m_Frame.Pop<IntPtr>().ToPointer();
                        result = (IntPtr)value.ToPointer();
                    } else {
                        result = m_Frame.Pop<IntPtr>();
                    }
                    break;
                }
                case StackItemType.Double: {
                    result = (IntPtr)m_Frame.Pop<double>();
                    break;
                }
                case StackItemType.None:
                case StackItemType.ObjectReference:
                case StackItemType.ValueType:
                default:
                    throw new InvalidProgramException();
            }

            m_Frame.Push(RuntimeType.NativeInt, result);
        }

        /// <summary>
        /// Performs a conversion operation to UIntPtr.
        /// 
        /// Stack: ..., value -> ..., value
        /// </summary>
        /// <param name="unsigned">Indicator if operating on unsinged values</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void InterpretConvertToUIntPtr(bool unsigned) {
            StackItemType type = m_Frame.PeekItemType(0);
            UIntPtr result;

            switch (type) {
                case StackItemType.Int32: {
                    if (unsigned) {
                        uint value = (uint)m_Frame.Pop<int>();
                        result = (UIntPtr)value;
                    } else {
                        int value = m_Frame.Pop<int>();
                        result = (UIntPtr)value;
                    }
                    break;
                }
                case StackItemType.Int64: {
                    if (unsigned) {
                        ulong value = (ulong)m_Frame.Pop<long>();
                        result = (UIntPtr)value;
                    } else {
                        result = (UIntPtr)m_Frame.Pop<long>();
                    }
                    break;
                }
                case StackItemType.ByReference:
                case StackItemType.NativeInt:  {
                    result = (UIntPtr)m_Frame.Pop<IntPtr>().ToPointer();
                    break;
                }
                case StackItemType.Double: {
                    result = (UIntPtr)m_Frame.Pop<double>();
                    break;
                }
                case StackItemType.None:
                case StackItemType.ObjectReference:
                case StackItemType.ValueType:
                default:
                    throw new InvalidProgramException();
            }

            m_Frame.Push(RuntimeType.NativeInt, result);
        }

        /// <summary>
        /// Performs a conversion operation to Double.
        /// 
        /// Stack: ..., value -> ..., value
        /// </summary>
        /// <param name="overflow">Indicator if overflow should be checked</param>
        /// <param name="unsigned">Indicator if operating on unsinged values</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void InterpretConvertToDouble(bool overflow, bool unsigned) {
            StackItemType type = m_Frame.PeekItemType(0);
            double result;

            switch (type) {
                case StackItemType.Int32: {
                    if (unsigned) {
                        uint value = (uint)m_Frame.Pop<int>();
                        result = overflow ? checked(value) : value;
                    } else {
                        int value = m_Frame.Pop<int>();
                        result = overflow ? checked(value) : value;
                    }
                    break;
                }
                case StackItemType.Int64: {
                    if (unsigned) {
                        ulong value = (ulong)m_Frame.Pop<long>();
                        result = overflow ? checked(value) : value;
                    } else {
                        long value = m_Frame.Pop<long>();
                        result = overflow ? checked(value) : value;
                    }
                    break;
                }
                case StackItemType.ByReference:
                case StackItemType.NativeInt:  {
                    if (unsigned) {
                        UIntPtr value = (UIntPtr)m_Frame.Pop<IntPtr>().ToPointer();
                        result = overflow ? checked(value.ToUInt64()) : (double)value;
                    } else {
                        IntPtr value = m_Frame.Pop<IntPtr>();
                        result = overflow ? checked(value.ToInt64()) : (double)value;
                    }
                    break;
                }
                case StackItemType.Double: {
                    result = m_Frame.Pop<double>();
                    break;
                }
                case StackItemType.None:
                case StackItemType.ObjectReference:
                case StackItemType.ValueType:
                default:
                    throw new InvalidProgramException();
            }

            m_Frame.Push(RuntimeType.Double, result);
        }
    }
}
