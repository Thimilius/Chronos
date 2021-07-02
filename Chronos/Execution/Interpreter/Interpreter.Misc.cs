using Chronos.Metadata;
using Chronos.Model;
using System;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Chronos.Execution {
    public partial class Interpreter {
        /// <summary>
        /// Copies a memory block with a certain size from a destination address to a source address.
        /// 
        /// Stack: ..., destination address, source address, size -> ...
        /// </summary>
        private unsafe void InterpretCopyBlock() {
            uint size = m_Frame.Pop<uint>();
            IntPtr sourcePointer = m_Frame.Pop<IntPtr>();
            IntPtr destinationPointer = m_Frame.Pop<IntPtr>();

            Unsafe.CopyBlock((void*)destinationPointer, (void*)sourcePointer, size);
        }

        /// <summary>
        /// Initializes a block of memory with a certain size at an address with a value.
        /// 
        /// Stack: ..., address, value, size -> ...
        /// </summary>
        private unsafe void InterpretInitBlock() {
            uint size = m_Frame.Pop<uint>();
            byte value = m_Frame.Pop<byte>();
            IntPtr address = m_Frame.Pop<IntPtr>();

            Unsafe.InitBlock((void*)address, value, size);
        }

        /// <summary>
        /// Checks if a floating point value is a finite number.
        /// 
        /// Stack: ..., value -> ..., value
        /// </summary>
        private void InterpretCheckFinite() {
            // We only peek at the value as we do not want to change the stack.
            double value = m_Frame.Peek<double>(0);

            if (!double.IsFinite(value)) {
                VirtualMachine.ExceptionEngine.ThrowNotFiniteNumberException();
            }
        }

        /// <summary>
        /// Pushes the size of a type as a 32-bit integer on the stack.
        /// 
        /// Stack: ... -> ..., size
        /// </summary>
        /// <param name="token">The token of the type</param>
        private void InterpretSizeOf(EntityHandle token) {
            Debug.Assert(token.Kind == HandleKind.TypeDefinition || token.Kind == HandleKind.TypeReference || token.Kind == HandleKind.TypeSpecification);

            TypeDescription type = GetActualType(m_TokenResolver.ResolveType(token));

            int size = 0;
            if (type.IsSpecialSystemType) {
                switch (type.SpecialSystemType) {
                    case SpecialSystemType.Void:
                        size = 0;
                        break;
                    case SpecialSystemType.Boolean:
                    case SpecialSystemType.SByte:
                    case SpecialSystemType.Byte:
                        size = 1;
                        break;
                    case SpecialSystemType.Char:
                    case SpecialSystemType.Int16:
                    case SpecialSystemType.UInt16:
                        size = 2;
                        break;
                    case SpecialSystemType.Int32:
                    case SpecialSystemType.UInt32:
                    case SpecialSystemType.Single:
                        size = 4;
                        break;
                    case SpecialSystemType.Int64:
                    case SpecialSystemType.UInt64:
                    case SpecialSystemType.IntPtr:
                    case SpecialSystemType.UIntPtr:
                    case SpecialSystemType.Double:
                    case SpecialSystemType.Object:
                    case SpecialSystemType.String:
                    case SpecialSystemType.ValueType:
                    case SpecialSystemType.Array:
                    case SpecialSystemType.MulticastDelegate:
                    case SpecialSystemType.RuntimeTypeHandle:
                    case SpecialSystemType.Exception:
                        size = 8;
                        break;
                }
            } else if (type.IsStruct) {
                // We only get actual size of the type for value types.
                size = type.Size;
            } else {
                // For everything else we do not get the actual size of the type.
                // Instead the size is the size of a reference or pointer to that type.
                size = IntPtr.Size;
            }

            m_Frame.Push(RuntimeType.Int32, size);
        }

        /// <summary>
        /// Loads the runtime representation of a metadata token.
        /// 
        /// Stack: ... -> ..., RuntimeHandle
        /// </summary>
        /// <param name="token">The token to load the metadata from</param>
        private void InterpretLoadToken(EntityHandle token) {
            // NOTE: For now we just support loading a type token.
            // The instruction is actually capable of loading a field or a method token as well.
            Debug.Assert(token.Kind == HandleKind.TypeDefinition || token.Kind == HandleKind.TypeReference || token.Kind == HandleKind.TypeSpecification);

            TypeDescription type = m_TokenResolver.ResolveType(token);

            m_Frame.Push(RuntimeType.FromType(MetadataSystem.RuntimeTypeHandleType), GCHandle.ToIntPtr(type.Handle));
        }

        /// <summary>
        /// Records that the subsequent 'callvirt' method call is constrained.
        /// 
        /// Stack: ... -> ...
        /// </summary>
        private void InterpretConstraint(EntityHandle token) {
            Debug.Assert(token.Kind == HandleKind.TypeDefinition || token.Kind == HandleKind.TypeReference || token.Kind == HandleKind.TypeSpecification);

            TypeDescription type = GetActualType(m_TokenResolver.ResolveType(token));

            // We save the type token and record the constrained flag which will get reset by the 'callvirt' instruction.
            m_ConstrainedType = type;
            m_ConstrainedFlag = true;
        }
    }
}
