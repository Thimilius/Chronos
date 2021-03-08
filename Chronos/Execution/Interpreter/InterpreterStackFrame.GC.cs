using Chronos.Memory;
using Chronos.Metadata;
using System;
using System.Diagnostics;

namespace Chronos.Execution {
    public unsafe partial class InterpreterStackFrame : IStackFrame {
        /// <summary>
        /// <inheritdoc cref="IStackFrame.InspectStack(ObjectReferenceCallback, ByReferenceCallback)"/>
        /// </summary>
        public void InspectStack(ObjectReferenceCallback objectReferenceCallback, ByReferenceCallback byReferenceCallback) {
            Debug.Assert(objectReferenceCallback != null);
            Debug.Assert(byReferenceCallback != null);

            int largeStructStackByteOffset = 0;
            for (int i = 0; i < m_RuntimeTypesStackOffset; i++) {
                RuntimeType runtimeType = m_RuntimeTypesStack[i];
                StackItemType stackItemType = runtimeType.ItemType;
                int byteOffset = i * SLOT_SIZE;

                if (stackItemType == StackItemType.ObjectReference) {
                    objectReferenceCallback(PeekAt<IntPtr>(byteOffset));
                } else if (stackItemType == StackItemType.ByReference) {
                    byReferenceCallback(PeekAt<IntPtr>(byteOffset));
                } else if (stackItemType == StackItemType.ValueType) {
                    TypeDescription type = runtimeType.Type;
                    byte* memory;
                    if (type.IsLargeStruct) {
                        // Get the address of the large struct.
                        memory = &m_LargeStructStackMemory[largeStructStackByteOffset];

                        // We need to keep track of the byte offset for the large struct stack as we iterate through it.
                        largeStructStackByteOffset += type.Size;
                    } else {
                        // Get the address of the small struct.
                        memory = &m_StackMemory[byteOffset];
                    }

                    GCHelper.InspectStruct(objectReferenceCallback, memory, type);
                }
            }
        }

        /// <summary>
        /// <inheritdoc cref="IStackFrame.InspectLocals(ObjectReferenceCallback, ByReferenceCallback)"/>
        /// </summary>
        public void InspectLocals(ObjectReferenceCallback objectReferenceCallback, ByReferenceCallback byReferenceCallback) {
            Debug.Assert(objectReferenceCallback != null);
            Debug.Assert(byReferenceCallback != null);

            for (int i = 0; i < m_Locals.Count; i++) {
                RuntimeType localRuntimeType = m_Locals[i].RuntimeType;
                StackItemType localItemType = localRuntimeType.ItemType;
                int offset = m_Locals[i].Offset;

                if (localItemType == StackItemType.ObjectReference) {
                    objectReferenceCallback(*(IntPtr*)&m_LocalsMemory[offset]);
                } else if (localItemType == StackItemType.ByReference) {
                    byReferenceCallback(*(IntPtr*)&m_LocalsMemory[offset]);
                } else if (localItemType == StackItemType.ValueType) {
                    GCHelper.InspectStruct(objectReferenceCallback, &m_LocalsMemory[offset], localRuntimeType.Type);
                }
            }
        }

        /// <summary>
        /// <inheritdoc cref="IStackFrame.InspectArguments(ObjectReferenceCallback, ByReferenceCallback)"/>
        /// </summary>
        public void InspectArguments(ObjectReferenceCallback objectReferenceCallback, ByReferenceCallback byReferenceCallback) {
            Debug.Assert(objectReferenceCallback != null);
            Debug.Assert(byReferenceCallback != null);

            int argumentCount = m_MethodCallData.ArgumentCount;
            for (int i = 0; i < argumentCount; i++) {
                RuntimeType argumentRuntimeType = m_MethodCallData.GetArgumentType(i);
                StackItemType argumentStackItemType = argumentRuntimeType.ItemType;

                if (argumentStackItemType == StackItemType.ObjectReference) {
                    objectReferenceCallback(m_MethodCallData.GetArgument<IntPtr>(i));
                } else if (argumentStackItemType == StackItemType.ByReference) {
                    byReferenceCallback(m_MethodCallData.GetArgument<IntPtr>(i));
                } else if (argumentStackItemType == StackItemType.ValueType) {
                    GCHelper.InspectStruct(objectReferenceCallback, (byte*)m_MethodCallData.GetArgumentAddress(i), argumentRuntimeType.Type);
                }
            }
        }
    }
}
