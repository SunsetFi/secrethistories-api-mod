namespace SHRestAPI.Hashing
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Utilities for computing hash codes.
    /// </summary>
    public static class HashUtils
    {
        /// <summary>
        /// Computes a hash code for the given objects.
        /// </summary>
        /// <param name="objects">The objects to hash.</param>
        /// <returns>A hash code.</returns>
        public static int Hash(params object[] objects)
        {
            unchecked
            {
                return HashAll(objects);
            }
        }

        /// <summary>
        /// Computes a hash code for the given objects, without account to order.
        /// </summary>
        /// <param name="objects">The objects to hash.</param>
        /// <returns>A hash code.</returns>
        public static int HashAllUnordered<T>(IEnumerable<T> objects)
        {
            unchecked
            {
                return HashAllUnordered(objects.Select(x => x.GetHashCode()));
            }
        }

        /// <summary>
        /// Computes a hash code for the given codes.
        /// </summary>
        /// <param name="codes">The codes to hash.</param>
        /// <returns>A hash code.</returns>
        public static int HashAllUnordered(IEnumerable<int> codes)
        {
            unchecked
            {
                return HashAll(codes.OrderBy(c => c));
            }
        }

        /// <summary>
        /// Computes a hash code for the given objects.
        /// </summary>
        /// <param name="objects">The objects to hash.</param>
        /// <returns>A hash code.</returns>
        public static int HashAll<T>(IEnumerable<T> objects)
        {
            unchecked
            {
                return HashAll(objects.Select(x => x.GetHashCode()));
            }
        }

        /// <summary>
        /// Computes a hash code for the given codes.
        /// </summary>
        /// <param name="codes">The codes to hash.</param>
        /// <returns>A hash code.</returns>
        public static int HashAll(IEnumerable<int> codes)
        {
            unchecked
            {
                // This is called a Berstein hash.  I don't understand it.  Nobody understands it.  Just live with it.
                int hash = 17;
                foreach (var code in codes)
                {
                    hash = (hash * 23) + code;
                }

                return hash;
            }
        }
    }
}
