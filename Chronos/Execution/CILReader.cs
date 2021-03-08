using System.Diagnostics;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;

namespace Chronos.Execution {
    /// <summary>
    /// Reader for the Common Language Intermediate bytecode.
    /// </summary>
    public struct CILReader {
        /// <summary>
        /// Holds the multibyte prefix used in the CIL bytecode.
        /// </summary>
        private const byte MULTIBYTE_PREFIX = 0xFE;

        /// <summary>
        /// A reference to the byte array holding the bytecode.
        /// </summary>
        private readonly byte[] m_Bytes;

        /// <summary>
        /// True if the reader as a next element to read otherwise false.
        /// </summary>
        public bool HasNext => Offset < m_Bytes.Length;
        /// <summary>
        /// The offset of the reader pointing to the next byte (or more) to read.
        /// </summary>
        public int Offset { get; private set; }

        /// <summary>
        /// Constructs a new reader with the given bytecode array.
        /// </summary>
        /// <param name="bytes">The array of bytecode</param>
        public CILReader(byte[] bytes) {
            m_Bytes = bytes;
            Offset = 0;
        }

        /// <summary>
        /// Reads in a byte.
        /// </summary>
        /// <returns>The read byte</returns>
        public byte ReadByte() {
            Debug.Assert(Offset + 1 <= m_Bytes.Length);

            return m_Bytes[Offset++];
        }

        /// <summary>
        /// Reads in a short.
        /// </summary>
        /// <returns>The read short</returns>
        public short ReadShort() {
            Debug.Assert(Offset + 2 <= m_Bytes.Length);

            short val = (short)(m_Bytes[Offset] + (m_Bytes[Offset + 1] << 8));
            Offset += 2;
            return val;
        }

        /// <summary>
        /// Reads in an int.
        /// </summary>
        /// <returns>The read int</returns>
        public int ReadInt() {
            Debug.Assert(Offset + 4 <= m_Bytes.Length);

            int val = m_Bytes[Offset] + (m_Bytes[Offset + 1] << 8) + (m_Bytes[Offset + 2] << 16) + (m_Bytes[Offset + 3] << 24);
            Offset += 4;
            return val;
        }

        /// <summary>
        /// Reads in an int at a given target.
        /// </summary>
        /// <param name="target">The target to read the int from</param>
        /// <returns>The read int from the target</returns>
        public int ReadIntAt(int target) {
            Debug.Assert(target + 4 <= m_Bytes.Length);

            int val = m_Bytes[target] + (m_Bytes[target + 1] << 8) + (m_Bytes[target + 2] << 16) + (m_Bytes[target + 3] << 24);

            return val;
        }

        /// <summary>
        /// Reads in a long.
        /// </summary>
        /// <returns>The read long</returns>
        public long ReadLong() {
            long value = ReadInt();
            value |= ((long)ReadInt()) << 32;
            return value;
        }

        /// <summary>
        /// Reads in a float.
        /// </summary>
        /// <returns>The read float</returns>
        public unsafe float ReadFloat() {
            int value = ReadInt();
            return *(float*)&value;
        }

        /// <summary>
        /// Reads in a double
        /// </summary>
        /// <returns>The read double</returns>
        public unsafe double ReadDouble() {
            long value = ReadLong();
            return *(double*)&value;
        }

        /// <summary>
        /// Reads in a CIL operation code.
        /// </summary>
        /// <returns>The read CIL operation code</returns>
        public ILOpCode ReadOpCode() {
            byte b = ReadByte();
            ILOpCode opCode = (ILOpCode)b;
            if (b == MULTIBYTE_PREFIX) {
                opCode = (ILOpCode)(short)(0xFE00 + ReadByte());
            }
            return opCode;
        }

        /// <summary>
        /// Reads in a metadata token.
        /// </summary>
        /// <returns>The read metadata token</returns>
        public EntityHandle ReadToken() {
            return MetadataTokens.EntityHandle(ReadInt());
        }

        /// <summary>
        /// Reads in a string metadata token.
        /// </summary>
        /// <returns>The read string metadata token</returns>
        public UserStringHandle ReadStringToken() {
            return MetadataTokens.UserStringHandle(ReadInt());
        }

        /// <summary>
        /// Seeks to a given target.
        /// </summary>
        /// <param name="target">The target to seek to</param>
        public void Seek(int target) {
            Debug.Assert(target >= 0 && target < m_Bytes.Length);

            Offset = target;
        }
    }
}
