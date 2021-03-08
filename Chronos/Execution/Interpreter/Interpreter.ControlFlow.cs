using System;
using System.Diagnostics;
using System.Reflection.Metadata;

namespace Chronos.Execution {
    public partial class Interpreter {
        /// <summary>
        /// Performs general branch instruction an jumps to a given target.
        /// 
        /// Stack ... (, value, value), value -> ...
        /// </summary>
        /// <param name="reader">A reference to the bytecode reader</param>
        /// <param name="opCode">The operation code containing the branch instruction</param>
        /// <param name="target">The target to seek to</param>
        private void InterpretBranch(ref CILReader reader, ILOpCode opCode, int target) {
            switch (opCode) {
                case ILOpCode.Br_s:
                case ILOpCode.Br:
                    reader.Seek(target);
                    break;
                case ILOpCode.Brfalse_s:
                case ILOpCode.Brfalse:
                case ILOpCode.Brtrue_s:
                case ILOpCode.Brtrue: {
                    StackItemType type = m_Frame.PeekItemType(0);

                    bool value = type switch
                    {
                        StackItemType.Int32 => m_Frame.Pop<int>() != 0,
                        StackItemType.Int64 => m_Frame.Pop<long>() != 0,
                        StackItemType.NativeInt => m_Frame.Pop<IntPtr>() != IntPtr.Zero,
                        StackItemType.ObjectReference => m_Frame.Pop<IntPtr>() != IntPtr.Zero,
                        _ => throw new InvalidProgramException(),
                    };

                    if (value) {
                        if (opCode == ILOpCode.Brtrue_s || opCode == ILOpCode.Brtrue) {
                            reader.Seek(target);
                        }
                    } else {
                        if (opCode == ILOpCode.Brfalse_s || opCode == ILOpCode.Brfalse) {
                            reader.Seek(target);
                        }
                    }

                    break;
                }
                case ILOpCode.Beq_s:
                case ILOpCode.Beq: {
                    InterpretCompare(ILOpCode.Ceq);
                    InterpretBranch(ref reader, ILOpCode.Brtrue, target);
                    break;
                }
                case ILOpCode.Bge_s:
                case ILOpCode.Bge: {
                    InterpretCompare(ILOpCode.Clt);
                    InterpretBranch(ref reader, ILOpCode.Brfalse, target);
                    break;
                }
                case ILOpCode.Bgt_s:
                case ILOpCode.Bgt: {
                    InterpretCompare(ILOpCode.Cgt);
                    InterpretBranch(ref reader, ILOpCode.Brtrue, target);
                    break;
                }
                case ILOpCode.Ble_s:
                case ILOpCode.Ble: {
                    InterpretCompare(ILOpCode.Cgt);
                    InterpretBranch(ref reader, ILOpCode.Brfalse, target);
                    break;
                }
                case ILOpCode.Blt_s:
                case ILOpCode.Blt: {
                    InterpretCompare(ILOpCode.Clt);
                    InterpretBranch(ref reader, ILOpCode.Brtrue, target);
                    break;
                }
                case ILOpCode.Bne_un_s:
                case ILOpCode.Bne_un: {
                    InterpretCompare(ILOpCode.Ceq);
                    InterpretBranch(ref reader, ILOpCode.Brfalse, target);
                    break;
                }
                case ILOpCode.Bge_un_s:
                case ILOpCode.Bge_un: {
                    InterpretCompare(ILOpCode.Clt_un);
                    InterpretBranch(ref reader, ILOpCode.Brfalse, target);
                    break;
                }
                case ILOpCode.Bgt_un_s:
                case ILOpCode.Bgt_un: {
                    InterpretCompare(ILOpCode.Cgt_un);
                    InterpretBranch(ref reader, ILOpCode.Brtrue, target);
                    break;
                }
                case ILOpCode.Ble_un_s:
                case ILOpCode.Ble_un: {
                    InterpretCompare(ILOpCode.Cgt_un);
                    InterpretBranch(ref reader, ILOpCode.Brfalse, target);
                    break;
                }
                case ILOpCode.Blt_un_s:
                case ILOpCode.Blt_un: {
                    InterpretCompare(ILOpCode.Clt_un);
                    InterpretBranch(ref reader, ILOpCode.Brtrue, target);
                    break;
                }
                default:
                    Debug.Assert(false);
                    break;
            }
        }

        /// <summary>
        /// Performs a switch branch based on a jump table.
        /// 
        /// Stack: ..., value -> ...
        /// </summary>
        /// <param name="reader">A reference to the bytecode reader</param>
        private void InterpretSwitch(ref CILReader reader) {
            // The size of the operands for the switch instruction are all 32-bit integers meaning 4 bytes wide.
            const int OPERAND_SIZE = 4;

            int value = m_Frame.Pop<int>();
            int count = reader.ReadInt();
            int jumpBase = reader.Offset + OPERAND_SIZE * count;

            if (value < count) {
                int branch = reader.Offset + value * OPERAND_SIZE;
                int delta = reader.ReadIntAt(branch);
                reader.Seek(jumpBase + delta);
            } else {
                // If we get here we got no match so we just to the next instruction.
                reader.Seek(jumpBase);
            }
        }

        /// <summary>
        /// Compares two values on the stack and pushes the result.
        /// 
        /// Stack: ..., value, value -> ..., value
        /// </summary>
        /// <param name="opCode">The operation code containing the compare instruction</param>
        private unsafe void InterpretCompare(ILOpCode opCode) {
            StackItemType firstType = m_Frame.PeekItemType(0);
            StackItemType secondType = m_Frame.PeekItemType(1);
            bool result = default;

            // This ensures we properly promote the types as necessary for the operation.
            StackItemType type = firstType > secondType ? firstType : secondType;

            switch (type) {
                case StackItemType.Int32: {
                    int value1 = m_Frame.Pop<int>();
                    int value2 = m_Frame.Pop<int>();

                    switch (opCode) {
                        case ILOpCode.Ceq:
                            result = value1 == value2;
                            break;
                        case ILOpCode.Cgt:
                            result = value2 > value1;
                            break;
                        case ILOpCode.Cgt_un:
                            result = (uint)value2 > (uint)value1;
                            break;
                        case ILOpCode.Clt:
                            result = value2 < value1;
                            break;
                        case ILOpCode.Clt_un:
                            result = (uint)value2 < (uint)value1;
                            break;
                        default:
                            Debug.Assert(false);
                            break;
                    }

                    break;
                }
                case StackItemType.Int64: {
                    long value1 = m_Frame.Pop<long>();
                    long value2 = m_Frame.Pop<long>();

                    switch (opCode) {
                        case ILOpCode.Ceq:
                            result = value1 == value2;
                            break;
                        case ILOpCode.Cgt:
                            result = value2 > value1;
                            break;
                        case ILOpCode.Cgt_un:
                            result = (ulong)value2 > (ulong)value1;
                            break;
                        case ILOpCode.Clt:
                            result = value2 < value1;
                            break;
                        case ILOpCode.Clt_un:
                            result = (ulong)value2 < (ulong)value1;
                            break;
                        default:
                            Debug.Assert(false);
                            break;
                    }

                    break;
                }
                case StackItemType.ByReference:
                case StackItemType.NativeInt: {
                    IntPtr value1 = m_Frame.Pop<IntPtr>();
                    IntPtr value2 = m_Frame.Pop<IntPtr>();

                    switch (opCode) {
                        case ILOpCode.Ceq:
                            result = value1 == value2;
                            break;
                        case ILOpCode.Cgt:
                            result = (long)value2 > (long)value1;
                            break;
                        case ILOpCode.Cgt_un:
                            result = (ulong)((UIntPtr)value2.ToPointer()) > (ulong)((UIntPtr)value1.ToPointer());
                                break;
                        case ILOpCode.Clt:
                            result = (long)value2 < (long)value1;
                            break;
                        case ILOpCode.Clt_un:
                            result = (ulong)((UIntPtr)value2.ToPointer()) < (ulong)((UIntPtr)value1.ToPointer());
                            break;
                        default:
                            Debug.Assert(false);
                            break;
                    }

                    break;
                }
                case StackItemType.Double: {
                    double value1 = m_Frame.Pop<double>();
                    double value2 = m_Frame.Pop<double>();

                    switch (opCode) {
                        case ILOpCode.Ceq:
                            result = value1 == value2;
                            break;
                        case ILOpCode.Cgt:
                        case ILOpCode.Cgt_un:
                            result = value2 > value1;
                            break;
                        case ILOpCode.Clt:
                        case ILOpCode.Clt_un:
                            result = value2 < value1;
                            break;
                        default:
                            Debug.Assert(false);
                            break;
                    }

                    break;
                }
                case StackItemType.ObjectReference: {
                    IntPtr value1 = m_Frame.Pop<IntPtr>();
                    IntPtr value2 = m_Frame.Pop<IntPtr>();

                    if (opCode == ILOpCode.Ceq) {
                        result = value1 == value2;
                    } else if (opCode == ILOpCode.Cgt_un) {
                        // The 'Cgt_un' works as a not equal instruction and is mostly used to compare with null.
                        result = value1 != value2;
                    } else {
                        throw new InvalidProgramException();
                    }
                    break;
                }
                case StackItemType.None:
                case StackItemType.ValueType:
                default:
                    throw new InvalidProgramException();
            }

            m_Frame.Push(RuntimeType.Int32, result ? 1 : 0);
        }
    }
}
