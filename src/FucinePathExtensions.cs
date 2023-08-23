namespace SHRestAPI
{
    using System;
    using SecretHistories.Abstract;
    using SecretHistories.Entities;
    using SecretHistories.Fucine;
    using SecretHistories.Spheres;
    using SecretHistories.UI;
    using SHRestAPI.Server.Exceptions;

    /// <summary>
    /// Extensions for the <see cref="FucinePath"/> class.
    /// </summary>
    public static class FucinePathExtensions
    {
        /// <summary>
        /// Gets the token at the specified path, or null if the item is not found or is not a token.
        /// </summary>
        /// <param name="path">The path to get the token at.</param>
        /// <returns>The token, or null if the path is not a token.</returns>
        public static Token GetToken(this FucinePath path)
        {
            return path.WithItemAtAbsolutePath(token => token, sphere => null);
        }

        /// <summary>
        /// Gets the payload of the token at the specified path.
        /// </summary>
        /// <typeparam name="T">The type of payload to filter for.</typeparam>
        /// <param name="path">The path to get the payload at.</param>
        /// <returns>The payload if the path represents a token with the correct payload, or null if any conditions were not met.</returns>
        public static T GetPayload<T>(this FucinePath path)
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
        public static void WithItemAtAbsolutePath<T>(this FucinePath path, Action<Token> withToken, Action<Sphere> withSphere)
        {
            if (!path.IsAbsolute() || path.IsWild() || path.IsRoot())
            {
                throw new NotFoundException("The provided path is not an absolute path.");
            }

            if (path.GetEndingPathPart().Category == FucinePathPart.PathCategory.Token)
            {
                var token = Watchman.Get<HornedAxe>().GetTokenByPath(path);
                if (token == null || !token.IsValid())
                {
                    throw new NotFoundException($"No token found at path \"{path}\".");
                }

                withToken(token);
            }
            else
            {
                var sphere = Watchman.Get<HornedAxe>().GetSphereByReallyAbsolutePathOrNullSphere(path);
                if (sphere == null || !sphere.IsValid())
                {
                    throw new NotFoundException($"No sphere found at path \"{path}\".");
                }

                withSphere(sphere);
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
        public static T WithItemAtAbsolutePath<T>(this FucinePath path, Func<Token, T> withToken, Func<Sphere, T> withSphere)
        {
            if (!path.IsAbsolute() || path.IsWild() || path.IsRoot())
            {
                throw new NotFoundException("The provided path is not an absolute path.");
            }

            if (path.GetEndingPathPart().Category == FucinePathPart.PathCategory.Token)
            {
                var token = Watchman.Get<HornedAxe>().GetTokenByPath(path);
                if (token == null || !token.IsValid())
                {
                    throw new NotFoundException($"No token found at path \"{path}\".");
                }

                return withToken(token);
            }
            else
            {
                var sphere = Watchman.Get<HornedAxe>().GetSphereByReallyAbsolutePathOrNullSphere(path);
                if (sphere == null || !sphere.IsValid())
                {
                    throw new NotFoundException($"No sphere found at path \"{path}\".");
                }

                return withSphere(sphere);
            }
        }
    }
}
