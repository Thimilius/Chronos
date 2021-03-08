using Chronos.Metadata;
using Chronos.Model;
using System;
using System.Runtime.InteropServices;

namespace Chronos.Lib {
    /// <summary>
    /// Native functions for the System.Type class. 
    /// </summary>
    public static unsafe class TypeNative {
        /// <summary>
        /// Gets the type object from a type handle.
        /// </summary>
        /// <param name="handle">The type handle</param>
        /// <returns>The type object corresponding to the type handle</returns>
        public static IntPtr GetTypeFromHandle(IntPtr handle) {
            TypeDescription type = (TypeDescription)GCHandle.FromIntPtr(handle).Target;
            return StaticStorage.GetOrCreateRuntimeType(type);
        }
    }
}
