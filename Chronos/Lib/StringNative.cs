using Chronos.Memory;
using Chronos.Model;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Chronos.Lib {
    /// <summary>
    /// Native functions for the System.String class.
    /// </summary>
    public static unsafe class StringNative {
        /// <summary>
        /// Pseudo string constructor allocating a new string and taking in a character and repeating it a certain number of times.
        /// </summary>
        /// <param name="pointer">The 'this' pointer that is always going to be null</param>
        /// <param name="c">The character to fill the string with</param>
        /// <param name="length">The length to repeat the character</param>
        /// <returns>The new allocated string</returns>
        public static IntPtr Ctor(IntPtr pointer, char c, int length) {
            // We should always get passed a null pointer (it is the 'this' pointer to this pseudo constructor).
            Debug.Assert(pointer == IntPtr.Zero);
            Debug.Assert(length >= 0);

            StringObject* str = VirtualMachine.GarbageCollector.AllocateNewString(length);
            char* buffer = ObjectModel.GetStringBuffer(str);

            // Fill the buffer with the character.
            for (int i = 0; i < length; i++) {
                buffer[i] = c;
            }
            
            return (IntPtr)str;
        }

        /// <summary>
        /// Concatenates two string objects together to form a new one. 
        /// </summary>
        /// <param name="a">The first string</param>
        /// <param name="b">The second string</param>
        /// <returns>The new concatenated string</returns>
        public static IntPtr Concat(IntPtr a, IntPtr b) {
            StringObject* stringA = (StringObject*)a;
            StringObject* stringB = (StringObject*)b;

            // We bail out early if the strings are null or empty.
            if (stringA == null || stringA->Length == 0) {
                return b;
            }
            if (stringB == null || stringB->Length == 0) {
                return a;
            }

            int lengthA = stringA->Length;
            int lengthB = stringB->Length;
            char* bufferA = ObjectModel.GetStringBuffer(stringA);
            char* bufferB = ObjectModel.GetStringBuffer(stringB);

            int length = lengthA + lengthB;
            StringObject* str = VirtualMachine.GarbageCollector.AllocateNewString(length);
            char* buffer = ObjectModel.GetStringBuffer(str);

            // Fill the content of the new string from both others.
            Unsafe.CopyBlock(buffer, bufferA, (uint)lengthA * sizeof(char));
            Unsafe.CopyBlock(buffer + lengthA, bufferB, (uint)lengthB * sizeof(char));

            return (IntPtr)str;
        }

        /// <summary>
        /// Determines if the contents of two strings with the same length are the same.
        /// </summary>
        /// <param name="a">The first string</param>
        /// <param name="b">The second string</param>
        /// <returns>True if the contents are the same otherwise false</returns>
        public static bool EqualsHelper(IntPtr a, IntPtr b) {
            StringObject* stringA = (StringObject*)a;
            StringObject* stringB = (StringObject*)b;

            Debug.Assert(stringA != null);
            Debug.Assert(stringB != null);
            Debug.Assert(stringA->Length == stringB->Length);

            byte* bufferA = (byte*)&stringA->FirstCharacter;
            byte* bufferB = (byte*)&stringB->FirstCharacter;

            return MemoryHelper.MemoryCompare(bufferA, bufferB, stringA->Length * sizeof(char));
        }
    }
}
