using System.Reflection.Metadata;

namespace Chronos.Metadata {
    /// <summary>
    /// The metadata describing an exception region for a method.
    /// </summary>
    public struct MethodExceptionRegion {
        /// <summary>
        /// The kind of the exception region.
        /// </summary>
        public ExceptionRegionKind Kind { get; }
        /// <summary>
        /// The catch type if the region kind is a catch.
        /// </summary>
        public TypeDescription CatchType { get; }
        /// <summary>
        /// The byte offset of the handler block.
        /// </summary>
        public int HandlerOffset { get; }
        /// <summary>
        /// The byte length of the handler block.
        /// </summary>
        public int HandlerLength { get; }
        /// <summary>
        /// The byte offset of the try block.
        /// </summary>
        public int TryOffset { get; }
        /// <summary>
        /// The byte length of the try block.
        /// </summary>
        public int TryLength { get; }

        /// <summary>
        /// Constructs a new method exception region.
        /// </summary>
        /// <param name="kind">The kind of the exception region</param>
        /// <param name="catchType">The catch type if the region kind is a catch</param>
        /// <param name="handlerOffset">The byte offset of the handler block</param>
        /// <param name="handlerLength">The byte length of the handler block</param>
        /// <param name="tryOffset">The byte offset of the try block</param>
        /// <param name="tryLength">The byte length of the try block</param>
        public MethodExceptionRegion(ExceptionRegionKind kind, TypeDescription catchType, int handlerOffset, int handlerLength, int tryOffset, int tryLength) {
            Kind = kind;
            CatchType = catchType;
            HandlerOffset = handlerOffset;
            HandlerLength = handlerLength;
            TryOffset = tryOffset;
            TryLength = tryLength;
        }
    }
}
