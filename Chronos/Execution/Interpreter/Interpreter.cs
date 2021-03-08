using Chronos.Exceptions;
using Chronos.Memory;
using Chronos.Metadata;
using Chronos.Tracing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;

namespace Chronos.Execution {
    /// <summary>
    /// Bytecode interpreter capable of interpreting a method.
    /// </summary>
    public partial class Interpreter : IDisposable {
        /// <summary>
        /// Holds information about a leave.
        /// </summary>
        private readonly struct LeaveInfo {
            /// <summary>
            /// The offset of the leave.
            /// </summary>
            public int Offset { get; }
            /// <summary>
            /// The offset of the target of the leave.
            /// </summary>
            public int Target { get; }
            /// <summary>
            /// The next index of the exception regions to check for a next leave.
            /// </summary>
            public int NextIndex { get; }

            /// <summary>
            /// Constructs a new leave info.
            /// </summary>
            /// <param name="offset">The offset of the leave</param>
            /// <param name="target">The offset of the target of the leave</param>
            /// <param name="nextIndex">The next index of the exception regions to check for a next leave</param>
            public LeaveInfo(int offset, int target, int nextIndex = 0) {
                Offset = offset;
                Target = target;
                NextIndex = nextIndex;
            }
        }

        /// <summary>
        /// The stack frame of the interpreters.
        /// </summary>
        public IStackFrame StackFrame { get; }

        /// <summary>
        /// Holds a reference to the tracer used to trace the execution.
        /// </summary>
        private readonly ITracer m_Tracer;
        /// <summary>
        /// Holds a reference to the method being interpreted.
        /// </summary>
        private readonly MethodDescription m_Method;
        /// <summary>
        /// Holds a reference to the stack allocator.
        /// </summary>
        private readonly StackAllocator m_StackAllocator;
        /// <summary>
        /// Holds a reference to the token resolver.
        /// </summary>
        private readonly ITokenResolver m_TokenResolver;
        /// <summary>
        /// Contains the stack frame.
        /// </summary>
        private readonly InterpreterStackFrame m_Frame;

        /// <summary>
        /// The type used in a constrained virtual call.
        /// </summary>
        private TypeDescription m_ConstrainedType;
        /// <summary>
        /// Flag that indicates the next virtual call is constrained.
        /// </summary>
        private bool m_ConstrainedFlag;

        /// <summary>
        /// The leave info stack that is needed for multiple nested try-blocks.
        /// </summary>
        private readonly List<LeaveInfo> m_LeaveInfoStack;
        /// <summary>
        /// The offset into the leave info stack.
        /// </summary>
        private int m_LeaveInfoStackOffset;
        /// <summary>
        /// The currently dispatched exception.
        /// </summary>
        private Exception m_DispatchedException;

        /// <summary>
        /// Constructs a new interpreter with given arguments.
        /// </summary>
        /// <param name="tracer">The tracer used to trace the execution</param>
        /// <param name="stackAllocator">The stack allocator</param>
        /// <param name="method">The method to interpret</param>
        /// <param name="callData">The reference to the call data</param>
        public Interpreter(ITracer tracer, StackAllocator stackAllocator, MethodDescription method, ref MethodCallData callData) {
            m_Tracer = tracer;
            m_Method = method;
            m_StackAllocator = stackAllocator;
            m_TokenResolver = m_Method.OwningType.OwningModule;
            
            m_Frame = new InterpreterStackFrame(m_StackAllocator, m_Method, ref callData);
            StackFrame = m_Frame;

            m_ConstrainedType = null;
            m_ConstrainedFlag = false;

            m_LeaveInfoStack = new List<LeaveInfo>();
            m_LeaveInfoStackOffset = 0;
        }

        /// <summary>
        /// Starts to interpret the method.
        /// </summary>
        /// <param name="callData">The reference to the method call data</param>
        public void InterpretMethod(ref MethodCallData callData) {
            CILReader reader = new CILReader(m_Method.Body.ILBytes);

            evaluation_loop:;
            try {
                while (reader.HasNext) {
                    ILOpCode opCode = reader.ReadOpCode();

#if TRACE_EXECUTION
                    m_Tracer.Trace(opCode);
#endif

                    switch (opCode) {
                        // Nothing
                        case ILOpCode.Nop:
                        case ILOpCode.Break:
                            break;

                        // Stack - Arguments
                        case ILOpCode.Ldarg_0:
                        case ILOpCode.Ldarg_1:
                        case ILOpCode.Ldarg_2:
                        case ILOpCode.Ldarg_3:
                            InterpretLoadArgument(opCode - ILOpCode.Ldarg_0, ref callData);
                            break;
                        case ILOpCode.Ldarg_s:
                            InterpretLoadArgument(reader.ReadByte(), ref callData);
                            break;
                        case ILOpCode.Ldarg:
                            InterpretLoadArgument(reader.ReadShort(), ref callData);
                            break;
                        case ILOpCode.Ldarga_s:
                            InterpretLoadArgumentAddress(reader.ReadByte(), ref callData);
                            break;
                        case ILOpCode.Ldarga:
                            InterpretLoadArgumentAddress(reader.ReadShort(), ref callData);
                            break;
                        case ILOpCode.Starg_s:
                            InterpretStoreArgument(reader.ReadByte(), ref callData);
                            break;
                        case ILOpCode.Starg:
                            InterpretStoreArgument(reader.ReadShort(), ref callData);
                            break;
                        // Stack - Locals
                        case ILOpCode.Ldloc_0:
                        case ILOpCode.Ldloc_1:
                        case ILOpCode.Ldloc_2:
                        case ILOpCode.Ldloc_3:
                            InterpretLoadLocal(opCode - ILOpCode.Ldloc_0);
                            break;
                        case ILOpCode.Ldloc_s:
                            InterpretLoadLocal(reader.ReadByte());
                            break;
                        case ILOpCode.Ldloc:
                            InterpretLoadLocal(reader.ReadShort());
                            break;
                        case ILOpCode.Ldloca_s:
                            InterpretLoadLocalAddress(reader.ReadByte());
                            break;
                        case ILOpCode.Ldloca:
                            InterpretLoadLocalAddress(reader.ReadShort());
                            break;
                        case ILOpCode.Stloc_0:
                        case ILOpCode.Stloc_1:
                        case ILOpCode.Stloc_2:
                        case ILOpCode.Stloc_3:
                            InterpretStoreLocal(opCode - ILOpCode.Stloc_0);
                            break;
                        case ILOpCode.Stloc_s:
                            InterpretStoreLocal(reader.ReadByte());
                            break;
                        case ILOpCode.Stloc:
                            InterpretStoreLocal(reader.ReadShort());
                            break;
                        // Stack - Generic
                        case ILOpCode.Dup:
                            InterpretDuplicate();
                            break;
                        case ILOpCode.Pop:
                            InterpretPop();
                            break;
                        case ILOpCode.Ldftn:
                            InterpretLoadFunction(reader.ReadToken());
                            break;
                        case ILOpCode.Ldvirtftn:
                            InterpretLoadVirtualFunction(reader.ReadToken());
                            break;
                        case ILOpCode.Localloc:
                            throw new NotImplementedException();
                        // Stack - Constants
                        case ILOpCode.Ldnull:
                            InterpretLoadNull();
                            break;
                        case ILOpCode.Ldc_i4_m1:
                            InterpretLoadConstant(-1);
                            break;
                        case ILOpCode.Ldc_i4_0:
                        case ILOpCode.Ldc_i4_1:
                        case ILOpCode.Ldc_i4_2:
                        case ILOpCode.Ldc_i4_3:
                        case ILOpCode.Ldc_i4_4:
                        case ILOpCode.Ldc_i4_5:
                        case ILOpCode.Ldc_i4_6:
                        case ILOpCode.Ldc_i4_7:
                        case ILOpCode.Ldc_i4_8:
                            InterpretLoadConstant(opCode - ILOpCode.Ldc_i4_0);
                            break;
                        case ILOpCode.Ldc_i4_s:
                            InterpretLoadConstant((sbyte)reader.ReadByte());
                            break;
                        case ILOpCode.Ldc_i4:
                            InterpretLoadConstant(reader.ReadInt());
                            break;
                        case ILOpCode.Ldc_i8:
                            InterpretLoadConstant(reader.ReadLong());
                            break;
                        case ILOpCode.Ldc_r4:
                            InterpretLoadConstant(reader.ReadFloat());
                            break;
                        case ILOpCode.Ldc_r8:
                            InterpretLoadConstant(reader.ReadDouble());
                            break;
                        // Stack - Pointer
                        case ILOpCode.Ldind_i1:
                            InterpretLoadIndirect<sbyte, int>(RuntimeType.Int32);
                            break;
                        case ILOpCode.Ldind_u1:
                            InterpretLoadIndirect<byte, int>(RuntimeType.Int32);
                            break;
                        case ILOpCode.Ldind_i2:
                            InterpretLoadIndirect<short, int>(RuntimeType.Int32);
                            break;
                        case ILOpCode.Ldind_u2:
                            InterpretLoadIndirect<ushort, int>(RuntimeType.Int32);
                            break;
                        case ILOpCode.Ldind_i4:
                            InterpretLoadIndirect<int, int>(RuntimeType.Int32);
                            break;
                        case ILOpCode.Ldind_u4:
                            InterpretLoadIndirect<uint, uint>(RuntimeType.Int32);
                            break;
                        case ILOpCode.Ldind_i8:
                            InterpretLoadIndirect<long, long>(RuntimeType.Int64);
                            break;
                        case ILOpCode.Ldind_i:
                            InterpretLoadIndirect<IntPtr, IntPtr>(RuntimeType.NativeInt);
                            break;
                        case ILOpCode.Ldind_r4:
                            InterpretLoadIndirectFloat();
                            break;
                        case ILOpCode.Ldind_r8:
                            InterpretLoadIndirect<double, double>(RuntimeType.Double);
                            break;
                        case ILOpCode.Ldind_ref:
                            InterpretLoadIndirectRef();
                            break;
                        case ILOpCode.Stind_i1:
                            InterpretStoreIndirect<int, sbyte>();
                            break;
                        case ILOpCode.Stind_i2:
                            InterpretStoreIndirect<int, short>();
                            break;
                        case ILOpCode.Stind_i4:
                            InterpretStoreIndirect<int, int>();
                            break;
                        case ILOpCode.Stind_i8:
                            InterpretStoreIndirect<long, long>();
                            break;
                        case ILOpCode.Stind_r4:
                            InterpretStoreIndirectFloat();
                            break;
                        case ILOpCode.Stind_r8:
                            InterpretStoreIndirect<double, double>();
                            break;
                        case ILOpCode.Stind_i:
                            InterpretStoreIndirect<IntPtr, IntPtr>();
                            break;
                        case ILOpCode.Stind_ref:
                            InterpretStoreIndirect<IntPtr, IntPtr>();
                            break;

                        // Control flow - Simple
                        case ILOpCode.Br_s:
                        case ILOpCode.Brfalse_s:
                        case ILOpCode.Brtrue_s:
                        case ILOpCode.Beq_s:
                        case ILOpCode.Bge_s:
                        case ILOpCode.Bgt_s:
                        case ILOpCode.Ble_s:
                        case ILOpCode.Blt_s:
                        case ILOpCode.Bne_un_s:
                        case ILOpCode.Bge_un_s:
                        case ILOpCode.Bgt_un_s:
                        case ILOpCode.Ble_un_s:
                        case ILOpCode.Blt_un_s: {
                                int delta = (sbyte)reader.ReadByte();
                                InterpretBranch(ref reader, opCode, reader.Offset + delta);
                                break;
                            }
                        case ILOpCode.Br:
                        case ILOpCode.Brfalse:
                        case ILOpCode.Brtrue:
                        case ILOpCode.Beq:
                        case ILOpCode.Bge:
                        case ILOpCode.Bgt:
                        case ILOpCode.Ble:
                        case ILOpCode.Blt:
                        case ILOpCode.Bne_un:
                        case ILOpCode.Bge_un:
                        case ILOpCode.Bgt_un:
                        case ILOpCode.Ble_un:
                        case ILOpCode.Blt_un: {
                                int delta = reader.ReadInt();
                                InterpretBranch(ref reader, opCode, reader.Offset + delta);
                                break;
                            }
                        case ILOpCode.Switch:
                            InterpretSwitch(ref reader);
                            break;
                        // Control flow - Methods
                        case ILOpCode.Call:
                            InterpretCall(reader.ReadToken(), false);
                            break;
                        case ILOpCode.Callvirt:
                            InterpretCall(reader.ReadToken(), true);
                            break;
                        case ILOpCode.Ret:
                            InterpretReturn(ref callData);
                            // A return should return (not break and continue like nothing happened...)
#if TRACE_EXECUTION
                            if (opCode != ILOpCode.Call && opCode != ILOpCode.Callvirt && opCode != ILOpCode.Newobj) {
                                m_Tracer.TraceLine();
                            }
                            m_Frame.TraceStack(m_Tracer);
#endif
                            return;
                        case ILOpCode.Jmp:
                        case ILOpCode.Calli:
                            throw new NotImplementedException();
                        // Control flow - Compare
                        case ILOpCode.Ceq:
                        case ILOpCode.Cgt:
                        case ILOpCode.Cgt_un:
                        case ILOpCode.Clt:
                        case ILOpCode.Clt_un:
                            InterpretCompare(opCode);
                            break;

                        // Arithmetic
                        case ILOpCode.Add:
                        case ILOpCode.Add_ovf:
                        case ILOpCode.Add_ovf_un:
                        case ILOpCode.Sub:
                        case ILOpCode.Sub_ovf:
                        case ILOpCode.Sub_ovf_un:
                        case ILOpCode.Mul:
                        case ILOpCode.Mul_ovf:
                        case ILOpCode.Mul_ovf_un:
                        case ILOpCode.Div:
                        case ILOpCode.Div_un:
                        case ILOpCode.Rem:
                        case ILOpCode.Rem_un:
                        case ILOpCode.And:
                        case ILOpCode.Or:
                        case ILOpCode.Xor:
                            InterpretBinary(opCode);
                            break;
                        case ILOpCode.Shl:
                        case ILOpCode.Shr:
                        case ILOpCode.Shr_un:
                            InterpretShift(opCode);
                            break;
                        case ILOpCode.Neg:
                        case ILOpCode.Not:
                            InterpretUnary(opCode);
                            break;

                        // Conversion - Normal
                        case ILOpCode.Conv_i1:
                            InterpretConvertToSByte(false, false);
                            break;
                        case ILOpCode.Conv_i2:
                            InterpretConvertToInt16(false, false);
                            break;
                        case ILOpCode.Conv_i4:
                            InterpretConvertToInt32(false, false);
                            break;
                        case ILOpCode.Conv_i8:
                            InterpretConvertToInt64(false, false);
                            break;
                        case ILOpCode.Conv_r4:
                            InterpretConvertToDouble(false, false);
                            break;
                        case ILOpCode.Conv_r8:
                            InterpretConvertToDouble(false, false);
                            break;
                        case ILOpCode.Conv_i:
                            InterpretConvertToIntPtr(false);
                            break;
                        case ILOpCode.Conv_u1:
                            InterpretConvertToByte(false, false);
                            break;
                        case ILOpCode.Conv_u2:
                            InterpretConvertToUInt16(false, false);
                            break;
                        case ILOpCode.Conv_u4:
                            InterpretConvertToUInt32(false, false);
                            break;
                        case ILOpCode.Conv_u8:
                            InterpretConvertToUInt64(false, false);
                            break;
                        case ILOpCode.Conv_u:
                            InterpretConvertToUIntPtr(false);
                            break;
                        case ILOpCode.Conv_r_un:
                            InterpretConvertToDouble(false, true);
                            break;
                        // Conversion - Overflow
                        case ILOpCode.Conv_ovf_i1:
                            InterpretConvertToSByte(true, false);
                            break;
                        case ILOpCode.Conv_ovf_i2:
                            InterpretConvertToInt16(true, false);
                            break;
                        case ILOpCode.Conv_ovf_i4:
                            InterpretConvertToInt32(true, false);
                            break;
                        case ILOpCode.Conv_ovf_i8:
                            InterpretConvertToInt64(true, false);
                            break;
                        case ILOpCode.Conv_ovf_i:
                            InterpretConvertToIntPtr(false);
                            break;
                        case ILOpCode.Conv_ovf_u1:
                            InterpretConvertToByte(true, false);
                            break;
                        case ILOpCode.Conv_ovf_u2:
                            InterpretConvertToUInt16(true, false);
                            break;
                        case ILOpCode.Conv_ovf_u4:
                            InterpretConvertToUInt32(true, false);
                            break;
                        case ILOpCode.Conv_ovf_u8:
                            InterpretConvertToUInt64(true, false);
                            break;
                        case ILOpCode.Conv_ovf_u:
                            InterpretConvertToUIntPtr(false);
                            break;
                        // Conversion - Overflow and Unsigned
                        case ILOpCode.Conv_ovf_i1_un:
                            InterpretConvertToSByte(true, true);
                            break;
                        case ILOpCode.Conv_ovf_i2_un:
                            InterpretConvertToInt16(true, true);
                            break;
                        case ILOpCode.Conv_ovf_i4_un:
                            InterpretConvertToInt32(true, true);
                            break;
                        case ILOpCode.Conv_ovf_i8_un:
                            InterpretConvertToInt64(true, true);
                            break;
                        case ILOpCode.Conv_ovf_i_un:
                            InterpretConvertToIntPtr(true);
                            break;
                        case ILOpCode.Conv_ovf_u1_un:
                            InterpretConvertToByte(true, true);
                            break;
                        case ILOpCode.Conv_ovf_u2_un:
                            InterpretConvertToUInt16(true, true);
                            break;
                        case ILOpCode.Conv_ovf_u4_un:
                            InterpretConvertToUInt32(true, true);
                            break;
                        case ILOpCode.Conv_ovf_u8_un:
                            InterpretConvertToUInt64(true, true);
                            break;
                        case ILOpCode.Conv_ovf_u_un:
                            InterpretConvertToUIntPtr(true);
                            break;

                        // Object model - Generic
                        case ILOpCode.Newobj:
                            InterpretNewObject(reader.ReadToken());
                            break;
                        case ILOpCode.Castclass:
                            InterpretCastClass(reader.ReadToken());
                            break;
                        case ILOpCode.Isinst:
                            InterpretIsInstance(reader.ReadToken());
                            break;
                        case ILOpCode.Ldstr:
                            InterpretLoadString(reader.ReadStringToken());
                            break;
                        case ILOpCode.Initobj:
                            InterpretInitObject(reader.ReadToken());
                            break;
                        case ILOpCode.Ldobj:
                            InterpretLoadObject(reader.ReadToken());
                            break;
                        case ILOpCode.Cpobj:
                            InterpretCopyObject(reader.ReadToken());
                            break;
                        case ILOpCode.Stobj:
                            InterpretStoreObject(reader.ReadToken());
                            break;
                        // Object model - Access
                        case ILOpCode.Ldfld:
                            InterpretLoadField(reader.ReadToken());
                            break;
                        case ILOpCode.Ldflda:
                            InterpretLoadFieldAddress(reader.ReadToken());
                            break;
                        case ILOpCode.Stfld:
                            InterpretStoreField(reader.ReadToken());
                            break;
                        case ILOpCode.Ldsfld:
                            InterpretLoadStaticField(reader.ReadToken());
                            break;
                        case ILOpCode.Ldsflda:
                            InterpretLoadStaticFieldAddress(reader.ReadToken());
                            break;
                        case ILOpCode.Stsfld:
                            InterpretStoreStaticField(reader.ReadToken());
                            break;
                        // Object model - Boxing
                        case ILOpCode.Unbox:
                            InterpretUnbox(reader.ReadToken());
                            break;
                        case ILOpCode.Box:
                            InterpretBox(reader.ReadToken());
                            break;
                        case ILOpCode.Unbox_any:
                            InterpretUnboxAny(reader.ReadToken());
                            break;

                        // Array
                        case ILOpCode.Newarr:
                            InterpretNewArray(reader.ReadToken());
                            break;
                        case ILOpCode.Ldlen:
                            InterpretLoadArrayLength();
                            break;
                        case ILOpCode.Ldelema:
                            InterpretLoadArrayElementAddress(reader.ReadToken());
                            break;
                        case ILOpCode.Ldelem_i1:
                            InterpretLoadArrayElement<sbyte, int>(RuntimeType.Int32);
                            break;
                        case ILOpCode.Ldelem_u1:
                            InterpretLoadArrayElement<byte, int>(RuntimeType.Int32);
                            break;
                        case ILOpCode.Ldelem_i2:
                            InterpretLoadArrayElement<short, int>(RuntimeType.Int32);
                            break;
                        case ILOpCode.Ldelem_u2:
                            InterpretLoadArrayElement<ushort, int>(RuntimeType.Int32);
                            break;
                        case ILOpCode.Ldelem_i4:
                            InterpretLoadArrayElement<int, int>(RuntimeType.Int32);
                            break;
                        case ILOpCode.Ldelem_u4:
                            InterpretLoadArrayElement<uint, uint>(RuntimeType.Int32);
                            break;
                        case ILOpCode.Ldelem_i8:
                            InterpretLoadArrayElement<long, long>(RuntimeType.Int64);
                            break;
                        case ILOpCode.Ldelem_i:
                            InterpretLoadArrayElement<IntPtr, IntPtr>(RuntimeType.NativeInt);
                            break;
                        case ILOpCode.Ldelem_r4:
                            InterpretLoadArrayElementFloat();
                            break;
                        case ILOpCode.Ldelem_r8:
                            InterpretLoadArrayElement<double, double>(RuntimeType.Double);
                            break;
                        case ILOpCode.Ldelem_ref:
                            InterpretLoadArrayElementReference();
                            break;
                        case ILOpCode.Ldelem:
                            InterpretLoadArrayElement(reader.ReadToken());
                            break;
                        case ILOpCode.Stelem_i1:
                            InterpretStoreArrayElement<int, sbyte>();
                            break;
                        case ILOpCode.Stelem_i2:
                            InterpretStoreArrayElement<int, short>();
                            break;
                        case ILOpCode.Stelem_i4:
                            InterpretStoreArrayElement<int, int>();
                            break;
                        case ILOpCode.Stelem_i8:
                            InterpretStoreArrayElement<long, long>();
                            break;
                        case ILOpCode.Stelem_i:
                            InterpretStoreArrayElement<IntPtr, IntPtr>();
                            break;
                        case ILOpCode.Stelem_r4:
                            InterpretStoreArrayElementFloat();
                            break;
                        case ILOpCode.Stelem_r8:
                            InterpretStoreArrayElement<double, double>();
                            break;
                        case ILOpCode.Stelem_ref:
                            InterpretStoreArrayElementReference();
                            break;
                        case ILOpCode.Stelem:
                            InterpretStoreArrayElement(reader.ReadToken());
                            break;

                        // Misc - Type references
                        case ILOpCode.Refanyval:
                        case ILOpCode.Mkrefany:
                        case ILOpCode.Refanytype:
                            throw new NotImplementedException();
                        // Misc - Memory
                        case ILOpCode.Cpblk:
                            InterpretCopyBlock();
                            break;
                        case ILOpCode.Initblk:
                            InterpretInitBlock();
                            break;
                        // Misc - General
                        case ILOpCode.Ckfinite:
                            InterpretCheckFinite();
                            break;
                        case ILOpCode.Sizeof:
                            InterpretSizeOf(reader.ReadToken());
                            break;
                        case ILOpCode.Ldtoken:
                            InterpretLoadToken(reader.ReadToken());
                            break;
                        case ILOpCode.Arglist:
                            throw new NotImplementedException();

                        // Exceptions
                        case ILOpCode.Throw:
                            InterpretThrow();
                            break;
                        case ILOpCode.Endfinally:
                            InterpretEndFinally(ref reader);
                            break;
                        case ILOpCode.Leave:
                            InterpretLeave(reader.ReadInt(), ref reader);
                            break;
                        case ILOpCode.Leave_s:  
                            InterpretLeave(reader.ReadByte(), ref reader);
                            break;
                        case ILOpCode.Rethrow:
                            InterpretRethrow();
                            break;
                        case ILOpCode.Endfilter:
                            throw new NotImplementedException();

                        // Prefixes
                        case ILOpCode.Unaligned:
                            // We do not need to do anything except read past the alignment operand.
                            reader.ReadByte();
                            break;
                        case ILOpCode.Volatile:
                            // We also do not need to do anything here.
                            break;
                        case ILOpCode.Readonly:
                            // The readonly prefix would usually have an effect on the 'ldelema' instruction
                            // by telling it to not perform any type check.
                            // However we do not make this type check regardless, so we can just ignore this prefix.
                            break;
                        case ILOpCode.Constrained:
                            InterpretConstraint(reader.ReadToken());
                            break;
                        case ILOpCode.Tail:
                            throw new NotImplementedException();

                        default:
                            Debug.Assert(false);
                            break;
                    }

#if TRACE_EXECUTION
                    if (opCode != ILOpCode.Call && opCode != ILOpCode.Callvirt && opCode != ILOpCode.Newobj) {
                        m_Tracer.TraceLine();
                    }
                    m_Frame.TraceStack(m_Tracer);
#endif
                }
            } catch (ManagedException managedException) {
                if (MethodHandlesException(managedException, ref reader)) {
                    goto evaluation_loop;
                } else {
                    throw;
                }
            }
        }

        /// <summary>
        /// <inheritdoc cref="IDisposable.Dispose"/>
        /// </summary>
        public void Dispose() {
            m_Frame.Dispose();
        }

        /// <summary>
        /// Gets the actual type of a type.
        /// </summary>
        /// <param name="type">The type</param>
        /// <returns>The actual type of the type</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static TypeDescription GetActualType(TypeDescription type) {
            return type.IsEnum ? type.GetUnderlyingType() : type;
        }

        /// <summary>
        /// Throw an exception if a given pointer is invalid.
        /// </summary>
        /// <param name="pointer">The pointer to check</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void ThrowOnInvalidPointer(void* pointer) {
            if (pointer == null) {
                VirtualMachine.ExceptionEngine.ThrowNullReferenceException();
            }
        }

        /// <summary>
        /// Throw an exception if a given pointer is invalid.
        /// </summary>
        /// <param name="pointer">The pointer to check</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ThrowOnInvalidPointer(IntPtr pointer) {
            if (pointer == IntPtr.Zero) {
                VirtualMachine.ExceptionEngine.ThrowNullReferenceException();
            }
        }
    }
}
