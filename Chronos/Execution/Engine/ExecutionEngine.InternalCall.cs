using Chronos.Lib;
using Chronos.Metadata;
using System;
using System.Diagnostics;

namespace Chronos.Execution {
    public partial class ExecutionEngine {
        /// <summary>
        /// Makes a method call to an internally implemented method.
        /// </summary>
        /// <param name="method">The method to call</param>
        /// <param name="callData">The method call data</param>
        private void MakeInternalCall(MethodDescription method, ref MethodCallData callData) {
            // Get the method pointer for the internal function.
            Delegate internalMethod = Internals.Get(method);

            // Setup the arguments into a general object array.
            int argumentCount = callData.ArgumentCount;
            object[] arguments = new object[argumentCount];
            for (int i = 0; i < argumentCount; i++) {
                RuntimeType argumentRuntimeType = callData.GetArgumentType(i);
                TypeDescription argumentType = argumentRuntimeType.Type;

                if (argumentRuntimeType.ItemType == StackItemType.ByReference || argumentRuntimeType.ItemType == StackItemType.ObjectReference) {
                    arguments[i] = callData.GetArgument<IntPtr>(i);
                } else if (argumentType.IsSpecialSystemType) {
                    switch (argumentType.SpecialSystemType) {
                        case SpecialSystemType.Boolean:
                            arguments[i] = callData.GetArgument<bool>(i);
                            break;
                        case SpecialSystemType.Char:
                            arguments[i] = callData.GetArgument<char>(i);
                            break;
                        case SpecialSystemType.SByte:
                            arguments[i] = callData.GetArgument<sbyte>(i);
                            break;
                        case SpecialSystemType.Byte:
                            arguments[i] = callData.GetArgument<byte>(i);
                            break;
                        case SpecialSystemType.Int16:
                            arguments[i] = callData.GetArgument<short>(i);
                            break;
                        case SpecialSystemType.UInt16:
                            arguments[i] = callData.GetArgument<ushort>(i);
                            break;
                        case SpecialSystemType.Int32:
                            arguments[i] = callData.GetArgument<int>(i);
                            break;
                        case SpecialSystemType.UInt32:
                            arguments[i] = callData.GetArgument<uint>(i);
                            break;
                        case SpecialSystemType.Int64:
                            arguments[i] = callData.GetArgument<long>(i);
                            break;
                        case SpecialSystemType.UInt64:
                            arguments[i] = callData.GetArgument<ulong>(i);
                            break;
                        case SpecialSystemType.Single:
                            arguments[i] = callData.GetArgument<float>(i);
                            break;
                        case SpecialSystemType.Double:
                            arguments[i] = callData.GetArgument<double>(i);
                            break;
                        case SpecialSystemType.IntPtr:
                            arguments[i] = callData.GetArgument<IntPtr>(i);
                            break;
                        case SpecialSystemType.UIntPtr:
                            arguments[i] = callData.GetArgument<UIntPtr>(i);
                            break;
                        case SpecialSystemType.Object:
                        case SpecialSystemType.String:
                        case SpecialSystemType.Array:
                        case SpecialSystemType.MulticastDelegate:
                        case SpecialSystemType.RuntimeTypeHandle:
                        case SpecialSystemType.Exception:
                            arguments[i] = callData.GetArgument<IntPtr>(i);
                            break;
                        default:
                            Debug.Assert(false);
                            break;
                    }
                } else if (argumentType.IsStruct) {
                    if (argumentType.IsLargeStruct) {
                        // We do not support large structs as an argument type for internal methods.
                        Debug.Assert(false);
                    } else {
                        arguments[i] = callData.GetArgument<long>(i);
                    }
                } else {
                    // Everything else is a pointer.
                    arguments[i] = callData.GetArgument<IntPtr>(i);
                }
            }

            // Invoke the internal method.
            object result = internalMethod.DynamicInvoke(arguments);

            // Setup the return type if needed.
            RuntimeType returnRuntimeType = callData.GetReturnType();
            TypeDescription returnType = returnRuntimeType.Type;

            if (!returnType.IsVoid) {
                if (returnType.IsSpecialSystemType) {
                    switch (returnType.SpecialSystemType) {
                        case SpecialSystemType.Boolean:
                            callData.SetReturnValue(returnRuntimeType, (bool)result);
                            break;
                        case SpecialSystemType.Char:
                            callData.SetReturnValue(returnRuntimeType, (char)result);
                            break;
                        case SpecialSystemType.SByte:
                        case SpecialSystemType.Byte:
                            callData.SetReturnValue(returnRuntimeType, (sbyte)result);
                            break;
                        case SpecialSystemType.Int16:
                        case SpecialSystemType.UInt16:
                            callData.SetReturnValue(returnRuntimeType, (short)result);
                            break;
                        case SpecialSystemType.Int32:
                        case SpecialSystemType.UInt32:
                            callData.SetReturnValue(returnRuntimeType, (int)result);
                            break;
                        case SpecialSystemType.Int64:
                        case SpecialSystemType.UInt64:
                            callData.SetReturnValue(returnRuntimeType, (long)result);
                            break;
                        case SpecialSystemType.Single:
                            callData.SetReturnValue(returnRuntimeType, (float)result);
                            break;
                        case SpecialSystemType.Double:
                            callData.SetReturnValue(returnRuntimeType, (double)result);
                            break;
                        case SpecialSystemType.UIntPtr:
                        case SpecialSystemType.IntPtr:
                        case SpecialSystemType.Object:
                        case SpecialSystemType.String:
                        case SpecialSystemType.Array:
                        case SpecialSystemType.MulticastDelegate:
                        case SpecialSystemType.RuntimeTypeHandle:
                        case SpecialSystemType.Exception:
                            callData.SetReturnValue(returnRuntimeType, (IntPtr)result);
                            break;
                        default:
                            Debug.Assert(false);
                            break;
                    }
                } else if (returnType.IsStruct) {
                    if (returnType.IsLargeStruct) {
                        // We do not support large structs as a return type for internal methods.
                        Debug.Assert(false);
                    } else {
                        callData.SetReturnValue(returnRuntimeType, (long)result);
                    }
                } else {
                    // Everything else is a pointer.
                    callData.SetReturnValue(returnRuntimeType, (IntPtr)result);
                }
            }
        }
    }
}
