using System;

namespace Chronos.Memory {
    /// <summary>
    /// Delegate for an object reference callback.
    /// </summary>
    /// <param name="pointer">The found object references</param>
    public delegate void ObjectReferenceCallback(IntPtr pointer);
}
