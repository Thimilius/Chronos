using System;
using System.Reflection.Metadata;

namespace Chronos.Metadata {
    /// <summary>
    /// Interface for storing tokens and their corresponding metadata.
    /// </summary>
    public interface ITokenStorage : ITokenResolver {
        /// <summary>
        /// Stores a token with the corresponding metadata.
        /// </summary>
        /// <typeparam name="T">The type of the metadata</typeparam>
        /// <param name="token">The token of the metadata</param>
        /// <param name="description">The metadata to store</param>
        void StoreToken<T>(EntityHandle token, T description) where T : MetadataDescription;
        /// <summary>
        /// Stores a user string token with the pointer to the corresponding string object.
        /// </summary>
        /// <param name="token">The usert string token</param>
        /// <param name="pointer">The pointer to the corresponding string object</param>
        void StoreString(UserStringHandle token, IntPtr pointer);
    }
}
