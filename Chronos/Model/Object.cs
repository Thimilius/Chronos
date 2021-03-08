using Chronos.Metadata;
using System;
using System.Runtime.InteropServices;

namespace Chronos.Model {
    /// <summary>
    /// Flags for objects.
    /// </summary>
    [Flags]
    public enum ObjectFlags {
        /// <summary>
        /// No flags.
        /// </summary>
        None   = 0,

        /// <summary>
        /// Marked as a reachable object.
        /// </summary>
        GCMark = 1 << 0
    }

    /// <summary>
    /// Special objects are directly implemented internally following a rough hierarchy:
    /// 
    /// ObjectBase                    Base for every object containing:
    ///  |                                - An index as a handle to determine the type of the object
    ///  |                                - A pointer to the next object in the list of objects
    ///  |                                 (This is subject to change as it is quite wasteful memory,
    ///  |                                  but the simplest implementation for now)
    ///  |                            
    ///  +-- StringObject             Represents a string object encoded in UTF-16
    ///  |                            
    ///  +-- ArrayObject              Represents a SZ array object for all element types
    ///  |       |                    
    ///  |       +-- MDArrayObject    Represents MD array object for all element types
    ///  |                            
    ///  +-- DelegateObject           Represents a delegate 
    ///  |                            
    ///  +-- ExceptionObject          Represent the base exception object
    ///  |                            
    ///  +-- RuntimeTypeObject        Represents a runtime type
    ///  
    /// </summary>          
    public unsafe struct ObjectBase {
        /// <summary>
        /// The size of the object base.
        /// </summary>
        public static readonly int SIZE = sizeof(ObjectBase);

        /// <summary>
        /// Gets the type of the object.
        /// </summary>
        public TypeDescription Type => (TypeDescription)TypeHandle.Target;

        /// <summary>
        /// The handle to the type of the object.
        /// </summary>
        public GCHandle TypeHandle;
        /// <summary>
        /// The flags of the object.
        /// </summary>
        public ObjectFlags Flags;
        /// <summary>
        /// A pointer to the next object.
        /// </summary>
        public ObjectBase* Next;
    }

    /// <summary>
    /// Represents a string object encoded in UTF-16.
    /// Additional fields (apart from ObjectBase) directly map to fields in 'System.String'.
    /// </summary>
    public unsafe struct StringObject {
        /// <summary>
        /// The size of a string object.
        /// </summary>
        public static readonly int SIZE = sizeof(StringObject);
        /// <summary>
        /// Gets the offset to the start of the buffer.
        /// </summary>
        public static readonly int BUFFER_OFFSET = sizeof(ObjectBase) + sizeof(int);

        /// <summary>
        /// The object base.
        /// </summary>
        public ObjectBase Base;

        /// <summary>
        /// The length of the string.
        /// </summary>
        public int Length;
        /// <summary>
        /// The first character in the buffer that is directly allocated next to us.
        /// </summary>
        public char FirstCharacter;
    }

    /// <summary>
    /// Represents the base object for all arrays.
    /// Additional fields (apart from ObjectBase) directly map to fields in 'System.Array'.
    /// </summary>
    public unsafe struct ArrayObject {
        /// <summary>
        /// The size of the array base object.
        /// </summary>
        public static readonly int SIZE = sizeof(ArrayObject);
        /// <summary>
        /// Gets the offset to the start of the buffer.
        /// </summary>
        public static readonly int BUFFER_OFFSET = sizeof(ObjectBase) + sizeof(int) + sizeof(int);

        /// <summary>
        /// The object base.
        /// </summary>
        public ObjectBase Base;

        /// <summary>
        /// The length of the array meaning the number of components.
        /// </summary>
        public int Length;
        /// <summary>
        /// The rank of the array.
        /// </summary>
        public int Rank;
    }

    /// <summary>
    /// Represents a multidimensional array object.
    /// </summary>
    public unsafe struct MDArrayObject {
        /// <summary>
        /// Gets the offset to the start of the buffer.
        /// We need to include the fact that multidimensional arrays have the lengths for each dimension stored first.
        /// Further we have to remember that the actual data is aligned to 8 bytes meaning we have to add one for an uneven rank.
        /// </summary>
        public int BUFFER_OFFSET => sizeof(ArrayObject) + (Base.Rank % 2 == 0 ? Base.Rank : Base.Rank + 1) * sizeof(int);

        /// <summary>
        /// The array base.
        /// </summary>
        public ArrayObject Base;

        /// <summary>
        /// The length of the first dimension in the buffer that is directly allocated next to us.
        /// </summary>
        public int FirstLength;
    }

    /// <summary>
    /// Represents a delegate object.
    /// Additional fields (apart from ObjectBase) directly map to fields in 'System.MulticastDelegate'.
    /// </summary>
    public unsafe struct DelegateObject {
        /// <summary>
        /// The object base.
        /// </summary>
        public ObjectBase Base;

        /// <summary>
        /// Gets the method this delegate belongs to.
        /// </summary>
        public MethodDescription Method => (MethodDescription)MethodHandle.Target;

        /// <summary>
        /// The object target this delegate belongs to.
        /// </summary>
        public ObjectBase* Target;
        /// <summary>
        /// The handle to the method this delegate belongs to.
        /// </summary>
        public GCHandle MethodHandle;
        /// <summary>
        /// The invocation list.
        /// </summary>
        public ArrayObject* InvocationList;
    }

    /// <summary>
    /// Represent an exception object at runtime.
    /// Additional fields (apart from ObjectBase) directly map to fields in 'System.Exception'.
    /// </summary>
    public unsafe struct ExceptionObject {
        /// <summary>
        /// The object base.
        /// </summary>
        public ObjectBase Base;

        /// <summary>
        /// The message of the exception.
        /// </summary>
        public StringObject* Message;
        /// <summary>
        /// The inner exception.
        /// </summary>
        public ExceptionObject* InnerException;
    }

    /// <summary>
    /// Represents a type object at runtime.
    /// Additional fields (apart from ObjectBase) directly map to fields in 'System.RuntimeType'.
    /// </summary>
    public unsafe struct RuntimeTypeObject {
        /// <summary>
        /// The size of the runtime type object.
        /// </summary>
        public static readonly int SIZE = sizeof(RuntimeTypeObject);

        /// <summary>
        /// The object base.
        /// </summary>
        public ObjectBase Base;

        /// <summary>
        /// The full name of the runtime type.
        /// </summary>
        public StringObject* FullName;
    }
}
