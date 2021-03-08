using Chronos.Metadata;
using Chronos.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Chronos.Lib {
    /// <summary>
    /// Native functions for the System.Delegate class.
    /// </summary>
    public static unsafe class DelegateNative {
        /// <summary>
        /// Checks whether or not two delegates have the same type.
        /// </summary>
        /// <param name="a">The first delegate</param>
        /// <param name="b">The second deletate</param>
        /// <returns>True if the two delegates are the same type otherwise false</returns>
        public static bool EqualTypesHelper(IntPtr a, IntPtr b) {
            ObjectBase* objA = (ObjectBase*)a;
            ObjectBase* objB = (ObjectBase*)b;

            if (objA == null || objB == null) {
                return false;
            }
            
            return objA->TypeHandle == objB->TypeHandle;
        }

        /// <summary>
        /// Combines the invocation list of a given delegate with the invocation list of another delegate.
        /// </summary>
        /// <param name="a">The first delegate</param>
        /// <param name="b">The second deletate</param>
        /// <returns>The new delegate with the combined invocation list</returns>
        public static IntPtr Combine(IntPtr a, IntPtr b) {
            DelegateObject* delA = (DelegateObject*)a;
            DelegateObject* delB = (DelegateObject*)b;

            if (delA == null) {
                return b;
            }

            if (delB == null) {
                return a;
            }

            int newInvocationCount = delA->InvocationList->Length + delB->InvocationList->Length;
            ArrayTypeDescription arrayType = MetadataSystem.GetArrayType(MetadataSystem.MulticastDelegateType, 1);
            ArrayObject* newInvocationList = VirtualMachine.GarbageCollector.AllocateNewSZArray(arrayType, newInvocationCount);
            IntPtr* newBuffer = ObjectModel.GetArrayBuffer<IntPtr>(newInvocationList);

            IntPtr* bufferA = ObjectModel.GetArrayBuffer<IntPtr>(delA->InvocationList);
            for (int i = 0; i < delA->InvocationList->Length; i++) {
                newBuffer[i] = bufferA[i];
            }

            IntPtr* bufferB = ObjectModel.GetArrayBuffer<IntPtr>(delB->InvocationList);
            for (int i = 0; i < delB->InvocationList->Length; i++) {
                newBuffer[i + delA->InvocationList->Length] = bufferB[i];
            }

            DelegateObject* clone = (DelegateObject*)ObjectNative.MemberwiseClone(a);
            clone->InvocationList = newInvocationList;

            return new IntPtr(clone);
        }

        /// <summary>
        /// Removes the invocation list of a given delegate from the invocation list of another delegate.
        /// </summary>
        /// <param name="a">The first delegate to remove the invocation list from</param>
        /// <param name="b">The second deletate with the invocation list to remove</param>
        /// <returns>The new delegate with the invocation list removed</returns>
        public static IntPtr Remove(IntPtr a, IntPtr b) {
            static bool DelegatesAreEqual(DelegateObject* a, DelegateObject* b) {
                if (a == b) {
                    return true;
                }

                return a->MethodHandle == b->MethodHandle && a->Target == b->Target;
            }

            DelegateObject* delA = (DelegateObject*)a;
            DelegateObject* delB = (DelegateObject*)b;

            if (delA == null) {
                return IntPtr.Zero;
            }

            if (delB == null) {
                return a;
            }

            // NOTE: This implementation is quite poor performance wise.
            // It does work like it should however.

            IntPtr* bufferA = ObjectModel.GetArrayBuffer<IntPtr>(delA->InvocationList);
            IntPtr* bufferB = ObjectModel.GetArrayBuffer<IntPtr>(delB->InvocationList);

            List<int> indiciesToRemove = new List<int>();
            for (int i = 0; i < delA->InvocationList->Length; i++) {
                for (int j = 0; j < delB->InvocationList->Length; j++) {
                    if (DelegatesAreEqual((DelegateObject*)bufferA[i], (DelegateObject*)bufferB[j])) {
                        indiciesToRemove.Add(i);
                    }
                }
            }
            
            // It might be that there was no actual delegate found we could remove.
            if (indiciesToRemove.Count == 0) {
                return a;
            }

            // It might be that we want to remove the complete invocation list, in which case the result is null.
            if (delA->InvocationList->Length == indiciesToRemove.Count) {
                return new IntPtr(null);
            }

            Debug.Assert(delA->InvocationList->Length - indiciesToRemove.Count >= 0);
            int newInvocationCount = delA->InvocationList->Length - indiciesToRemove.Count;

            ArrayTypeDescription arrayType = MetadataSystem.GetArrayType(MetadataSystem.MulticastDelegateType, 1);
            ArrayObject* newInvocationList = VirtualMachine.GarbageCollector.AllocateNewSZArray(arrayType, newInvocationCount);
            IntPtr* newBuffer = ObjectModel.GetArrayBuffer<IntPtr>(newInvocationList);

            int newIndex = 0;
            for (int i = 0; i < delA->InvocationList->Length; i++) {
                if (!indiciesToRemove.Contains(i)) {
                    newBuffer[newIndex++] = bufferA[i];
                }
            }

            DelegateObject* clone = (DelegateObject*)ObjectNative.MemberwiseClone(a);
            clone->InvocationList = newInvocationList;

            return new IntPtr(clone);
        }
    }
}
