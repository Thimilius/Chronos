using Chronos.Metadata;
using System;
using System.Diagnostics;

namespace Chronos.Memory {
    public static unsafe class GCHelper {
        /// <summary>
        /// Inspects a struct for object references.
        /// </summary>
        /// <param name="objectReferenceCallback">Callback that gets called when an object reference is found</param>
        /// <param name="memory">The memory of the struct</param>
        /// <param name="type">The type of the struct</param>
        public static void InspectStruct(ObjectReferenceCallback objectReferenceCallback, byte* memory, TypeDescription type) {
            Debug.Assert(objectReferenceCallback != null);
            Debug.Assert(memory != null);
            Debug.Assert(type != null);

            var fields = type.InstanceFields;
            foreach (FieldDescription field in fields) {
                TypeDescription fieldType = field.Type;

                byte* fieldMemory = &memory[field.Offset];
                if (fieldType.IsReference) {
                    objectReferenceCallback(*(IntPtr*)fieldMemory);
                } else if (fieldType.IsStruct) {
                    // For structs we can recursively inspect it.
                    // Because no cyclic references are allowed this works.
                    InspectStruct(objectReferenceCallback, fieldMemory, fieldType);
                }
            }
        }
    }
}
