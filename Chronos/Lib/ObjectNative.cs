using Chronos.Memory;
using Chronos.Metadata;
using Chronos.Model;
using System;
using System.Diagnostics;

namespace Chronos.Lib {
    /// <summary>
    /// Native functions for the System.Object class.
    /// </summary>
    public static unsafe class ObjectNative {
        /// <summary>
        /// Gets the type object of an object.
        /// </summary>
        /// <param name="this">The object to get the type of</param>
        /// <returns>The type object of the object</returns>
        public static IntPtr GetType(IntPtr @this) {
            ObjectBase* obj = (ObjectBase*)@this;
            
            Debug.Assert(obj != null);

            return StaticStorage.GetOrCreateRuntimeType(obj->Type);
        }

        /// <summary>
        /// Creates a shallow clone of a given object.
        /// </summary>
        /// <param name="this">The object to clone</param>
        /// <returns>The shallow clone of the object</returns>
        public static IntPtr MemberwiseClone(IntPtr @this) {
            return new IntPtr(ObjectModel.Clone((ObjectBase*)@this));
        }

        /// <summary>
        /// Determines if two objects are the same.
        /// </summary>
        /// <param name="a">The first object</param>
        /// <param name="b">The second object</param>
        /// <returns>True if the contents are the same otherwise false</returns>
        public static bool EqualsHelper(IntPtr a, IntPtr b) {
            ObjectBase* objA = (ObjectBase*)a;
            ObjectBase* objB = (ObjectBase*)b;

            if (objA == objB) {
                return true;
            }

            if (objA == null || objB == null) {
                return false;
            }

            // For structs we actually want to compare the actual memory and check for equality.
            TypeDescription typeA = objA->Type;
            if (!(typeA.IsStruct || typeA.IsPrimitive)) {
                return false;
            }

            TypeDescription typeB = objB->Type;
            if (typeA != typeB) {
                return false;
            }

            return MemoryHelper.MemoryCompare(ObjectModel.GetObjectData(objA), ObjectModel.GetObjectData(objB), typeA.Size);
        }
    }
}
