namespace SHRestAPI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using SecretHistories.Entities;
    using SecretHistories.Fucine;
    using SecretHistories.Spheres;
    using SecretHistories.UI;

    /// <summary>
    /// Parses fucine path strings the hard way, by comparing against what exists at every step.
    /// </summary>
    /// <remarks>
    /// This is requires because in Book of Hours, one element ("t.ambrosial!"), contains a ! in the path.
    /// This results in the payload id getting cut off early when used with HornedAxe.GetTokenFromPath.
    /// In practice, we could actually just have this return the array of parts and pass that to the FucinePath constructor...
    /// However, the remembering of what spheres and tokens we found is useful.
    /// </remarks>
    // FIXME: Dont throw web exceptions from here, throw better exceptions and wrap them when used in controllers.
    public class SafeFucinePath
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SafeFucinePath"/> class.
        /// </summary>
        /// <param name="path">The path.</param>
        public SafeFucinePath(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (!path.StartsWith("~/"))
            {
                throw new SafeFucinePathException("Only absolute paths are supported.");
            }

            // Leave the slash.
            path = path.Substring(1);

            var parts = new List<FucinePathPart>
            {
                new RootPathPart(),
            };

            var spheresScope = FucineRoot.Get().Spheres;
            while (!string.IsNullOrEmpty(path))
            {
                // Paths are always spheres followed by tokens, but can cut out at any point.
                var sphere = ParseSphereFromPath(ref path, spheresScope);
                if (!sphere)
                {
                    throw new PathElementNotFoundException($"Could not find sphere at path {path}.");
                }

                // We found a sphere, we are no longer targeting a token.
                this.TargetToken = null;
                this.ContainingSphere = this.TargetSphere;
                this.TargetSphere = sphere;
                parts.Add(new SpherePathPart(sphere.Id));

                if (string.IsNullOrEmpty(path))
                {
                    break;
                }

                var token = ParseTokenFromPath(ref path, sphere);
                if (!token)
                {
                    throw new PathElementNotFoundException($"Could not find token at path {path}.");
                }

                // We found a token, we are no longer targeting a sphere.
                this.TargetSphere = null;
                this.ContainingSphere = sphere;
                this.TargetToken = token;
                spheresScope = token.Payload.GetSpheres();
                parts.Add(new TokenPathPart(token.PayloadId));
            }
        }

        /// <summary>
        /// Gets the sphere this path targets.
        /// </summary>
        public Sphere TargetSphere { get; private set; }

        /// <summary>
        /// Gets the token this path targets, or null if the path does not target a token.
        /// </summary>
        public Token TargetToken { get; private set; }

        /// <summary>
        /// Gets the sphere the target is contained in.
        /// </summary>
        public Sphere ContainingSphere { get; private set; }

        /// <summary>
        /// Gets the path parts that make up this path.
        /// </summary>
        public IReadOnlyList<FucinePathPart> PathParts { get; private set; }

        private static Token ParseTokenFromPath(ref string path, Sphere from)
        {
            if (path.StartsWith("/!"))
            {
                path = path.Substring(2);
            }
            else if (path.StartsWith("!"))
            {
                path = path.Substring(1);
            }
            else
            {
                throw new Exception($"Path fragment {path} does not start with a /! or !, so it cannot be a token.");
            }

            var candidates = from.GetTokens().ToArray();
            Array.Sort(candidates, TokenIdLengthComparator.Instance);
            foreach (var token in candidates)
            {
                var payloadId = token.PayloadId;

                // Some payload ids start with this (ElementStacks), some dont (ConnectedTerrain).
                if (payloadId.StartsWith("!"))
                {
                    payloadId = payloadId.Substring(1);
                }

                if (path.StartsWith(payloadId, StringComparison.InvariantCultureIgnoreCase))
                {
                    var substr = path.Substring(payloadId.Length);
                    if (!string.IsNullOrEmpty(substr) && !substr.StartsWith("/") && !substr.StartsWith("!"))
                    {
                        continue;
                    }

                    path = substr;
                    return token;
                }
            }

            return null;
        }

        private static Sphere ParseSphereFromPath(ref string path, ICollection<Sphere> from)
        {
            if (!path.StartsWith("/"))
            {
                throw new Exception($"Path fragment {path} does not start with a /, so it cannot be a sphere.");
            }

            path = path.Substring(1);

            var candidates = from.ToArray();
            Array.Sort(candidates, SphereIdLengthComparator.Instance);
            foreach (var sphere in candidates)
            {
                if (path.StartsWith(sphere.Id, StringComparison.InvariantCultureIgnoreCase))
                {
                    var substr = path.Substring(sphere.Id.Length);
                    if (!string.IsNullOrEmpty(substr) && !substr.StartsWith("/") && !substr.StartsWith("!"))
                    {
                        continue;
                    }

                    path = substr;
                    return sphere;
                }
            }

            return null;
        }

        /// <summary>
        /// An exception indicating an error in a fucine path.
        /// </summary>
        public class SafeFucinePathException : Exception
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="SafeFucinePathException"/> class.
            /// </summary>
            /// <param name="message">The exception message.</param>
            public SafeFucinePathException(string message)
            : base(message)
            {
            }
        }

        /// <summary>
        /// An exception indicating that a path element was not found.
        /// </summary>
        public class PathElementNotFoundException : SafeFucinePathException
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="PathElementNotFoundException"/> class.
            /// </summary>
            /// <param name="message">The exception message.</param>
            public PathElementNotFoundException(string message)
            : base(message)
            {
            }
        }

        private class SphereIdLengthComparator : IComparer<Sphere>
        {
            public static readonly SphereIdLengthComparator Instance = new SphereIdLengthComparator();

            public int Compare(Sphere x, Sphere y)
            {
                // Longer lengths come first, as they are more exacting to match.
                return x.Id.Length - y.Id.Length;
            }
        }

        private class TokenIdLengthComparator : IComparer<Token>
        {
            public static readonly TokenIdLengthComparator Instance = new TokenIdLengthComparator();

            public int Compare(Token x, Token y)
            {
                // Longer lengths come first, as they are more exacting to match.
                return x.PayloadId.Length - y.PayloadId.Length;
            }
        }
    }
}
