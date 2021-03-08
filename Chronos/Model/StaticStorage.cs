using Chronos.Memory;
using Chronos.Metadata;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Chronos.Model {
    /// <summary>
    /// The storage for static data.
    /// </summary>
    public static unsafe class StaticStorage {
        /// <summary>
        /// A handle to the empty string object.
        /// </summary>
        public static IntPtr EmptyString { get; private set; }

        /// <summary>
        /// The allocator used for static data.
        /// </summary>
        private static HeapAllocator s_StaticAllocator;
        /// <summary>
        /// The memory for static data.
        /// </summary>
        private static byte* s_StaticMemory;
        /// <summary>
        /// A list of static objects.
        /// </summary>
        private static ObjectBase* s_StaticObjects;

        /// <summary>
        /// Holds a list of all runtime type objects.
        /// </summary>
        private static readonly Dictionary<TypeDescription, IntPtr> s_RuntimeTypeObjects = new Dictionary<TypeDescription, IntPtr>();

        /// <summary>
        /// Initializes the static storage before loading.
        /// </summary>
        public static void PreLoad() {
            s_StaticAllocator = new HeapAllocator(0);
        }

        /// <summary>
        /// Initializes the empty string instance.
        /// </summary>
        public static void InitializeEmptyString() {
            IntPtr str = AllocateStaticStringFromLiteral(string.Empty);
            EmptyString = str;
        }

        /// <summary>
        /// Initializes the static storage.
        /// </summary>
        public static void Initialize() {
            int size = ComputerStaticStorageSize();
            s_StaticMemory = s_StaticAllocator.Allocate(size);
            
            // Initalize all static memory to 0.
            Unsafe.InitBlock(s_StaticMemory, 0, (uint)size);

            // We explicitly initialize the System.String.Empty field.
            TypeDescription type = MetadataSystem.StringType;
            FieldDescription field = type.GetField("Empty");
            Debug.Assert(field != null);

            StoreStaticField<IntPtr, IntPtr>(field, EmptyString);
        }

        /// <summary>
        /// Loads the value of a static field.
        /// </summary>
        /// <typeparam name="TFrom">The type of the value</typeparam>
        /// <typeparam name="TTo">The type the value should be cast to</typeparam>
        /// <param name="field">The static field to load the value from</param>
        /// <returns>The loaded value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TTo LoadStaticField<TFrom, TTo>(FieldDescription field) where TFrom : unmanaged where TTo : unmanaged {
            TypeDescription type = field.OwningType;

            TFrom temp = *(TFrom*)&s_StaticMemory[type.StaticStorageOffset + field.Offset];

            return Unsafe.As<TFrom, TTo>(ref temp);
        }

        /// <summary>
        /// Loads the address of a static field.
        /// </summary>
        /// <param name="field">The static field to load the address from</param>
        /// <returns>The address of the static field</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IntPtr LoadStaticFieldAddress(FieldDescription field) {
            TypeDescription type = field.OwningType;

            return new IntPtr(&s_StaticMemory[type.StaticStorageOffset + field.Offset]);
        }

        /// <summary>
        /// Stores a static field with a given value.
        /// </summary>
        /// <typeparam name="TFrom">The type of the value</typeparam>
        /// <typeparam name="TTo">The type the value should be cast to</typeparam>
        /// <param name="field">The field to store the value into</param>
        /// <param name="value">The value to store</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void StoreStaticField<TFrom, TTo>(FieldDescription field, TFrom value) where TFrom : unmanaged where TTo : unmanaged {
            TypeDescription type = field.OwningType;

            *(TTo*)&s_StaticMemory[type.StaticStorageOffset + field.Offset] = Unsafe.As<TFrom, TTo>(ref value);
        }

        /// <summary>
        /// Allocates a static string from a string literal.
        /// </summary>
        /// <param name="literal">The literal to fill the new allocated string with</param>
        /// <returns>The new allocated static string</returns>
        public static IntPtr AllocateStaticStringFromLiteral(string literal) {
            int length = literal.Length;

            int size = StringObject.SIZE + sizeof(char) * length;
            ObjectBase* obj = AllocateStaticObject(MetadataSystem.StringType, size);

            StringObject* str = (StringObject*)obj;
            str->Length = length;

            // Copy the literal into the buffer.
            char* buffer = ObjectModel.GetStringBuffer(str);
            ReadOnlySpan<char> span = literal.AsSpan();
            for (int i = 0; i < length; i++) {
                buffer[i] = span[i];
            }

            return (IntPtr)str;
        }

        /// <summary>
        /// Gets or creates a runtime type object for a given type.
        /// </summary>
        /// <param name="type">The type to get or create the runtime type object for</param>
        /// <returns>The runtime type object corresponding to the type</returns>
        public static unsafe IntPtr GetOrCreateRuntimeType(TypeDescription type) {
            if (!s_RuntimeTypeObjects.TryGetValue(type, out IntPtr pointer)) {
                // NOTE: Not sure if these static allocations could cause any problems.
                RuntimeTypeObject* rto = (RuntimeTypeObject*)AllocateStaticObject(MetadataSystem.RuntimeTypeType, RuntimeTypeObject.SIZE);
                rto->FullName = (StringObject*)AllocateStaticStringFromLiteral(type.FullName);

                pointer = new IntPtr(rto);

                s_RuntimeTypeObjects[type] = pointer;
            }
            return pointer;
        }

        /// <summary>
        /// Inspects the static storage for object references.
        /// </summary>
        /// <param name="objectReferenceCallback">Callback that gets called when an object reference is found</param>
        public static void InspectStaticStorage(ObjectReferenceCallback objectReferenceCallback) {
            foreach (TypeDescription type in MetadataSystem.Types) {
                int staticStorageOffset = type.StaticStorageOffset;
                
                // We only need to inspect the type if it has a static storage.
                if (staticStorageOffset < 0) {
                    continue;
                }

                var staticFields = type.StaticFields;
                foreach (FieldDescription field in staticFields) {
                    TypeDescription fieldType = field.Type;

                    byte* fieldMemory = &s_StaticMemory[staticStorageOffset + field.Offset];
                    if (fieldType.IsReference) {
                        objectReferenceCallback(*(IntPtr*)fieldMemory);
                    } else if (fieldType.IsStruct) {
                        GCHelper.InspectStruct(objectReferenceCallback, fieldMemory, type);
                    }
                }
            }
        }

        /// <summary>
        /// Shuts down the static storage and clears all its resources.
        /// </summary>
        public static void Shutdown() {
            ObjectBase* obj = s_StaticObjects;
            while (obj != null) {
                ObjectBase* free = obj;
                obj = obj->Next;

                s_StaticAllocator.Free((byte*)free);
            }
            s_StaticAllocator.Dispose();
        }

        /// <summary>
        /// Computes the combined size for the static storage of all types.
        /// </summary>
        /// <returns>The combined size for the static storage of all types</returns>
        private static int ComputerStaticStorageSize() {
            int result = 0;
            foreach (TypeDescription type in MetadataSystem.Types) {
                // Skip types which do not have any static storage.
                if (type.IsArray || type.IsByReference || type.IsPointer) {
                    continue;
                }

                int offset = result;
                int size = ComputeStaticStorageSizeForType(type);
                result += size;

                // If the type does not have any static storage, we indicate that by setting the offset to a negative number.
                if (size == 0) {
                    offset = -1;
                }
                type.SetStaticStorageOffset(offset);

                // We also want to align the static storage for types to 8 bytes.
                int padding = size % 8;
                result += padding;
            }
            return result;
        }

        /// <summary>
        /// Computes the required static storage size for a give type.
        /// </summary>
        /// <param name="type">The type to compute the static storage size for</param>
        /// <returns>The static storage size for the type</returns>
        private static int ComputeStaticStorageSizeForType(TypeDescription type) {
            // We do not need static storage for enums.
            if (type.IsEnum) {
                return 0;
            }

            var staticFields = type.StaticFields;

            int result = 0;
            foreach (var field in staticFields) {
                TypeDescription fieldType = field.Type;

                // We want to align the static field appropriate for the type.
                int alignment = fieldType.GetAlignment();
                int padding = result % alignment;
                result += padding;

                // We save the current byte offset into the field for later use.
                field.SetOffset(result);

                if (fieldType.IsStruct) {
                    result += fieldType.Size;
                } else {
                    result += IntPtr.Size;
                }
            }
            return result;
        }

        /// <summary>
        /// Allocates a static object.
        /// </summary>
        /// <param name="type">The type of the object</param>
        /// <param name="size">The size of the object</param>
        /// <returns>The new allocated static object</returns>
        private static ObjectBase* AllocateStaticObject(TypeDescription type, int size) {
            byte* memory = s_StaticAllocator.Allocate(size);
            Unsafe.InitBlock(memory, 0, (uint)size);

            ObjectBase* obj = (ObjectBase*)memory;
            obj->TypeHandle = type.Handle;
            obj->Flags = ObjectFlags.None;

            obj->Next = s_StaticObjects;
            s_StaticObjects = obj;

            return obj;
        }
    }
}
