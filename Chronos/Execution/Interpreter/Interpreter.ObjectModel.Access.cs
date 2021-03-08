using Chronos.Metadata;
using Chronos.Model;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;

namespace Chronos.Execution {
    public partial class Interpreter {
        /// <summary>
        /// Pushes the value of a field of an object on the stack.
        /// 
        /// The object on the stack is allowed to be of the following types:
        ///     - ObjectReference
        ///     - ByReference
        ///     - NativeInt
        ///     - ValueType
        /// 
        /// Stack: ..., obj -> ..., value
        /// </summary>
        /// <param name="token">The token of the field</param>
        private unsafe void InterpretLoadField(EntityHandle token) {
            Debug.Assert(token.Kind == HandleKind.FieldDefinition || token.Kind == HandleKind.MemberReference);

            FieldDescription field = m_TokenResolver.ResolveField(token);
            // It is possible for this normal field instruction to also refer to static fields.
            // In that case we just delegate the work as if a corresponding static field instruction has been made.
            if (field.Attributes.HasFlag(FieldAttributes.Static)) {
                InterpretLoadStaticField(token);
                return;
            }

            TypeDescription fieldType = GetActualType(field.Type);
            if (fieldType.IsSpecialSystemType) {
                switch (fieldType.SpecialSystemType) {
                    case SpecialSystemType.Boolean:
                        LoadField<byte, int>(field, fieldType);
                        break;
                    case SpecialSystemType.Char:
                        LoadField<char, int>(field, fieldType);
                        break;
                    case SpecialSystemType.SByte:
                    case SpecialSystemType.Byte:
                        LoadField<sbyte, int>(field, fieldType);
                        break;
                    case SpecialSystemType.Int16:
                    case SpecialSystemType.UInt16:
                        LoadField<short, int>(field, fieldType);
                        break;
                    case SpecialSystemType.Int32:
                    case SpecialSystemType.UInt32:
                        LoadField<int, int>(field, fieldType);
                        break;
                    case SpecialSystemType.Int64:
                    case SpecialSystemType.UInt64:
                        LoadField<long, long>(field, fieldType);
                        break;
                    case SpecialSystemType.Single:
                        LoadFieldFloat(field, fieldType);
                        break;
                    case SpecialSystemType.Double:
                        LoadField<double, double>(field, fieldType);
                        break;
                    case SpecialSystemType.IntPtr:
                    case SpecialSystemType.UIntPtr:
                    case SpecialSystemType.Object:
                    case SpecialSystemType.String:
                    case SpecialSystemType.Array:
                    case SpecialSystemType.MulticastDelegate:
                    case SpecialSystemType.RuntimeTypeHandle:
                    case SpecialSystemType.Exception:
                        LoadField<IntPtr, IntPtr>(field, fieldType);
                        break;
                    case SpecialSystemType.Enum:
                        // Enums should not happen as the underlying type should have been resolved.
                        Debug.Assert(false);
                        break;
                }
            } else if (fieldType.IsLargeStruct) {
                LoadFieldLargeStruct(field, fieldType);
            } else if (fieldType.IsStruct) {
                // Small structs are always 8 bytes wide.
                LoadField<long, long>(field, fieldType);
            } else {
                // Everything else is a pointer.
                LoadField<IntPtr, IntPtr>(field, fieldType);
            }
        }

        /// <summary>
        /// Pushes the address of a field of an object on the stack.
        /// 
        /// The object on the stack is allowed to be of the following types:
        ///     - ObjectReference
        ///     - ByReference
        ///     - NativeInt
        /// 
        /// Stack: ..., obj -> .., address (managed or unmanaged)
        /// </summary>
        /// <param name="token">The token of the field</param>
        private unsafe void InterpretLoadFieldAddress(EntityHandle token) {
            Debug.Assert(token.Kind == HandleKind.FieldDefinition || token.Kind == HandleKind.MemberReference);

            FieldDescription field = m_TokenResolver.ResolveField(token);
            // It is possible for this normal field instruction to also refer to static fields.
            // In that case we just delegate the work as if a corresponding static field instruction has been made.
            if (field.Attributes.HasFlag(FieldAttributes.Static)) {
                InterpretLoadStaticFieldAddress(token);
                return;
            }

            TypeDescription fieldType = field.Type;

            StackItemType stackItemType = m_Frame.PeekItemType(0);
            byte* pointer = (byte*)m_Frame.Pop<IntPtr>();
            // For an object reference we first have to add the base object size to the pointer.
            if (stackItemType == StackItemType.ObjectReference) {
                pointer += ObjectBase.SIZE;
            }

            pointer += field.Offset;

            // If we got an unmanaged pointer the result is also an unmanaged pointer.
            // Otherwise the result is a managed pointer.
            if (stackItemType == StackItemType.NativeInt) {
                m_Frame.Push(RuntimeType.FromPointer(fieldType), (IntPtr)pointer);
            } else {
                m_Frame.Push(RuntimeType.FromReference(fieldType), (IntPtr)pointer);
            }
        }

        /// <summary>
        /// Stores a value from the stack in a field of an object.
        /// 
        /// The object on the stack is allowed to be of the following types:
        ///     - ObjectReference
        ///     - ByReference
        ///     - NativeInt
        ///     
        /// Stack: ..., obj, value -> ...
        /// </summary>
        /// <param name="token"></param>
        private unsafe void InterpretStoreField(EntityHandle token) {
            Debug.Assert(token.Kind == HandleKind.FieldDefinition || token.Kind == HandleKind.MemberReference);

            FieldDescription field = m_TokenResolver.ResolveField(token);
            // It is possible for this normal field instruction to also refer to static fields.
            // In that case we just delegate the work as if a corresponding static field instruction has been made.
            if (field.Attributes.HasFlag(FieldAttributes.Static)) {
                InterpretStoreStaticField(token);
                return;
            }

            TypeDescription fieldType = GetActualType(field.Type);
            if (fieldType.IsSpecialSystemType) {
                switch (fieldType.SpecialSystemType) {
                    case SpecialSystemType.Boolean:
                        StoreField<int, bool>(field);
                        break;
                    case SpecialSystemType.Char:
                        StoreField<int, char>(field);
                        break;
                    case SpecialSystemType.SByte:
                    case SpecialSystemType.Byte:
                        StoreField<int, sbyte>(field);
                        break;
                    case SpecialSystemType.Int16:
                    case SpecialSystemType.UInt16:
                        StoreField<int, short>(field);
                        break;
                    case SpecialSystemType.Int32:
                    case SpecialSystemType.UInt32:
                        StoreField<int, int>(field);
                        break;
                    case SpecialSystemType.Int64:
                    case SpecialSystemType.UInt64:
                        StoreField<long, long>(field);
                        break;
                    case SpecialSystemType.Single:
                        StoreFieldFloat(field);
                        break;
                    case SpecialSystemType.Double:
                        StoreField<double, double>(field);
                        break;
                    case SpecialSystemType.IntPtr:
                    case SpecialSystemType.UIntPtr:
                    case SpecialSystemType.Object:
                    case SpecialSystemType.String:
                    case SpecialSystemType.Array:
                    case SpecialSystemType.MulticastDelegate:
                    case SpecialSystemType.RuntimeTypeHandle:
                    case SpecialSystemType.Exception:
                        StoreField<IntPtr, IntPtr>(field);
                        break;
                    case SpecialSystemType.Enum:
                        // Enums should not happen as the underlying type should have been resolved.
                        Debug.Assert(false);
                        break;
                }
            } else if (fieldType.IsLargeStruct) {
                StoreFieldLargeStruct(field);
            } else if (fieldType.IsStruct) {
                // Small structs are always 8 bytes wide.
                StoreField<long, long>(field);
            } else {
                // Everything else is a pointer.
                StoreField<IntPtr, IntPtr>(field);
            }
        }

        /// <summary>
        /// Pushes the value of a static field on the stack.
        /// 
        /// Stack: ... -> ..., value
        /// </summary>
        /// <param name="token">The token of the static field</param>
        private unsafe void InterpretLoadStaticField(EntityHandle token) {
            Debug.Assert(token.Kind == HandleKind.FieldDefinition || token.Kind == HandleKind.MemberReference);

            FieldDescription field = m_TokenResolver.ResolveField(token);
            TypeDescription fieldType = GetActualType(field.Type);

            // We need to make sure the static constructor has run.
            VirtualMachine.ExecutionEngine.EnsureStaticConstructorHasRun(field.OwningType);

            // Figure out the type we are going to push on the stack.
            RuntimeType runtimeTypeToPush = RuntimeType.FromType(fieldType);

            if (fieldType.IsSpecialSystemType) {
                switch (fieldType.SpecialSystemType) {
                    case SpecialSystemType.Boolean:
                        m_Frame.Push(runtimeTypeToPush, StaticStorage.LoadStaticField<bool, int>(field));
                        break;
                    case SpecialSystemType.Char:
                        m_Frame.Push(runtimeTypeToPush, StaticStorage.LoadStaticField<char, int>(field));
                        break;
                    case SpecialSystemType.SByte:
                    case SpecialSystemType.Byte:
                        m_Frame.Push(runtimeTypeToPush, StaticStorage.LoadStaticField<byte, int>(field));
                        break;
                    case SpecialSystemType.Int16:
                    case SpecialSystemType.UInt16:
                        m_Frame.Push(runtimeTypeToPush, StaticStorage.LoadStaticField<short, int>(field));
                        break;
                    case SpecialSystemType.Int32:
                    case SpecialSystemType.UInt32:
                        m_Frame.Push(runtimeTypeToPush, StaticStorage.LoadStaticField<int, int>(field));
                        break;
                    case SpecialSystemType.Int64:
                    case SpecialSystemType.UInt64:
                        m_Frame.Push(runtimeTypeToPush, StaticStorage.LoadStaticField<long, long>(field));
                        break;
                    case SpecialSystemType.Single:
                        m_Frame.Push(runtimeTypeToPush, (double)StaticStorage.LoadStaticField<float, float>(field));
                        break;
                    case SpecialSystemType.Double:
                        m_Frame.Push(runtimeTypeToPush, StaticStorage.LoadStaticField<double, double>(field));
                        break;
                    case SpecialSystemType.IntPtr:
                    case SpecialSystemType.UIntPtr:
                    case SpecialSystemType.Object:
                    case SpecialSystemType.String:
                    case SpecialSystemType.Array:
                    case SpecialSystemType.MulticastDelegate:
                    case SpecialSystemType.RuntimeTypeHandle:
                    case SpecialSystemType.Exception:
                        m_Frame.Push(runtimeTypeToPush, StaticStorage.LoadStaticField<IntPtr, IntPtr>(field));
                        break;
                    case SpecialSystemType.Enum:
                        // Enums should not happen as the underlying type should have been resolved.
                        Debug.Assert(false);
                        break;
                }
            } else if (fieldType.IsLargeStruct) {
                // For large structs we simple get the static field address so it can be copied from there.
                m_Frame.PushLargeStructFrom(runtimeTypeToPush, (void*)StaticStorage.LoadStaticFieldAddress(field));
            } else if (fieldType.IsStruct) {
                // Small structs are always 8 bytes wide.
                m_Frame.Push(runtimeTypeToPush, StaticStorage.LoadStaticField<long, long>(field));
            } else {
                // Everything else is a pointer.
                m_Frame.Push(runtimeTypeToPush, StaticStorage.LoadStaticField<IntPtr, IntPtr>(field));
            }
        }

        /// <summary>
        /// Pushes the address of a static field on the stack.
        /// 
        /// Stack: ... -> ..., address (managed or unmanaged)
        /// </summary>
        /// <param name="token">The token of the static field</param>
        private unsafe void InterpretLoadStaticFieldAddress(EntityHandle token) {
            Debug.Assert(token.Kind == HandleKind.FieldDefinition || token.Kind == HandleKind.MemberReference);

            FieldDescription field = m_TokenResolver.ResolveField(token);
            TypeDescription fieldType = GetActualType(field.Type);

            // We need to make sure the static constructor has run.
            VirtualMachine.ExecutionEngine.EnsureStaticConstructorHasRun(field.OwningType);

            // NOTE: Currently we do not distinguish between a managed or unmanaged pointer to the static field.
            m_Frame.Push(RuntimeType.FromReference(fieldType), StaticStorage.LoadStaticFieldAddress(field));
        }

        /// <summary>
        /// Stores a value from the stack in a static field.
        /// 
        /// Stack: ..., value, -> ...
        /// </summary>
        /// <param name="token">The token of the static field</param>
        private unsafe void InterpretStoreStaticField(EntityHandle token) {
            Debug.Assert(token.Kind == HandleKind.FieldDefinition || token.Kind == HandleKind.MemberReference);

            FieldDescription field = m_TokenResolver.ResolveField(token);
            TypeDescription fieldType = GetActualType(field.Type);

            // We need to make sure the static constructor has run.
            VirtualMachine.ExecutionEngine.EnsureStaticConstructorHasRun(field.OwningType);

            if (fieldType.IsSpecialSystemType) {
                switch (fieldType.SpecialSystemType) {
                    case SpecialSystemType.Boolean:
                        StaticStorage.StoreStaticField<int, bool>(field, m_Frame.Pop<int>());
                        break;
                    case SpecialSystemType.Char:
                        StaticStorage.StoreStaticField<int, char>(field, m_Frame.Pop<int>());
                        break;
                    case SpecialSystemType.SByte:
                    case SpecialSystemType.Byte:
                        StaticStorage.StoreStaticField<int, sbyte>(field, m_Frame.Pop<int>());
                        break;
                    case SpecialSystemType.Int16:
                    case SpecialSystemType.UInt16:
                        StaticStorage.StoreStaticField<int, short>(field, m_Frame.Pop<int>());
                        break;
                    case SpecialSystemType.Int32:
                    case SpecialSystemType.UInt32:
                        StaticStorage.StoreStaticField<int, int>(field, m_Frame.Pop<int>());
                        break;
                    case SpecialSystemType.Int64:
                    case SpecialSystemType.UInt64:
                        StaticStorage.StoreStaticField<long, long>(field, m_Frame.Pop<long>());
                        break;
                    case SpecialSystemType.Single:
                        StaticStorage.StoreStaticField<float, float>(field, (float)m_Frame.Pop<double>());
                        break;
                    case SpecialSystemType.Double:
                        StaticStorage.StoreStaticField<double, double>(field, m_Frame.Pop<double>());
                        break;
                    case SpecialSystemType.IntPtr:
                    case SpecialSystemType.UIntPtr:
                    case SpecialSystemType.Object:
                    case SpecialSystemType.String:
                    case SpecialSystemType.Array:
                    case SpecialSystemType.MulticastDelegate:
                    case SpecialSystemType.RuntimeTypeHandle:
                    case SpecialSystemType.Exception:
                        StaticStorage.StoreStaticField<IntPtr, IntPtr>(field, m_Frame.Pop<IntPtr>());
                        break;
                    case SpecialSystemType.Enum:
                        // Enums should not happen as the underlying type should have been resolved.
                        Debug.Assert(false);
                        break;
                }
            } else if (fieldType.IsLargeStruct) {
                // For large structs we simple get the static field address so it can be copied to there.
                m_Frame.PopLargeStructTo((void*)StaticStorage.LoadStaticFieldAddress(field));
            } else if (fieldType.IsStruct) {
                // Small structs are always 8 bytes wide.
                StaticStorage.StoreStaticField<long, long>(field, m_Frame.Pop<long>());
            } else {
                // Everything else is a pointer.
                StaticStorage.StoreStaticField<IntPtr, IntPtr>(field, m_Frame.Pop<IntPtr>());
            }
        }

        /// <summary>
        /// Loads a field of an object of a given type.
        /// </summary>
        /// <typeparam name="TFrom">The type of the field</typeparam>
        /// <typeparam name="TTo">The type the value of the field should be cast to</typeparam>
        /// <param name="field">The field to load</param>
        /// <param name="fieldType">The metadata field type description</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void LoadField<TFrom, TTo>(FieldDescription field, TypeDescription fieldType) where TFrom : unmanaged where TTo : unmanaged {
            RuntimeType runtimeType = m_Frame.PeekType(0);
            StackItemType stackItemType = runtimeType.ItemType;

            byte* pointer;
            if (stackItemType == StackItemType.ValueType) {
                // NOTE: For a struct instance it gets a little hacky.
                // We first get the address to the the instance and then pop it from the stack.
                // We assume that the popping does NOT infact free any actual memory.
                // Instead we rely on the instance still being present at the address.
                // This means we can properly read the field from it.
                if (runtimeType.Type.IsLargeStruct) {
                    pointer = (byte*)m_Frame.PeekLargeStructAddress();
                    m_Frame.PopLargeStruct();
                } else {
                    pointer = (byte*)m_Frame.PeekAddress(0);
                    m_Frame.Pop<long>();
                }
            } else if (stackItemType == StackItemType.ObjectReference) {
                pointer = (byte*)(m_Frame.Pop<IntPtr>());
                ThrowOnInvalidPointer(pointer);

                // For an object reference we first have to add the base object size to the pointer.
                pointer += ObjectBase.SIZE;
            } else {
                pointer = (byte*)m_Frame.Pop<IntPtr>();
                ThrowOnInvalidPointer(pointer);
            }

            pointer += field.Offset;

            TFrom temp = *(TFrom*)pointer;
            TTo value = Unsafe.As<TFrom, TTo>(ref temp);
            m_Frame.Push(RuntimeType.FromType(fieldType), value);
        }

        /// <summary>
        /// Loads a field of an object of type float.
        /// </summary>
        /// <param name="field">The field to load</param>
        /// <param name="fieldType">The metadata field type description</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void LoadFieldFloat(FieldDescription field, TypeDescription fieldType) {
            // NOTE: This is pretty much a direct copy of LoadField<T> but we need the special float to double conversion.

            RuntimeType runtimeType = m_Frame.PeekType(0);
            StackItemType stackItemType = runtimeType.ItemType;

            byte* pointer;
            if (stackItemType == StackItemType.ValueType) {
                // NOTE: For a struct instance it gets a little hacky.
                // We first get the address to the the instance and then pop it from the stack.
                // We assume that the popping does NOT infact free any actual memory.
                // Instead we rely on the instance still being present at the address.
                // This means we can properly read the field from it.
                if (runtimeType.Type.IsLargeStruct) {
                    pointer = (byte*)m_Frame.PeekLargeStructAddress();
                    m_Frame.PopLargeStruct();
                } else {
                    pointer = (byte*)m_Frame.PeekAddress(0);
                    m_Frame.Pop<long>();
                }
            } else if (stackItemType == StackItemType.ObjectReference) {
                pointer = (byte*)(m_Frame.Pop<IntPtr>());
                ThrowOnInvalidPointer(pointer);

                // For an object reference we first have to add the base object size to the pointer.
                pointer += ObjectBase.SIZE;
            } else {
                pointer = (byte*)m_Frame.Pop<IntPtr>();
                ThrowOnInvalidPointer(pointer);
            }

            pointer += field.Offset;

            m_Frame.Push(RuntimeType.FromType(fieldType), (double)*(float*)pointer);
        }

        /// <summary>
        /// Loads a field of an object of a large struct type.
        /// </summary>
        /// <param name="field">The field to load</param>
        /// <param name="fieldType">The metadata field type description</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void LoadFieldLargeStruct(FieldDescription field, TypeDescription fieldType) {
            RuntimeType runtimeType = m_Frame.PeekType(0);
            StackItemType stackItemType = runtimeType.ItemType;

            byte* pointer;
            if (stackItemType == StackItemType.ValueType) {
                // NOTE: For a struct instance it gets a little hacky.
                // We first get the address to the the instance and then pop it from the stack.
                // We assume that the popping does NOT infact free any actual memory.
                // Instead we rely on the instance still being present at the address.
                // This means we can properly read the field from it.
                if (runtimeType.Type.IsLargeStruct) {
                    pointer = (byte*)m_Frame.PeekLargeStructAddress();
                    m_Frame.PopLargeStruct();
                } else {
                    pointer = (byte*)m_Frame.PeekAddress(0);
                    m_Frame.Pop<long>();
                }
            } else if (stackItemType == StackItemType.ObjectReference) {
                pointer = (byte*)(m_Frame.Pop<IntPtr>());
                ThrowOnInvalidPointer(pointer);

                // For an object reference we first have to add the base object size to the pointer.
                pointer += ObjectBase.SIZE;
            } else {
                pointer = (byte*)m_Frame.Pop<IntPtr>();
                ThrowOnInvalidPointer(pointer);
            }

            pointer += field.Offset;

            // NOTE: We make additional assumptions here.
            // If we already had a large struct instance as a value on the stack,
            // we are now going to overwrite that very instance with this new large struct we are trying to load.
            // We assume that this overlapping memory copy gets handled properly.
            // Additionally we also assume that we are not going to need a new allocation
            // for the large struct stack as the size of the large struct instance we are
            // trying to load is always going to be equal or less than the size of the
            // large struct instance we had on the stack.
            // Because of these assumptions we do not need to copy the field to a temporary location.
            m_Frame.PushLargeStructFrom(RuntimeType.FromType(fieldType), pointer);
        }

        /// <summary>
        /// Stores a field of an object of a given type.
        /// </summary>
        /// <typeparam name="TFrom">The type of the field</typeparam>
        /// <typeparam name="TTo">The type the value of the field should be cast to</typeparam>
        /// <param name="field">The field to store</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void StoreField<TFrom, TTo>(FieldDescription field) where TFrom : unmanaged where TTo : unmanaged {
            // We do not call this function if we want to store a value type so we can savely pop the value here.
            TFrom temp = m_Frame.Pop<TFrom>();
            TTo value = Unsafe.As<TFrom, TTo>(ref temp);

            StackItemType stackItemType = m_Frame.PeekItemType(0);
            byte* pointer = (byte*)m_Frame.Pop<IntPtr>();
            ThrowOnInvalidPointer(pointer);
            
            // For an object reference we first have to add the base object size to the pointer.
            if (stackItemType == StackItemType.ObjectReference) {
                pointer += ObjectBase.SIZE;
            }

            pointer += field.Offset;

            *(TTo*)pointer = value;
        }

        /// <summary>
        /// Stores a field of an object of type float.
        /// </summary>
        /// <param name="field">The field to store</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void StoreFieldFloat(FieldDescription field) {
            // NOTE: This is pretty much a direct copy of StoreField<T> but we need the special double to float conversion.

            // We do not call this function if we want to store a value type so we can savely pop the value here.
            float value = (float)m_Frame.Pop<double>();

            StackItemType stackItemType = m_Frame.PeekItemType(0);
            byte* pointer = (byte*)m_Frame.Pop<IntPtr>();
            ThrowOnInvalidPointer(pointer);

            // For an object reference we first have to add the base object size to the pointer.
            if (stackItemType == StackItemType.ObjectReference) {
                pointer += ObjectBase.SIZE;
            }

            pointer += field.Offset;

            *(float*)pointer = value;
        }

        /// <summary>
        /// Stores a field of an object of a large struct type.
        /// </summary>
        /// <param name="field">The field to store</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void StoreFieldLargeStruct(FieldDescription field) {
            // We look past the value to check the type of object on the stack
            StackItemType stackItemType = m_Frame.PeekItemType(1);

            // NOTE: The large struct value is stored on a different stack.
            // This means we really only have a stub value on the normal stack.
            // We have to peek here and pop later because otherwise we would corrupt the stack
            // that stores the runtime types which is needed for poping the large struct properly.
            byte* pointer = (byte*)m_Frame.Peek<IntPtr>(1);
            ThrowOnInvalidPointer(pointer);

            // For an object reference we first have to add the base object size to the pointer.
            if (stackItemType == StackItemType.ObjectReference) {
                pointer += ObjectBase.SIZE;
            }

            pointer += field.Offset;

            m_Frame.PopLargeStructTo(pointer);
            // Here we do the 'late' pop which again relies on the fact that there are two stacks
            m_Frame.Pop<IntPtr>();
        }
    }
}
