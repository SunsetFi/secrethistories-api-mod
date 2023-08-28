namespace SHRestAPI
{
    using System;
    using SecretHistories.Abstract;
    using SecretHistories.Spheres;
    using SecretHistories.UI;
    using SHRestAPI.Server.Exceptions;

    /// <summary>
    /// Extensions for the <see cref="SafeFucinePath"/> class.
    /// </summary>
    public static class SafeFucinePathExtensions
    {
        /// <summary>
        /// Gets the token at the specified path, or null if the item is not found or is not a token.
        /// </summary>
        /// <param name="path">The path to get the token at.</param>
        /// <returns>The token, or null if the path is not a token.</returns>
        public static Token GetToken(this SafeFucinePath path)
        {
            return path.TargetToken;
        }

        /// <summary>
        /// Gets the payload of the token at the specified path.
        /// </summary>
        /// <typeparam name="T">The type of payload to filter for.</typeparam>
        /// <param name="path">The path to get the payload at.</param>
        /// <returns>The payload if the path represents a token with the correct payload, or null if any conditions were not met.</returns>
        public static T GetPayload<T>(this SafeFucinePath path)
            where T : class, ITokenPayload
        {
            var token = path.GetToken();
            if (token == null)
            {
                return null;
            }

            if (token.Payload is T payload)
            {
                return payload;
            }

            return null;
        }

        /// <summary>
        /// Invokes an action with the item at the specified path.
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="path">The path at which to get the item.</param>
        /// <param name="withToken">The funciton to invoke if the item is a token.</param>
        /// <param name="withSphere">The function to invoke if the item is a sphere.</param>
        /// <exception cref="NotFoundException">The path was invalid, or no item was found.</exception>
        public static void WithItemAtAbsolutePath<T>(this SafeFucinePath path, Action<Token> withToken, Action<Sphere> withSphere)
        {
            if (path.TargetToken)
            {
                withToken(path.TargetToken);
            }
            else if (path.TargetSphere)
            {
                withSphere(path.TargetSphere);
            }
            else
            {
                throw new NotFoundException($"No item found at path \"{path}\".");
            }
        }

        /// <summary>
        /// Invokes an action with the item at the specified path.
        /// </summary>
        /// <typeparam name="T">The return type.</typeparam>
        /// <param name="path">The path at which to get the item.</param>
        /// <param name="withToken">The funciton to invoke if the item is a token.</param>
        /// <param name="withSphere">The function to invoke if the item is a sphere.</param>
        /// <returns>The return value from either function.</returns>
        /// <exception cref="NotFoundException">The path was invalid, or no item was found.</exception>
        public static T WithItemAtAbsolutePath<T>(this SafeFucinePath path, Func<Token, T> withToken, Func<Sphere, T> withSphere)
        {
            if (path.TargetToken)
            {
                return withToken(path.TargetToken);
            }
            else if (path.TargetSphere)
            {
                return withSphere(path.TargetSphere);
            }
            else
            {
                throw new NotFoundException($"No item found at path \"{path}\".");
            }
        }
    }
}
