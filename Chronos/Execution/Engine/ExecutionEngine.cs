using Chronos.Exceptions;
using Chronos.Memory;
using Chronos.Metadata;
using Chronos.Model;
using Chronos.Tracing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Chronos.Execution {
    /// <summary>
    /// Abstract interface for an execution engine.
    /// </summary>
    public abstract partial class ExecutionEngine : IExecutionEngine {
        /// <summary>
        /// <inheritdoc cref="IExecutionEngine.StackSlotSize"/>
        /// </summary>
        public abstract int StackSlotSize { get; }
        /// <summary>
        /// <inheritdoc cref="IExecutionEngine.CallStack"/>
        /// </summary>
        public IEnumerable<IStackFrame> CallStack => m_StackFrames;

        /// <summary>
        /// The tracer used in the execution engine.
        /// </summary>
        protected abstract ITracer Tracer { get; }
        /// <summary>
        /// The stack allocator used in the execution engine.
        /// </summary>
        protected abstract StackAllocator StackAllocator { get; }
        /// <summary>
        /// The ordered collection of stack frames.
        /// </summary>
        private readonly Stack<IStackFrame> m_StackFrames = new Stack<IStackFrame>();

        /// <summary>
        /// <inheritdoc cref="IExecutionEngine.MakeCall(MethodDescription, ref MethodCallData)"/>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void MakeCall(MethodDescription method, ref MethodCallData callData) {
#if TRACE_EXECUTION
            ConsoleColor color = method.ImplAttributes == MethodImplAttributes.IL ? TracingConfig.METHOD_COLOR : TracingConfig.INTERNAL_METHOD_COLOR;
            Tracer.TraceColor(color, "\t" + method.FullName);
            Tracer.TraceLine();
            Tracer.Indent();
#endif
            try {
                DispatchCall(method, ref callData);
            } catch (ManagedException managedException) {
                // The method that got called cleary did not handle the exception.
                // So we push the current stack frame the exception went through.
                if (m_StackFrames.TryPeek(out IStackFrame stackFrame)) {
                    managedException.PushStackFrame(stackFrame);
                }
                

                // Further we need to make sure that the call data gets disposed.
                callData.Dispose();

                throw;
            } finally {
#if TRACE_EXECUTION
                Tracer.Unindent();
#endif
            }
        }
        /// <summary>
        /// <inheritdoc cref="IExecutionEngine.EnsureStaticConstructorHasRun(TypeDescription)"/>
        /// </summary>
        public void EnsureStaticConstructorHasRun(TypeDescription type) {
            Debug.Assert(type != null);

            if (type.Flags.HasFlag(TypeFlags.StaticConstructorNeedsToRun)) {
                // Set the fact that the static constructor has run now before actually calling it.
                // This prevents a dead lock if there are any cyclic dependencies.
                type.SetStaticConstructorHasRun();

                // Before we can call our own constructor, we have to make sure our base classes are initialized first.
                // We do not use recursion here as to properly handle a hiearchy where a middle type does not have a static constructor.
                TypeDescription baseType = type.BaseType;
                while (baseType != null) {
                    if (baseType.Flags.HasFlag(TypeFlags.StaticConstructorNeedsToRun)) {
                        baseType.SetStaticConstructorHasRun();

                        CallStaticConstructor(baseType);
                    }
                    baseType = baseType.BaseType;
                }

                // If the type had the flag we can safely assume we have a valid static constructor we can run.
                CallStaticConstructor(type);
            }
        }

        /// <summary>
        /// <inheritdoc cref="IExecutionEngine.RunFinalizer(ObjectBase*)"/>
        /// </summary>
        public unsafe void RunFinalizer(ObjectBase* obj) {
            TypeDescription type = obj->Type;
            Debug.Assert(type.GetFinalizer() != null);

            // Finalizers are always instance methods without any arguments, so only the 'this' pointer is required.
            ArgumentInfo[] argumentTypes = new ArgumentInfo[1];
            argumentTypes[0] = new ArgumentInfo() {
                RuntimeType = RuntimeType.FromObjectReference(type),
                Offset = 0
            };
            // The size for the 'this' pointer is always going to that of a pointer.
            int argumentSize = IntPtr.Size;
            MethodDescription finalizer = type.GetFinalizer();
            MethodCallData callData = new MethodCallData(StackAllocator, finalizer, argumentTypes, argumentSize);
            callData.SetArgument(0, new IntPtr(obj));

            MakeCall(finalizer, ref callData);

            callData.Dispose();
        }

        /// <summary>
        /// <inheritdoc cref="IExecutionEngine.WalkStack(IStackWalker)"/>
        /// </summary>
        public void WalkStack(IStackWalker stackWalker) {
            Debug.Assert(stackWalker != null);

            foreach (IStackFrame stackFrame in m_StackFrames) {
                stackWalker.OnStackFrame(stackFrame);
            }
        }

        /// <summary>
        /// Runs the given entry point.
        /// </summary>
        /// <param name="method">The entry point to run</param>
        /// <param name="applicationArguments">The arguments to the application</param>
        /// <returns>The exit code of the entry point</returns>
        public int RunEntryPoint(MethodDescription method, IList<string> applicationArguments) {
            Debug.Assert(method != null);
            Debug.Assert(applicationArguments != null);

            MethodSignatureDescription signature = method.Signature;
            MethodCallData callData = PrepareMethodCallDataForEntryPoint(method, applicationArguments);

#if TRACE_EXECUTION
            Tracer.TraceColorLine(TracingConfig.METHOD_COLOR, method.FullName);
#endif
            try {
                DispatchCall(method, ref callData);

                // Check if we there is an exit code that got returned.
                int exitCode = 0;
                if (signature.ReturnType.SpecialSystemType == SpecialSystemType.Int32) {
                    exitCode = callData.GetReturnValue<int>();
                }

                return exitCode;
            } finally {
                callData.Dispose();

                // If we land here and our program finished executing, then that means the stack MUST be empty.
                Debug.Assert(StackAllocator.Offset == 0);
            }
        }

        /// <summary>
        /// Shutsdown the execution engine and clears all its resources.
        /// </summary>
        public void Shutdown() {
            StackAllocator.Dispose();
        }

        /// <summary>
        /// Actual method call work to be implemented.
        /// </summary>
        /// <param name="method">The method to call</param>
        /// <param name="callData">The method call data</param>
        protected abstract void MakeManagedCall(MethodDescription method, ref MethodCallData callData);

        /// <summary>
        /// Pushes a new stack frame.
        /// </summary>
        /// <param name="stackFrame">The new stack frame to push</param>
        protected void PushStackFrame(IStackFrame stackFrame) {
            Debug.Assert(stackFrame != null);

            m_StackFrames.Push(stackFrame);
        }

        /// <summary>
        /// Pops the current stack frame.
        /// </summary>
        protected void PopStackFrame() {
            m_StackFrames.Pop();
        }

        /// <summary>
        /// Dispatches a method call.
        /// </summary>
        /// <param name="callData">The method call data</param>
        private void DispatchCall(MethodDescription method, ref MethodCallData callData) {
            // Before we call the method we have to make sure that the static constructor of the type has been run.
            EnsureStaticConstructorHasRun(method.OwningType);

            // Check wether or not we need to make an internal call.
            MethodImplAttributes implAttributes = method.ImplAttributes;
            if (implAttributes == MethodImplAttributes.IL) {
                MakeManagedCall(method, ref callData);
            } else if (implAttributes == MethodImplAttributes.InternalCall) {
                try {
                    PushStackFrame(new InternalStackFrame(method));
                    MakeInternalCall(method, ref callData);
                } finally {
                    PopStackFrame();
                }
            } else if (implAttributes == MethodImplAttributes.Runtime) {
                try {
                    PushStackFrame(new InternalStackFrame(method));
                    MakeRuntimeCall(method, ref callData);
                } finally {
                    PopStackFrame();
                }
            } else {
                Debug.Assert(false);
            }
        }

        /// <summary>
        /// Calls the static constructor for a given type.
        /// </summary>
        /// <param name="type">The type to call the static constructor for</param>
        private void CallStaticConstructor(TypeDescription type) {
            Debug.Assert(type != null);
            Debug.Assert(type.GetStaticConstructor() != null);

            // Static constructors do not have any arguments and no return value.
            MethodDescription staticConstructor = type.GetStaticConstructor();
            MethodCallData callData = new MethodCallData(StackAllocator, staticConstructor, null, 0);
            MakeCall(staticConstructor, ref callData);

            // We do not really need this Dispose here, as the call data for the static constructor never allocates anything.
            // It is however good practice to call it anyways.
            callData.Dispose();
        }

        /// <summary>
        /// Creates and prepares the method call data for the entry point.
        /// </summary>
        /// <param name="method">The entry point method</param>
        /// <param name="applicationArguments">The arguments to the application</param>
        /// <returns>The prepared method call data</returns>
        private unsafe MethodCallData PrepareMethodCallDataForEntryPoint(MethodDescription method, IList<string> applicationArguments) {
            Debug.Assert(method != null);
            Debug.Assert(applicationArguments != null);

            MethodSignatureDescription signature = method.Signature;
            int argumentCount = signature.ParameterTypes.Count;

            // Make sure we have a valid signature
            if (argumentCount == 0) {
                // We can bail out early if we do not have any arguments.
                return new MethodCallData(StackAllocator, method, null, 0);
            } else if (argumentCount > 1) {
                // We do not support an entry point with more than one argument.
                throw new InvalidProgramException();
            }

            TypeDescription parameterType = signature.ParameterTypes[0];
            ArrayTypeDescription arrayType = parameterType as ArrayTypeDescription;
            if (parameterType.GetCategoryFlags() != TypeFlags.Array || arrayType.ParameterType != MetadataSystem.StringType) {
                // We do not support an entry point where the first type is not a string array.
                throw new InvalidProgramException();
            }

            ArgumentInfo[] argumentTypes = new ArgumentInfo[1] {
                new ArgumentInfo() {
                    RuntimeType = RuntimeType.FromObjectReference(arrayType),
                    Offset = 0
                }
            };
            int argumentSize = MethodCallData.SLOT_SIZE;

            MethodCallData callData = new MethodCallData(StackAllocator, method, argumentTypes, argumentSize);

            // Now that we prepared the types and memory, we can set the actual arguments array.
            int arrayLength = applicationArguments.Count;
            ArrayObject* array = VirtualMachine.GarbageCollector.AllocateNewSZArray(arrayType, arrayLength);
            IntPtr* memory = (IntPtr*)(((byte*)array) + ArrayObject.BUFFER_OFFSET);
            // Fill the array with new string objects.
            for (int i = 0; i < arrayLength; i++) {
                memory[i] = ObjectModel.AllocateStringFromLiteral(applicationArguments[i]);
            }
            callData.SetArgument(0, new IntPtr(array));

            return callData;
        }
    }
}
