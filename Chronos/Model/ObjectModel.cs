using Chronos.Metadata;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Chronos.Model {
    /// <summary>
    /// Helper methods for the object mode.
    /// </summary>
    public static unsafe class ObjectModel {
        /// <summary>
        /// Resolves a virtual method.
        /// </summary>
        /// <param name="method">The method to resolve</param>
        /// <param name="type">The type used to resolve the method</param>
        /// <returns>The resolved virtual method</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static MethodDescription ResolveVirtualMethod(MethodDescription method, TypeDescription type) {
            Debug.Assert(type.MethodTable.ContainsKey(method));
            
            return type.MethodTable[method];
        }

        /// <summary>
        /// Determines whether an object is an instance of a given type.
        /// </summary>
        /// <param name="obj">The object to check</param>
        /// <param name="type">The instance type to check</param>
        /// <param name="throw">Indicates whether or not to throw if </param>
        /// <returns>True if the object is an instance of the type otherwise false</returns>
        public static bool IsInstanceOf(ObjectBase* obj, TypeDescription type, bool @throw) {
            if (obj == null) {
                if (@throw) {
                    goto fail;
                } else {
                    return true;
                }
            }

            TypeDescription objType = obj->Type;

            // For arrays we actually want to check for the same element type and rank.
            if (type.IsArray) {
                if (objType.IsArray) {
                    ArrayTypeDescription arrayType = type as ArrayTypeDescription;
                    ArrayTypeDescription objArrayType = objType as ArrayTypeDescription;

                    type = arrayType.ParameterType;
                    objType = objArrayType.ParameterType;

                    // Primitives and structs need to be exactly the same
                    if (objType.IsPrimitive || objType.IsStruct) {
                        if (type == objType && arrayType.Rank == objArrayType.Rank) {
                            return true;
                        } else {
                            goto fail;
                        }
                    } else if (objType.IsInstanceOfBase(type) && arrayType.Rank == objArrayType.Rank) {
                        return true;
                    } else {
                        goto fail;
                    }
                } else {
                    goto fail;
                }
            }

            // NOTE: We might want to look for implemented interfaces as well.
            if (objType.IsInstanceOfBase(type)) {
                return true;
            }

            fail:;
            if (@throw) {
                VirtualMachine.ExceptionEngine.ThrowInvalidCastException();
            }
            return false;
        }

        /// <summary>
        /// Creates a new boxed object of a value type.
        /// </summary>
        /// <param name="type">The type of the value type</param>
        /// <param name="value">The data of the value type</param>
        /// <returns>The new allocated boxed object</returns>
        public static ObjectBase* Box(TypeDescription type, byte* value) {
            ObjectBase* obj = VirtualMachine.GarbageCollector.AllocateNewObject(type);
            byte* data = GetObjectData(obj);

            // We can now copy the value to the object data.
            int size = type.Size;
            Unsafe.CopyBlock(data, value, (uint)size);

            return obj;
        }

        /// <summary>
        /// Creates a memberwise clone of a given object.
        /// </summary>
        /// <param name="obj">The object to clone</param>
        /// <returns>The cloned object</returns>
        public static ObjectBase* Clone(ObjectBase *obj) {
            Debug.Assert(obj != null);

            TypeDescription type = obj->Type;
            ObjectBase* clone = null;

            // Strings should not be cloned.
            Debug.Assert(type.SpecialSystemType != SpecialSystemType.String);

            // Arrays have to be handled explicitly.
            if (type.IsArray) {
                ArrayTypeDescription arrayType = type as ArrayTypeDescription;
                int elementSize = arrayType.ParameterType.GetVariableSize();

                if (type.IsMDArray) {
                    MDArrayObject* mdArray = (MDArrayObject*)obj;
                    int length = mdArray->Base.Length;
                    MDArrayObject* mdArrayClone = VirtualMachine.GarbageCollector.AllocateNewMDArray(arrayType, length);
                    int rank = mdArray->Base.Rank;

                    // We have to remember to copy the lengths of all dimensions.
                    int size = ((rank % 2 == 0 ? rank : rank + 1) * sizeof(int)) + (length * elementSize);
                    Unsafe.CopyBlock(&mdArrayClone->FirstLength, &mdArray->FirstLength, (uint)size);

                    clone = &mdArrayClone->Base.Base;
                } else {
                    ArrayObject* array = (ArrayObject*)obj;
                    int length = array->Length;
                    ArrayObject* arrayClone = VirtualMachine.GarbageCollector.AllocateNewSZArray(arrayType, length);

                    Unsafe.CopyBlock(((byte*)arrayClone) + ArrayObject.BUFFER_OFFSET, ((byte*)array) + ArrayObject.BUFFER_OFFSET, (uint)(length * elementSize));

                    clone = &arrayClone->Base;
                }
            } else {
                clone = VirtualMachine.GarbageCollector.AllocateNewObject(type);
                Unsafe.CopyBlock(GetObjectData(clone), GetObjectData(obj), (uint)type.Size);
            }

            return clone;
        }

        /// <summary>
        /// Allocates a string from a literal.
        /// </summary>
        /// <param name="literal">The literal to fill the new allocated string with</param>
        /// <returns>The new allocated string</returns>
        public static IntPtr AllocateStringFromLiteral(string literal) {
            int length = literal.Length;

            StringObject* str = VirtualMachine.GarbageCollector.AllocateNewString(length);

            // Copy the literal into the buffer.
            char* buffer = GetStringBuffer(str);
            ReadOnlySpan<char> span = literal.AsSpan();
            for (int i = 0; i < length; i++) {
                buffer[i] = span[i];
            }

            return (IntPtr)str;
        }

        /// <summary>
        /// Gets the data for an object.
        /// </summary>
        /// <param name="obj">The object to get the data from</param>
        /// <returns>The data for the object</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte* GetObjectData(ObjectBase *obj) {
            return ((byte*)obj) + ObjectBase.SIZE;
        }

        /// <summary>
        /// Gets the character buffer for a string object.
        /// </summary>
        /// <param name="str">The string object to get the character buffer from</param>
        /// <returns>The character buffer</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static char* GetStringBuffer(StringObject* str) {
            return (char*)(((byte*)str) + StringObject.BUFFER_OFFSET);
        }

        /// <summary>
        /// Gets the element buffer for an array object.
        /// </summary>
        /// <typeparam name="T">The type of the array element</typeparam>
        /// <param name="array">The array object to get the element buffer from</param>
        /// <returns>The element buffer</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* GetArrayBuffer<T>(ArrayObject* array) where T : unmanaged {
            return (T*)(((byte*)array) + ArrayObject.BUFFER_OFFSET);
        }
    }
}
