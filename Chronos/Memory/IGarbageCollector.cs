using Chronos.Metadata;
using Chronos.Model;
using System;

namespace Chronos.Memory {
    /// <summary>
    /// Interface for a garbage collector implementation.
    /// </summary>
    public unsafe interface IGarbageCollector {
        /// <summary>
        /// Allocates a new object of a given type.
        /// </summary>
        /// <param name="type">The type of the object</param>
        /// <returns>The new allocated object</returns>
        ObjectBase* AllocateNewObject(TypeDescription type);
        /// <summary>
        /// Allocates a new string with a given length.
        /// </summary>
        /// <param name="length">The length of the string</param>
        /// <returns>The new allocated string</returns>
        StringObject* AllocateNewString(int length);
        /// <summary>
        /// Allocates a new SZ array of a given type and length.
        /// </summary>
        /// <param name="type">The type of the array</param>
        /// <param name="length">The length of the array</param>
        /// <returns>The new allocated SZ array</returns>
        ArrayObject* AllocateNewSZArray(ArrayTypeDescription type, int length);
        /// <summary>
        /// Allocates a new MD array of a given type and length.
        /// </summary>
        /// <param name="type">The type of the array</param>
        /// <param name="length">The length of the array</param>
        /// <returns>The new allocated MD array</returns>
        MDArrayObject* AllocateNewMDArray(ArrayTypeDescription type, int length);

        /// <summary>
        /// Performs a garbage collection.
        /// </summary>
        void Collect();
        /// <summary>
        /// Suppresses the execution of the finalizer for a given object.
        /// </summary>
        /// <param name="obj">The object to suppress the execution of the finalizer for</param>
        void SuppressFinalize(IntPtr obj);
        /// <summary>
        /// Reregisters the execution of the finalizer for a given object.
        /// </summary>
        /// <param name="obj">The object to reregister the execution of the finalizer for</param>
        void ReRegisterForFinalize(IntPtr obj);
    }
}
