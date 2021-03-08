using System;
using System.Reflection.Metadata;

namespace Chronos.Metadata {
    /// <summary>
    /// Interface for resolving tokens to metadata.
    /// </summary>
    public interface ITokenResolver {
        /// <summary>
        /// Resolve a type with a given token.
        /// </summary>
        /// <param name="token">The type token</param>
        /// <returns>The resolved type</returns>
        TypeDescription ResolveType(EntityHandle token);
        /// <summary>
        /// Resolves a method with a given token.
        /// </summary>
        /// <param name="token">The method token</param>
        /// <returns>The resolved method</returns>
        MethodDescription ResolveMethod(EntityHandle token);
        /// <summary>
        /// Resolves a field with a given token.
        /// </summary>
        /// <param name="token">The field token</param>
        /// <returns>The resolved field</returns>
        FieldDescription ResolveField(EntityHandle token);
        /// <summary>
        /// Resolves the pointer to a string object with a given user string token.
        /// </summary>
        /// <param name="token">The user string token</param>
        /// <returns>The resolved pointer to the string object</returns>
        IntPtr ResolveString(UserStringHandle token);
    }
}
