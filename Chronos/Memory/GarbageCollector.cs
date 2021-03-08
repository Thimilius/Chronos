using Chronos.Execution;
using Chronos.Metadata;
using Chronos.Model;
using Chronos.Tracing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Chronos.Memory {
    /// <summary>
    /// A simple mark-sweep and non-moving garbage collector.
    /// </summary>
    // NOTE: Currently the sizes of objects themselves are not aligned to 8 bytes.
    // This does however not really matter right now, as we are not managing the heap memory ourselves.
    public unsafe class GarbageCollector : IGarbageCollector, IStackWalker {
        /// <summary>
        /// The growth rate for the garbage collection.
        /// </summary>
        private const int GC_GROWTH_RATE = 2;
        /// <summary>
        /// The inital number of bytes needed before a garbage collection starts.
        /// </summary>
        private const int GC_INITAL_BYTES_NEEDED = 1024 * 1024;
        /// <summary>
        /// The minimum number of bytes for a collection.
        /// </summary>
        private const int GC_MINIMUM_BYTES_NEEDED = GC_INITAL_BYTES_NEEDED / 2;

        /// <summary>
        /// The tracer for garbage collection.
        /// </summary>
        private readonly ITracer m_Tracer;
        /// <summary>
        /// The heap allocator.
        /// </summary>
        private readonly HeapAllocator m_HeapAllocator;

        /// <summary>
        /// A reference to the first item in the linked list of objects.
        /// </summary>
        private ObjectBase* m_Objects;
        /// <summary>
        /// A flag that indicates whether or not a garbage collection is currently in progress.
        /// </summary>
        private bool m_IsCollecting;
        /// <summary>
        /// Contains all root objects in the current garbage collection that need to be traced.
        /// </summary>
        private readonly Queue<IntPtr> m_RootObjects;
        /// <summary>
        /// A set containing the pointers of objects that need to run their finalizer after collection.
        /// </summary>
        private readonly HashSet<IntPtr> m_FinalizationList;
        /// <summary>
        /// The number of objects currently allocated.
        /// </summary>
        private int m_ObjectsAllocated;
        /// <summary>
        /// The number of bytes allocated.
        /// </summary>
        private long m_BytesAllocated;
        /// <summary>
        /// The number of bytes needed before the next garbage collection starts.
        /// </summary>
        private long m_BytesNeededForNextGC;

        /// <summary>
        /// Constructs a new garbage collector with a given heap size.
        /// </summary>
        /// <param name="heapSize">The size of the heap</param>
        public GarbageCollector(ITracer tracer, int heapSize) {
            Debug.Assert(tracer != null);
            
            m_Tracer = tracer;
            m_HeapAllocator = new HeapAllocator(heapSize);
            m_RootObjects = new Queue<IntPtr>();
            m_FinalizationList = new HashSet<IntPtr>();
            m_BytesNeededForNextGC = GC_INITAL_BYTES_NEEDED;
        }

        /// <summary>
        /// <inheritdoc cref="IGarbageCollector.AllocateNewObject(TypeDescription)"/>
        /// </summary>
        public ObjectBase* AllocateNewObject(TypeDescription type) {
            Debug.Assert(type != null);

            int size = ObjectBase.SIZE + type.Size;
            byte* memory = AllocateObject(size);

            ObjectBase* obj = (ObjectBase*)memory;
            InitializeObject(obj, type, size);

            RegisterForFinalize(type, obj);

            return obj;
        }

        /// <summary>
        /// <inheritdoc cref="IGarbageCollector.AllocateNewString(int)"/>
        /// </summary>
        public StringObject* AllocateNewString(int length) {
            int size = StringObject.SIZE + sizeof(char) * length;
            byte* memory = AllocateObject(size);

            ObjectBase* obj = (ObjectBase*)memory;
            InitializeObject(obj, MetadataSystem.StringType, size);
            StringObject* str = (StringObject*)obj;
            str->Length = length;

            return str;
        }

        /// <summary>
        /// <inheritdoc cref="IGarbageCollector.AllocateNewSZArray(ArrayTypeDescription, int)"/>
        /// </summary>
        public ArrayObject* AllocateNewSZArray(ArrayTypeDescription type, int length) {
            Debug.Assert(type != null);
            
            TypeDescription elementType = type.ParameterType;
            if (elementType.IsEnum) {
                elementType = elementType.GetUnderlyingType();
            }

            if (elementType.IsLargeStruct) {
                return AllocateNewSZArray(type, elementType.Size, length);
            }

            if (elementType.IsSpecialSystemType) {
                return elementType.SpecialSystemType switch
                {
                    SpecialSystemType.Boolean => AllocateNewSZArray(type, sizeof(bool), length),
                    SpecialSystemType.Char => AllocateNewSZArray(type, sizeof(char), length),
                    SpecialSystemType.SByte => AllocateNewSZArray(type, sizeof(sbyte), length),
                    SpecialSystemType.Byte => AllocateNewSZArray(type, sizeof(byte), length),
                    SpecialSystemType.Int16 => AllocateNewSZArray(type, sizeof(short), length),
                    SpecialSystemType.UInt16 => AllocateNewSZArray(type, sizeof(ushort), length),
                    SpecialSystemType.Int32 => AllocateNewSZArray(type, sizeof(int), length),
                    SpecialSystemType.UInt32 => AllocateNewSZArray(type, sizeof(uint), length),
                    SpecialSystemType.Int64 => AllocateNewSZArray(type, sizeof(long), length),
                    SpecialSystemType.UInt64 => AllocateNewSZArray(type, sizeof(ulong), length),
                    SpecialSystemType.IntPtr => AllocateNewSZArray(type, IntPtr.Size, length),
                    SpecialSystemType.UIntPtr => AllocateNewSZArray(type, UIntPtr.Size, length),
                    SpecialSystemType.Single => AllocateNewSZArray(type, sizeof(float), length),
                    SpecialSystemType.Double => AllocateNewSZArray(type, sizeof(double), length),
                    _ => AllocateNewSZArray(type, IntPtr.Size, length),
                };
            } else {
                return AllocateNewSZArray(type, IntPtr.Size, length);
            }
        }

        /// <summary>
        /// <inheritdoc cref="IGarbageCollector.AllocateNewMDArray(ArrayTypeDescription, int)"/>
        /// </summary>
        public MDArrayObject* AllocateNewMDArray(ArrayTypeDescription type, int length) {
            Debug.Assert(type != null);

            TypeDescription elementType = type.ParameterType;
            if (elementType.IsEnum) {
                elementType = elementType.GetUnderlyingType();
            }

            if (elementType.IsLargeStruct) {
                return AllocateNewMDArray(type, elementType.Size, length);
            }

            if (elementType.IsSpecialSystemType) {
                return elementType.SpecialSystemType switch
                {
                    SpecialSystemType.Boolean => AllocateNewMDArray(type, sizeof(bool), length),
                    SpecialSystemType.Char => AllocateNewMDArray(type, sizeof(char), length),
                    SpecialSystemType.SByte => AllocateNewMDArray(type, sizeof(sbyte), length),
                    SpecialSystemType.Byte => AllocateNewMDArray(type, sizeof(byte), length),
                    SpecialSystemType.Int16 => AllocateNewMDArray(type, sizeof(short), length),
                    SpecialSystemType.UInt16 => AllocateNewMDArray(type, sizeof(ushort), length),
                    SpecialSystemType.Int32 => AllocateNewMDArray(type, sizeof(int), length),
                    SpecialSystemType.UInt32 => AllocateNewMDArray(type, sizeof(uint), length),
                    SpecialSystemType.Int64 => AllocateNewMDArray(type, sizeof(long), length),
                    SpecialSystemType.UInt64 => AllocateNewMDArray(type, sizeof(ulong), length),
                    SpecialSystemType.IntPtr => AllocateNewMDArray(type, IntPtr.Size, length),
                    SpecialSystemType.UIntPtr => AllocateNewMDArray(type, UIntPtr.Size, length),
                    SpecialSystemType.Single => AllocateNewMDArray(type, sizeof(float), length),
                    SpecialSystemType.Double => AllocateNewMDArray(type, sizeof(double), length),
                    _ => AllocateNewMDArray(type, IntPtr.Size, length),
                };
            } else {
                return AllocateNewMDArray(type, IntPtr.Size, length);
            }
        }

        /// <summary>
        /// <inheritdoc cref="IGarbageCollector.Collect"/>
        /// </summary>
        public void Collect() {
            // We do not want to start collecting if we are already doing it.
            if (m_IsCollecting) {
                return;
            }

            m_IsCollecting = true;
            m_Tracer.TraceColorLine(TracingConfig.GARBAGE_COLLECTION_COLOR,
                "Garbage collection started with {0} bytes and {1} objects allocated",
                m_BytesAllocated,
                m_ObjectsAllocated);
            Stopwatch stopwatch = Stopwatch.StartNew();
            {
                // At the beginning of the algorithm the root object queue must be empty.
                Debug.Assert(m_RootObjects.Count == 0);

                // 1. Collect and mark all root object references.
                CollectRoots();
                // 2. Trace roots to find indirect references.
                Trace();
                // 3. Sweep all remaining non marked objects.
                Sweep();

                // Set the new needed byte count to trigger the next garbage collection.
                m_BytesNeededForNextGC = Math.Max(GC_MINIMUM_BYTES_NEEDED, m_BytesAllocated * GC_GROWTH_RATE);
            }
            m_Tracer.TraceColorLine(TracingConfig.GARBAGE_COLLECTION_COLOR,
                "Garbage collection finished within {0}ms and with {1} bytes and {2} objects remaining",
                stopwatch.Elapsed.TotalMilliseconds,
                m_BytesAllocated,
                m_ObjectsAllocated);

            m_IsCollecting = false;
        }

        /// <summary>
        /// <inheritdoc cref="IGarbageCollector.SuppressFinalize(IntPtr)"/>
        /// </summary>
        public void SuppressFinalize(IntPtr obj) {
            m_FinalizationList.Remove(obj);
        }

        /// <summary>
        /// <inheritdoc cref="IGarbageCollector.ReRegisterForFinalize(IntPtr)"/>
        /// </summary>
        public void ReRegisterForFinalize(IntPtr pointer) {
            ObjectBase* obj = (ObjectBase*)pointer;
            RegisterForFinalize(obj->Type, obj);
        }

        /// <summary>
        /// <inheritdoc cref="IStackWalker.OnStackFrame(IStackFrame)"/>
        /// </summary>
        public void OnStackFrame(IStackFrame stackFrame) {
            Debug.Assert(stackFrame != null);

            stackFrame.InspectArguments(MarkObject, MarkByReference);
            stackFrame.InspectStack(MarkObject, MarkByReference);
            stackFrame.InspectLocals(MarkObject, MarkByReference);
        }

        /// <summary>
        /// Shutsdown the garbage collector and clears all its resources.
        /// </summary>
        public void Shutdown() {
            // Because we are not using our own general purpose allocator right now,
            // we have to free each still remaining object individually.
            while (m_Objects != null) {
                ObjectBase* previous = null;
                ObjectBase* obj = m_Objects;

                while (obj != null) {
                    ObjectBase* free = obj;

                    obj = obj->Next;
                    if (previous != null) {
                        previous->Next = obj;
                    } else {
                        m_Objects = obj;
                    }

                    RunFinalizerAndFreeObject(free);
                }
            }
            Debug.Assert(m_ObjectsAllocated == 0);

            m_HeapAllocator.Dispose();
        }

        /// <summary>
        /// Registers an object for finalization.
        /// </summary>
        /// <param name="type">The type of the object</param>
        /// <param name="obj">The object to register</param>
        private void RegisterForFinalize(TypeDescription type, ObjectBase* obj) {
            // The object might be of a type that as a finalizer.
            if (type.Flags.HasFlag(TypeFlags.HasFinalizer)) {
                // We do not register the finalizer for a simple object of type System.Object.
                if (type != MetadataSystem.ObjectType) {
                    m_FinalizationList.Add(new IntPtr(obj));
                }
            }
        }

        /// <summary>
        /// Collects all current root objects.
        /// </summary>
        private void CollectRoots() {
            // To find the root set of objects:
            //     - We need to walk the stack and look for stored references
            //           - Look into operand stack (normal and for large structs)
            //           - Look into locals
            //           - Look into arguments
            //     - We need to look into statically stored memory

            VirtualMachine.ExecutionEngine.WalkStack(this);

            StaticStorage.InspectStaticStorage(MarkObject);
        }

        /// <summary>
        /// Marks a given object as reachable.
        /// </summary>
        /// <param name="pointer">The object to mark</param>
        private void MarkObject(IntPtr pointer) {
            ObjectBase* obj = (ObjectBase*)pointer;

            // We do not care for null references.
            if (obj == null) {
                return;
            }

            // We do not mark the object again if its already marked.
            if (obj->Flags.HasFlag(ObjectFlags.GCMark)) {
                return;
            }

            obj->Flags |= ObjectFlags.GCMark;
            m_RootObjects.Enqueue(pointer);
        }

        /// <summary>
        /// Marks a given by reference as reachable if required.
        /// </summary>
        /// <param name="pointer">The by reference to mark</param>
        private void MarkByReference(IntPtr pointer) {
            // NOTE: This is an EXTREMELY costly way to do this.
            // A more performant solution would likley require owning and managing the heap memory completly.

            // For a by reference we have to figure out which object it belongs to.
            // It might also be the case that the reference points to something which is not part of the managed heap.

            void* byReferencePointer = (void*)pointer;

            ObjectBase* obj = m_Objects;
            while (obj != null) {
                TypeDescription type = obj->Type;

                // We have to figure out the complete size of the object to determine the pointer bounds.
                void* lowerLimit = obj;
                int size = GetFullObjectSize(obj);
                void* upperLimit = (byte*)lowerLimit + size;

                // Figure out if the pointer points to or into the current object.
                if (byReferencePointer >= lowerLimit && byReferencePointer < upperLimit) {
                    // If we got here we determined the object the by reference points to.
                    MarkObject(new IntPtr(obj));
                    // That means we can skip checking the remaining objects because a pointer can obviously not point to multiple objects.
                    return;
                }

                obj = obj->Next;
            }
        }

        /// <summary>
        /// Traces through all current root objects.
        /// </summary>
        private void Trace() {
            while (m_RootObjects.Count > 0) {
                IntPtr pointer = m_RootObjects.Dequeue();
                ObjectBase* obj = (ObjectBase*)pointer;
                TypeDescription type = obj->Type;
                TraceObject(pointer, type);
            }
        }

        /// <summary>
        /// Traces an individual object for indirect references.
        /// </summary>
        /// <param name="pointer">The pointer to the memory of the object</param>
        /// <param name="type">The type of the object</param>
        private void TraceObject(IntPtr pointer, TypeDescription type) {
            Debug.Assert(pointer != IntPtr.Zero);
            Debug.Assert(type != null);

            if (type == MetadataSystem.StringType) {
                // Strings do not have any fields that can reference other objects.
                return;
            } else if (type.IsArray) {
                TraceArray(type as ArrayTypeDescription, (ArrayObject*)pointer);
            } else {
                byte* memory = (byte*)(pointer + ObjectBase.SIZE);
                TraceObjectFields(memory, type);
            }
        }

        /// <summary>
        /// Traces the elements of an array.
        /// </summary>
        /// <param name="arrayType">The type of the array to trace</param>
        /// <param name="array">The pointer to the array to trace</param>
        private void TraceArray(ArrayTypeDescription arrayType, ArrayObject* array) {
            TypeDescription elementType = arrayType.ParameterType;
            int elementSize = elementType.GetVariableSize();
            int length = array->Length;
            byte* memory = ((byte*)array) + ArrayObject.BUFFER_OFFSET;
            for (int i = 0; i < length; i++) {
                if (elementType.IsReference) {
                    MarkObject(((IntPtr*)memory)[i]);
                } else if (elementType.IsStruct) {
                    GCHelper.InspectStruct(MarkObject, &memory[i * elementSize], elementType);
                } else {
                    // If we have a primitive element type, we do not need to look further.
                    break;
                }
            }
        }

        /// <summary>
        /// Traces all fields of an object.
        /// </summary>
        /// <param name="memory">The pointer to the memory of the object</param>
        /// <param name="type">The type of the object</param>
        private void TraceObjectFields(byte* memory, TypeDescription type) {
            // First make sure we look into the fields of our base type.
            // We can skip checking System.Object as it contains no fields.
            TypeDescription baseType = type.BaseType;
            if (baseType != null && baseType != MetadataSystem.ObjectType) {
                TraceObjectFields(memory, baseType);
            }

            // Now we can look into our own fields.
            var fields = type.InstanceFields;
            foreach (FieldDescription field in fields) {
                TypeDescription fieldType = field.Type;

                byte* fieldMemory = &memory[field.Offset];
                if (fieldType.IsReference) {
                    MarkObject(*(IntPtr*)fieldMemory);
                } else if (fieldType.IsStruct) {
                    GCHelper.InspectStruct(MarkObject, fieldMemory, fieldType);
                }
            }
        }

        /// <summary>
        /// Loops through all objects and sweeps all non-marked objects.
        /// </summary>
        private void Sweep() {
            ObjectBase* previous = null;
            ObjectBase* obj = m_Objects;

            while (obj != null) {
                if (obj->Flags.HasFlag(ObjectFlags.GCMark)) {
                    // Clear the mark flag.
                    obj->Flags &= ~ObjectFlags.GCMark;

                    previous = obj;
                    obj = obj->Next;
                } else {
                    ObjectBase* unreachable = obj;

                    // We have to keep the linked list intact as we are removing items.
                    // We also have to handle the special case in which the very first item can not be reached.
                    // That means we have no 'previous' object yet.
                    obj = obj->Next;
                    if (previous != null) {
                        previous->Next = obj;
                    } else {
                        m_Objects = obj;
                    }

                    RunFinalizerAndFreeObject(unreachable);
                }
            }
        }

        /// <summary>
        /// Allocates a new SZ array of a given type, element size and length.
        /// </summary>
        /// <param name="type">The type of the array</param>
        /// <param name="elementSize">The size of an element in the array</param>
        /// <param name="length">The length of the array</param>
        /// <returns>The new allocated SZ array</returns>
        private ArrayObject* AllocateNewSZArray(ArrayTypeDescription type, int elementSize, int length) {
            Debug.Assert(type != null);
            
            int size = ArrayObject.SIZE + elementSize * length;
            byte* memory = AllocateObject(size);

            ObjectBase* obj = (ObjectBase*)memory;
            InitializeObject(obj, type, size);
            ArrayObject* array = (ArrayObject*)memory;
            array->Length = length;
            array->Rank = type.Rank;

            return array;
        }

        /// <summary>
        /// Allocates a new MD array of a given type, element size and length.
        /// </summary>
        /// <param name="type">The type of the array</param>
        /// <param name="elementSize">The size of an element in the array</param>
        /// <param name="length">The length of the array</param>
        /// <returns>The new allocated MD array</returns>
        private MDArrayObject* AllocateNewMDArray(ArrayTypeDescription type, int elementSize, int length) {
            Debug.Assert(type != null);

            // We want to pad the lengths to 8 bytes for multidimensional arrays with an uneven rank.
            int lengthsSize = type.Rank * sizeof(int) + (type.Rank % 2 == 0 ? 0 : sizeof(int));
            int size = ArrayObject.SIZE + lengthsSize + elementSize * length;
            byte* memory = AllocateObject(size);

            ObjectBase* obj = (ObjectBase*)memory;
            InitializeObject(obj, type, size);
            MDArrayObject* array = (MDArrayObject*)memory;
            array->Base.Length = length;
            array->Base.Rank = type.Rank;

            return array;
        }

        /// <summary>
        /// Initializes a new allocated object.
        /// </summary>
        /// <param name="obj">The pointer of the object</param>
        /// <param name="type">The type of the object</param>
        /// <param name="size">The size of the object</param>
        private void InitializeObject(ObjectBase* obj, TypeDescription type, int size) {
            Debug.Assert(obj != null);

            // The memory of all objects gets initialized to zero.
            Unsafe.InitBlock(obj, 0, (uint)size);

            obj->TypeHandle = type.Handle;
            obj->Flags = ObjectFlags.None;

            // Add object to the list of all objects.
            obj->Next = m_Objects;
            m_Objects = obj;
        }

        /// <summary>
        /// Allocates and object with a given size.
        /// </summary>
        /// <param name="size">The size of the object</param>
        /// <returns>The memory of the new allocated object</returns>
        private byte* AllocateObject(int size) {
            m_ObjectsAllocated++;
            m_BytesAllocated += size;

            // Check if we need to run a garbage collection.
            if (m_BytesAllocated > m_BytesNeededForNextGC) {
                Collect();
            }

            return m_HeapAllocator.Allocate(size);
        }

        /// <summary>
        /// Frees the memory of a given object and runs the finalizer for the object if needed.
        /// </summary>
        /// <param name="obj">The object to free</param>
        private void RunFinalizerAndFreeObject(ObjectBase* obj) {
            Debug.Assert(obj != null);

            // Figure out the size of the object we are freeing.
            int size = GetFullObjectSize(obj);

            // Check if the object needs to have its finalizer run.
            IntPtr pointer = new IntPtr(obj);
            if (m_FinalizationList.Contains(pointer)) {
                m_FinalizationList.Remove(pointer);
                VirtualMachine.ExecutionEngine.RunFinalizer(obj);
            }

            m_ObjectsAllocated--;
            m_BytesAllocated -= size;

#if DEBUG
            Unsafe.InitBlock((byte*)obj, 0xFF, (uint)size);
#endif
            m_HeapAllocator.Free((byte*)obj);
        }

        /// <summary>
        /// Gets the full size a given object occupies in memory.
        /// </summary>
        /// <param name="obj">The object to get the full size for</param>
        /// <returns>The full size the object occupies in memory</returns>
        private int GetFullObjectSize(ObjectBase* obj) {
            TypeDescription type = obj->Type;

            int size = 0;
            if (type == MetadataSystem.StringType) {
                size = StringObject.SIZE + sizeof(char) * ((StringObject*)obj)->Length;
            } else if (type.IsArray) {
                ArrayTypeDescription arrayType = type as ArrayTypeDescription;
                int elementSize = arrayType.ParameterType.GetVariableSize();
                int length = ((ArrayObject*)obj)->Length;
                if (type.IsMDArray) {
                    size = ArrayObject.SIZE + ((MDArrayObject*)obj)->BUFFER_OFFSET + elementSize * length;
                } else {
                    size = ArrayObject.SIZE + elementSize * length;
                }
            } else {
                size = ObjectBase.SIZE + type.Size;
            }

            return size;
        }
    }
}
