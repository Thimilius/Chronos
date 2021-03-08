using System;

namespace Chronos.Memory {
    /// <summary>
    /// Delegate for a by reference callback.
    /// </summary>
    /// <param name="pointer">The found by reference</param>
    public delegate void ByReferenceCallback(IntPtr pointer);
}
