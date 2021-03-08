using System;
using System.Diagnostics;
using System.Reflection.Metadata;

namespace Chronos.Execution {
    public partial class Interpreter {
        /// <summary>
        /// Performs a binary operation with two values on the stack and pushes the result.
        /// 
        /// Stack: ..., value, value -> ..., value
        /// </summary>
        /// <param name="opCode">The operation code containing the binary operation</param>
        private unsafe void InterpretBinary(ILOpCode opCode) {
            // This is a little helper function to throw a DividyByZero exception.
            T DivideByZero<T>() {
                VirtualMachine.ExceptionEngine.ThrowDivideByZeroException();
                return default(T);
            }

            StackItemType firstType = m_Frame.PeekItemType(0);
            StackItemType secondType = m_Frame.PeekItemType(1);

            switch (firstType) {
                case StackItemType.Int32: {
                    if (secondType == StackItemType.Int32) {
                        int value1 = m_Frame.Pop<int>();
                        int value2 = m_Frame.Pop<int>();
                        int result = opCode switch
                        {
                            ILOpCode.Add => value1 + value2,
                            ILOpCode.Add_ovf => checked(value1 + value2),
                            ILOpCode.Add_ovf_un => (int)checked((uint)value1 + (uint)value2),
                            ILOpCode.Sub => value2 - value1,
                            ILOpCode.Sub_ovf => checked(value2 - value1),
                            ILOpCode.Sub_ovf_un => (int)checked((uint)value2 - (uint)value1),
                            ILOpCode.Mul => value1 * value2,
                            ILOpCode.Mul_ovf => checked(value1 * value2),
                            ILOpCode.Mul_ovf_un => (int)checked((uint)value1 * (uint)value2),
                            ILOpCode.Div => value1 != 0 ? value2 / value1 : DivideByZero<int>(),
                            ILOpCode.Div_un => value1 != 0 ?(int)((uint)value2 / (uint)value1) : DivideByZero<int>(),
                            ILOpCode.Rem => value2 % value1,
                            ILOpCode.Rem_un => (int)((uint)value2 % (uint)value1),
                            ILOpCode.And => value1 & value2,
                            ILOpCode.Or => value1 | value2,
                            ILOpCode.Xor => value1 ^ value2,
                            _ => throw new InvalidProgramException(),
                        };
                        // Int32 op Int32 = Int32
                        m_Frame.Push(RuntimeType.Int32, result);
                    } else if (secondType == StackItemType.NativeInt) {
                        IntPtr value1 = (IntPtr)m_Frame.Pop<int>();
                        IntPtr value2 = m_Frame.Pop<IntPtr>();
                        IntPtr result = opCode switch
                        {
                            ILOpCode.Add => (IntPtr)((long)value1 + (long)value2),
                            ILOpCode.Add_ovf => (IntPtr)checked((long)value1 + (long)value2),
                            ILOpCode.Add_ovf_un => (IntPtr)(long)checked((ulong)(UIntPtr)value1.ToPointer() + (ulong)(UIntPtr)value2.ToPointer()),
                            ILOpCode.Sub => (IntPtr)((long)value1 - (long)value2),
                            ILOpCode.Sub_ovf => (IntPtr)checked((long)value1 - (long)value2),
                            ILOpCode.Sub_ovf_un => (IntPtr)(long)checked((ulong)(UIntPtr)value1.ToPointer() - (ulong)(UIntPtr)value2.ToPointer()),
                            ILOpCode.Mul => (IntPtr)((long)value1 * (long)value2),
                            ILOpCode.Mul_ovf => (IntPtr)checked((long)value1 * (long)value2),
                            ILOpCode.Mul_ovf_un => (IntPtr)(long)checked((ulong)(UIntPtr)value1.ToPointer() * (ulong)(UIntPtr)value2.ToPointer()),
                            ILOpCode.Div => value1 != IntPtr.Zero ? (IntPtr)((long)value1 / (long)value2) : DivideByZero<IntPtr>(),
                            ILOpCode.Div_un => value1 != IntPtr.Zero ? (IntPtr)(long)checked((ulong)(UIntPtr)value1.ToPointer() / (ulong)(UIntPtr)value2.ToPointer()) : DivideByZero<IntPtr>(),
                            ILOpCode.Rem => (IntPtr)((long)value1 % (long)value2),
                            ILOpCode.Rem_un => (IntPtr)(long)checked((ulong)(UIntPtr)value1.ToPointer() % (ulong)(UIntPtr)value2.ToPointer()),
                            ILOpCode.And => (IntPtr)((long)value1 & (long)value2),
                            ILOpCode.Or => (IntPtr)((long)value1 | (long)value2),
                            ILOpCode.Xor => (IntPtr)((long)value1 ^ (long)value2),
                            _ => throw new InvalidProgramException(),
                        };
                        // Int32 op NativeInt = NativeInt
                        m_Frame.Push(RuntimeType.NativeInt, result);
                    } else if (secondType == StackItemType.ByReference) {
                        IntPtr value1 = (IntPtr)m_Frame.Pop<int>();
                        RuntimeType referenceType = m_Frame.PeekType(0);
                        IntPtr value2 = m_Frame.Pop<IntPtr>();
                        IntPtr result = opCode switch
                        {
                            ILOpCode.Add => (IntPtr)((long)value1 + (long)value2),
                            ILOpCode.Add_ovf_un => (IntPtr)(long)checked((ulong)(UIntPtr)value1.ToPointer() + (ulong)(UIntPtr)value2.ToPointer()),
                            _ => throw new InvalidProgramException(),
                        };
                        // Int32 op ByReference = ByReference
                        m_Frame.Push(referenceType, result);
                    } else {
                        throw new InvalidProgramException();
                    }
                    break;
                }
                case StackItemType.Int64: {
                    if (secondType == StackItemType.Int64) {
                        long value1 = m_Frame.Pop<long>();
                        long value2 = m_Frame.Pop<long>();
                        long result = opCode switch
                        {
                            ILOpCode.Add => value1 + value2,
                            ILOpCode.Add_ovf => checked(value1 + value2),
                            ILOpCode.Add_ovf_un => (long)checked((ulong)value1 + (ulong)value2),
                            ILOpCode.Sub => value2 - value1,
                            ILOpCode.Sub_ovf => checked(value2 - value1),
                            ILOpCode.Sub_ovf_un => (long)checked((ulong)value2 - (ulong)value1),
                            ILOpCode.Mul => value1 * value2,
                            ILOpCode.Mul_ovf => checked(value1 * value2),
                            ILOpCode.Mul_ovf_un => (long)checked((ulong)value1 * (ulong)value2),
                            ILOpCode.Div => value1 != 0 ? value2 / value1 : DivideByZero<long>(),
                            ILOpCode.Div_un => value1 != 0 ? (long)((ulong)value2 / (ulong)value1) : DivideByZero<long>(),
                            ILOpCode.Rem => value2 % value1,
                            ILOpCode.Rem_un => (long)((ulong)value2 % (ulong)value1),
                            ILOpCode.And => value1 & value2,
                            ILOpCode.Or => value1 | value2,
                            ILOpCode.Xor => value1 ^ value2,
                            _ => throw new InvalidProgramException(),
                        };
                        // Int64 op Int64 = Int64
                        m_Frame.Push(RuntimeType.Int64, result);
                    } else {
                        throw new InvalidProgramException();
                    }
                    break;
                }
                case StackItemType.NativeInt: {
                    if (secondType == StackItemType.Int32) {
                        IntPtr value1 = (IntPtr)m_Frame.Pop<int>();
                        IntPtr value2 = m_Frame.Pop<IntPtr>();
                        IntPtr result = opCode switch
                        {
                            ILOpCode.Add => (IntPtr)((long)value1 + (long)value2),
                            ILOpCode.Add_ovf => (IntPtr)checked((long)value1 + (long)value2),
                            ILOpCode.Add_ovf_un => (IntPtr)(long)checked((ulong)(UIntPtr)value1.ToPointer() + (ulong)(UIntPtr)value2.ToPointer()),
                            ILOpCode.Sub => (IntPtr)((long)value1 - (long)value2),
                            ILOpCode.Sub_ovf => (IntPtr)checked((long)value1 - (long)value2),
                            ILOpCode.Sub_ovf_un => (IntPtr)(long)checked((ulong)(UIntPtr)value1.ToPointer() - (ulong)(UIntPtr)value2.ToPointer()),
                            ILOpCode.Mul => (IntPtr)((long)value1 * (long)value2),
                            ILOpCode.Mul_ovf => (IntPtr)checked((long)value1 * (long)value2),
                            ILOpCode.Mul_ovf_un => (IntPtr)(long)checked((ulong)(UIntPtr)value1.ToPointer() * (ulong)(UIntPtr)value2.ToPointer()),
                            ILOpCode.Div => value1 != IntPtr.Zero ? (IntPtr)((long)value1 / (long)value2) : DivideByZero<IntPtr>(),
                            ILOpCode.Div_un => value1 != IntPtr.Zero ? (IntPtr)(long)checked((ulong)(UIntPtr)value1.ToPointer() / (ulong)(UIntPtr)value2.ToPointer()) : DivideByZero<IntPtr>(),
                            ILOpCode.Rem => (IntPtr)((long)value1 % (long)value2),
                            ILOpCode.Rem_un => (IntPtr)(long)checked((ulong)(UIntPtr)value1.ToPointer() % (ulong)(UIntPtr)value2.ToPointer()),
                            ILOpCode.And => (IntPtr)((long)value1 & (long)value2),
                            ILOpCode.Or => (IntPtr)((long)value1 | (long)value2),
                            ILOpCode.Xor => (IntPtr)((long)value1 ^ (long)value2),
                            _ => throw new InvalidProgramException(),
                        };
                        // NativeInt op Int32 = NativeInt
                        m_Frame.Push(RuntimeType.Int32, result);
                    } else if (secondType == StackItemType.NativeInt) {
                        IntPtr value1 = m_Frame.Pop<IntPtr>();
                        IntPtr value2 = m_Frame.Pop<IntPtr>();
                        IntPtr result = opCode switch
                        {
                            ILOpCode.Add => (IntPtr)((long)value1 + (long)value2),
                            ILOpCode.Add_ovf => (IntPtr)checked((long)value1 + (long)value2),
                            ILOpCode.Add_ovf_un => (IntPtr)(long)checked((ulong)(UIntPtr)value1.ToPointer() + (ulong)(UIntPtr)value2.ToPointer()),
                            ILOpCode.Sub => (IntPtr)((long)value1 - (long)value2),
                            ILOpCode.Sub_ovf => (IntPtr)checked((long)value1 - (long)value2),
                            ILOpCode.Sub_ovf_un => (IntPtr)(long)checked((ulong)(UIntPtr)value1.ToPointer() - (ulong)(UIntPtr)value2.ToPointer()),
                            ILOpCode.Mul => (IntPtr)((long)value1 * (long)value2),
                            ILOpCode.Mul_ovf => (IntPtr)checked((long)value1 * (long)value2),
                            ILOpCode.Mul_ovf_un => (IntPtr)(long)checked((ulong)(UIntPtr)value1.ToPointer() * (ulong)(UIntPtr)value2.ToPointer()),
                            ILOpCode.Div => value1 != IntPtr.Zero ? (IntPtr)((long)value1 / (long)value2) : DivideByZero<IntPtr>(),
                            ILOpCode.Div_un => value1 != IntPtr.Zero ? (IntPtr)(long)checked((ulong)(UIntPtr)value1.ToPointer() / (ulong)(UIntPtr)value2.ToPointer()) : DivideByZero<IntPtr>(),
                            ILOpCode.Rem => (IntPtr)((long)value1 % (long)value2),
                            ILOpCode.Rem_un => (IntPtr)(long)checked((ulong)(UIntPtr)value1.ToPointer() % (ulong)(UIntPtr)value2.ToPointer()),
                            ILOpCode.And => (IntPtr)((long)value1 & (long)value2),
                            ILOpCode.Or => (IntPtr)((long)value1 | (long)value2),
                            ILOpCode.Xor => (IntPtr)((long)value1 ^ (long)value2),
                            _ => throw new InvalidProgramException(),
                        };
                        // NativeInt op NativeInt = NativeInt
                        m_Frame.Push(RuntimeType.NativeInt, result);
                    } else if (secondType == StackItemType.ByReference) {
                        IntPtr value1 = m_Frame.Pop<IntPtr>();
                        RuntimeType referenceType = m_Frame.PeekType(0);
                        IntPtr value2 = m_Frame.Pop<IntPtr>();
                            IntPtr result = opCode switch
                        {
                            ILOpCode.Add => (IntPtr)((long)value1 + (long)value2),
                            ILOpCode.Add_ovf_un => (IntPtr)(long)checked((ulong)(UIntPtr)value1.ToPointer() + (ulong)(UIntPtr)value2.ToPointer()),
                            _ => throw new InvalidProgramException(),
                        };
                        // NativeInt op ByReference = ByReference
                        m_Frame.Push(referenceType, result);
                    } else {
                        throw new InvalidProgramException();
                    }
                    break;
                }
                case StackItemType.Double: {
                    if (secondType == StackItemType.Double) {
                        double value1 = m_Frame.Pop<double>();
                        double value2 = m_Frame.Pop<double>();

                        if (!double.IsFinite(value1) || !double.IsFinite(value2)) {
                            throw new NotFiniteNumberException();
                        }

                        double result = opCode switch
                        {
                            ILOpCode.Add => value1 + value2,
                            ILOpCode.Sub => value2 - value1,
                            ILOpCode.Mul => value1 * value2,
                            ILOpCode.Div => value1 != 0 ? value2 / value1 : DivideByZero<double>(),
                            ILOpCode.Rem => value2 % value1,
                            _ => throw new InvalidProgramException(),
                        };
                        // Double op Double = Double
                        m_Frame.Push(RuntimeType.Double, result);
                    } else {
                        throw new InvalidProgramException();
                    }
                    break;
                }
                case StackItemType.ByReference:
                    if (secondType == StackItemType.Int32) {
                        RuntimeType referenceType = m_Frame.PeekType(0);
                        IntPtr value1 = m_Frame.Pop<IntPtr>();
                        IntPtr value2 = (IntPtr)m_Frame.Pop<int>();
                        IntPtr result = opCode switch
                        {
                            ILOpCode.Add => (IntPtr)((long)value1 + (long)value2),
                            ILOpCode.Add_ovf_un => (IntPtr)(long)checked((ulong)(UIntPtr)value1.ToPointer() + (ulong)(UIntPtr)value2.ToPointer()),
                            ILOpCode.Sub => (IntPtr)((long)value1 - (long)value2),
                            ILOpCode.Sub_ovf_un => (IntPtr)(long)checked((ulong)(UIntPtr)value1.ToPointer() - (ulong)(UIntPtr)value2.ToPointer()),
                            _ => throw new InvalidProgramException(),
                        };
                        // ByReference op Int32 = ByReference
                        m_Frame.Push(referenceType, result);
                    } else if (secondType == StackItemType.Int32) {
                        RuntimeType referenceType = m_Frame.PeekType(0);
                        IntPtr value1 = m_Frame.Pop<IntPtr>();
                        IntPtr value2 = (IntPtr)m_Frame.Pop<long>();
                        IntPtr result = opCode switch
                        {
                            ILOpCode.Add => (IntPtr)((long)value1 + (long)value2),
                            ILOpCode.Add_ovf_un => (IntPtr)(long)checked((ulong)(UIntPtr)value1.ToPointer() + (ulong)(UIntPtr)value2.ToPointer()),
                            ILOpCode.Sub => (IntPtr)((long)value1 - (long)value2),
                            ILOpCode.Sub_ovf_un => (IntPtr)(long)checked((ulong)(UIntPtr)value1.ToPointer() - (ulong)(UIntPtr)value2.ToPointer()),
                            _ => throw new InvalidProgramException(),
                        };
                        // ByReference op Int64 = ByReference
                        m_Frame.Push(referenceType, result);
                    } else if (secondType == StackItemType.ByReference) {
                        IntPtr value1 = m_Frame.Pop<IntPtr>();
                        IntPtr value2 = m_Frame.Pop<IntPtr>();
                        IntPtr result = opCode switch
                        {
                            ILOpCode.Sub => (IntPtr)((long)value1 - (long)value2),
                            ILOpCode.Sub_ovf_un => (IntPtr)(long)checked((ulong)(UIntPtr)value1.ToPointer() - (ulong)(UIntPtr)value2.ToPointer()),
                            _ => throw new InvalidProgramException(),
                        };
                        // ByReference op ByReference = NativeInt
                        m_Frame.Push(RuntimeType.NativeInt, result);
                    } else {
                        throw new InvalidProgramException();
                    }
                    break;
                case StackItemType.None:
                case StackItemType.ObjectReference:
                case StackItemType.ValueType:
                default:
                    throw new InvalidProgramException();
            }
        }

        /// <summary>
        /// Performs a shift operation on a value and pushes the result.
        /// 
        /// Stack: ..., value, shift amount -> ..., result
        /// </summary>
        /// <param name="opCode">The operation code containing the shift operation</param>
        private unsafe void InterpretShift(ILOpCode opCode) {
            StackItemType firstType = m_Frame.PeekItemType(0);
            StackItemType secondType = m_Frame.PeekItemType(1);

            if (firstType > StackItemType.NativeInt) {
                throw new InvalidProgramException();
            }

            int shiftAmount = m_Frame.Pop<int>();
            switch (secondType) {
                case StackItemType.Int32: {
                    int value = m_Frame.Pop<int>();

                    switch (opCode) {
                        case ILOpCode.Shl:
                            value <<= shiftAmount;
                            break;
                        case ILOpCode.Shr:
                            value >>= shiftAmount;
                            break;
                        case ILOpCode.Shr_un:
                            value = (int)((uint)value >> shiftAmount);
                            break;
                        default:
                            Debug.Assert(false);
                            break;
                    }

                    m_Frame.Push(RuntimeType.Int32, value);
                    break;
                }
                case StackItemType.Int64: {
                    long value = m_Frame.Pop<long>();

                    switch (opCode) {
                        case ILOpCode.Shl:
                            value <<= shiftAmount;
                            break;
                        case ILOpCode.Shr:
                            value >>= shiftAmount;
                            break;
                        case ILOpCode.Shr_un:
                            value = (long)((ulong)value >> shiftAmount);
                            break;
                        default:
                            Debug.Assert(false);
                            break;
                    }

                    m_Frame.Push(RuntimeType.Int64, value);
                    break;
                }
                case StackItemType.NativeInt: {
                    IntPtr value = m_Frame.Pop<IntPtr>();

                    switch (opCode) {
                        case ILOpCode.Shl:
                            value = (IntPtr)((long)value << shiftAmount);
                            break;
                        case ILOpCode.Shr:
                            value = (IntPtr)((long)value >> shiftAmount);
                            break;
                        case ILOpCode.Shr_un:
                            UIntPtr uintPtr = (UIntPtr)value.ToPointer();
                            value = (IntPtr)(long)((ulong)uintPtr >> shiftAmount);
                            break;
                        default:
                            Debug.Assert(false);
                            break;
                    }

                    m_Frame.Push(RuntimeType.NativeInt, value);
                    break;
                }
                case StackItemType.None:
                case StackItemType.ByReference:
                case StackItemType.Double:
                case StackItemType.ObjectReference:
                case StackItemType.ValueType:
                default:
                    throw new InvalidProgramException();
            }
        }

        /// <summary>
        /// Performans a unary operation on a value and pushes the result.
        /// 
        /// Stack: ..., value -> ..., value
        /// </summary>
        /// <param name="opCode">The operation code containing the unary operation</param>
        private void InterpretUnary(ILOpCode opCode) {
            StackItemType type = m_Frame.PeekItemType(0);
            switch (type) {
                case StackItemType.Int32: {
                    int value = m_Frame.Pop<int>();

                    switch (opCode) {
                        case ILOpCode.Neg:
                            m_Frame.Push(RuntimeType.Int32, -value);
                            break;
                        case ILOpCode.Not:
                            m_Frame.Push(RuntimeType.Int32, ~value);
                            break;
                        default:
                            Debug.Assert(false);
                            break;
                    }

                    break;
                }
                case StackItemType.Int64: {
                    long value = m_Frame.Pop<long>();

                    switch (opCode) {
                        case ILOpCode.Neg:
                            m_Frame.Push(RuntimeType.Int64, -value);
                            break;
                        case ILOpCode.Not:
                            m_Frame.Push(RuntimeType.Int64, ~value);
                            break;
                        default:
                            Debug.Assert(false);
                            break;
                    }

                    break;
                }
                case StackItemType.NativeInt: {
                    IntPtr value = m_Frame.Pop<IntPtr>();

                    switch (opCode) {
                        case ILOpCode.Neg:
                            m_Frame.Push(RuntimeType.NativeInt, (IntPtr)(-(long)value));
                            break;
                        case ILOpCode.Not:
                            m_Frame.Push(RuntimeType.NativeInt, (IntPtr)(~(long)value));
                            break;
                        default:
                            Debug.Assert(false);
                            break;
                    }

                    break;
                }
                case StackItemType.Double: {
                    double value = m_Frame.Pop<double>();

                    switch (opCode) {
                        case ILOpCode.Neg:
                            m_Frame.Push(RuntimeType.Double, -value);
                            break;
                        case ILOpCode.Not:
                            throw new InvalidProgramException();
                        default:
                            Debug.Assert(false);
                            break;
                    }

                    break;
                }
                case StackItemType.None:
                case StackItemType.ByReference:
                case StackItemType.ObjectReference:
                case StackItemType.ValueType:
                default:
                    throw new InvalidProgramException();
            }
        }
    }
}
