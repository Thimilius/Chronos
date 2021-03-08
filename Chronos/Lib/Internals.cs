using Chronos.Metadata;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Chronos.Lib {
    /// <summary>
    /// Stores function pointers for internal methods.
    /// </summary>
    public static class Internals {
        /// <summary>
        /// Holds the function pointers for the internal methods.
        /// </summary>
        private static readonly Dictionary<string, Delegate> m_Internals = new Dictionary<string, Delegate>();

        /// <summary>
        /// Initializes all function pointers to internal methods.
        /// </summary>
        public static void Initialize() {
            // Array
            RegisterInternalMethod("int System.Array.GetLengthHelper(int)", (Func<IntPtr, int, int>)ArrayNative.GetLengthHelper);

            // Console
            RegisterInternalMethod("void System.Console.WriteLine(string)", (Action<IntPtr>)ConsoleNative.WriteLine);

            // Debug
            RegisterInternalMethod("void System.Diagnostics.Debug.Assert(bool)", (Action<bool>)DebugNative.Assert);

            // Delegate
            RegisterInternalMethod("bool System.Delegate.EqualTypesHelper(System.Delegate, System.Delegate)", (Func<IntPtr, IntPtr, bool>)DelegateNative.EqualTypesHelper);
            RegisterInternalMethod("System.Delegate System.Delegate.Combine(System.Delegate, System.Delegate)", (Func<IntPtr, IntPtr, IntPtr>)DelegateNative.Combine);
            RegisterInternalMethod("System.Delegate System.Delegate.Remove(System.Delegate, System.Delegate)", (Func<IntPtr, IntPtr, IntPtr>)DelegateNative.Remove);

            // Enum
            RegisterInternalMethod("string System.Enum.ToString()", (Func<IntPtr, IntPtr>)EnumNative.ToString);

            // GC
            RegisterInternalMethod("void System.GC.Collect()", (Action)VirtualMachine.GarbageCollector.Collect);
            RegisterInternalMethod("void System.GC.SuppressFinalize(object)", (Action<IntPtr>)VirtualMachine.GarbageCollector.SuppressFinalize);
            RegisterInternalMethod("void System.GC.ReRegisterForFinalize(object)", (Action<IntPtr>)VirtualMachine.GarbageCollector.ReRegisterForFinalize);

            // Math
            RegisterInternalMethod("double System.Math.Sin(double)", (Func<double, double>)Math.Sin);
            RegisterInternalMethod("double System.Math.Cos(double)", (Func<double, double>)Math.Cos);
            RegisterInternalMethod("double System.Math.Tan(double)", (Func<double, double>)Math.Tan);
            RegisterInternalMethod("double System.Math.Sinh(double)", (Func<double, double>)Math.Sinh);
            RegisterInternalMethod("double System.Math.Cosh(double)", (Func<double, double>)Math.Cosh);
            RegisterInternalMethod("double System.Math.Tanh(double)", (Func<double, double>)Math.Tanh);
            RegisterInternalMethod("double System.Math.Atin(double)", (Func<double, double>)Math.Asin);
            RegisterInternalMethod("double System.Math.Atos(double)", (Func<double, double>)Math.Acos);
            RegisterInternalMethod("double System.Math.Atan(double)", (Func<double, double>)Math.Atan);
            RegisterInternalMethod("double System.Math.Atan2(double, double)", (Func<double, double, double>)Math.Atan2);

            // Number
            RegisterInternalMethod("string System.Number.CharToString(char)", (Func<char, IntPtr>)NumberNative.CharToString);
            RegisterInternalMethod("string System.Number.SByteToString(sbyte)", (Func<sbyte, IntPtr>)NumberNative.SByteToString);
            RegisterInternalMethod("string System.Number.ByteToString(byte)", (Func<byte, IntPtr>)NumberNative.ByteToString);
            RegisterInternalMethod("string System.Number.Int16ToString(short)", (Func<short, IntPtr>)NumberNative.Int16ToString);
            RegisterInternalMethod("string System.Number.UInt16ToString(ushort)", (Func<ushort, IntPtr>)NumberNative.UInt16ToString);
            RegisterInternalMethod("string System.Number.Int32ToString(int)", (Func<int, IntPtr>)NumberNative.Int32ToString);
            RegisterInternalMethod("string System.Number.UInt32ToString(uint)", (Func<uint, IntPtr>)NumberNative.UInt32ToString);
            RegisterInternalMethod("string System.Number.Int64ToString(long)", (Func<long, IntPtr>)NumberNative.Int64ToString);
            RegisterInternalMethod("string System.Number.UInt64ToString(ulong)", (Func<ulong, IntPtr>)NumberNative.UInt64ToString);
            RegisterInternalMethod("string System.Number.IntPtrToString(System.IntPtr)", (Func<IntPtr, IntPtr>)NumberNative.IntPtrToString);
            RegisterInternalMethod("string System.Number.UIntPtrToString(System.UIntPtr)", (Func<UIntPtr, IntPtr>)NumberNative.UIntPtrToString);

            // Object
            RegisterInternalMethod("System.Type System.Object.GetType()", (Func<IntPtr, IntPtr>)ObjectNative.GetType);
            RegisterInternalMethod("object System.Object.MemberwiseClone()", (Func<IntPtr, IntPtr>)ObjectNative.MemberwiseClone);
            RegisterInternalMethod("bool System.Object.EqualsHelper(object, object)", (Func<IntPtr, IntPtr, bool>)ObjectNative.EqualsHelper);

            // String
            RegisterInternalMethod("void System.String..ctor(char, int)", (Func<IntPtr, char, int, IntPtr>)StringNative.Ctor);
            RegisterInternalMethod("string System.String.Concat(string, string)", (Func<IntPtr, IntPtr, IntPtr>)StringNative.Concat);
            RegisterInternalMethod("bool System.String.EqualsHelper(string, string)", (Func<IntPtr, IntPtr, bool>)StringNative.EqualsHelper);

            // Type
            RegisterInternalMethod("System.Type System.Type.GetTypeFromHandle(System.RuntimeTypeHandle)", (Func<IntPtr, IntPtr>)TypeNative.GetTypeFromHandle);
        }

        /// <summary>
        /// Gets the function pointer to the internal method for a given method.
        /// </summary>
        /// <param name="method">The method to look up</param>
        /// <returns>The function pointer to the internal method</returns>
        public static Delegate Get(MethodDescription method) {
            Debug.Assert(m_Internals.ContainsKey(method.FullName));
            return m_Internals[method.FullName];
        }

        private static void RegisterInternalMethod(string fullName, Delegate internalMethod) {
            m_Internals[fullName] = internalMethod;
        }
    }
}
