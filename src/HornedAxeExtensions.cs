namespace SHRestAPI
{
    using System.Linq;
    using SecretHistories.Entities;
    using SecretHistories.Entities.NullEntities;
    using SecretHistories.Fucine;
    using SecretHistories.Spheres;
    using SHRestAPI.Server.Exceptions;

    /// <summary>
    /// Extensions for the <see cref="HornedAxe"/> class.
    /// </summary>
    public static class HornedAxeExtensions
    {
        /// <summary>
        /// Gets a sphere by its absolute path, or null if no such sphere exists.
        /// </summary>
        /// <param name="hornedAxe">The HornedAxe object.</param>
        /// <param name="spherePath">The path to the sphere.</param>
        /// <returns>The sphere if it exists, or a NullSphere if not.</returns>
        /// <exception cref="NotFoundException">Thrown if the sphere path is invalid.</exception>
        /// <remarks>
        /// This exists because GetSphereByAbsolutePath is a mess.  It returns the tabletop sphere whenever it cannot find anything, and supports a lot of stuff we don't want.
        /// </remarks>
        public static Sphere GetSphereByReallyAbsolutePathOrNullSphere(this HornedAxe hornedAxe, FucinePath spherePath)
        {
            if (!spherePath.IsAbsolute())
            {
                throw new NotFoundException("trying to find a sphere with sphere path '" + spherePath.ToString() + "', which is not absolute");
            }

            if (spherePath.GetEndingPathPart().Category == FucinePathPart.PathCategory.Root)
            {
                throw new NotFoundException("trying to find a sphere with sphere path '" + spherePath.ToString() + "', which seems to be a bare root path");
            }

            if (spherePath.IsPathToSphereInRoot())
            {
                return FucineRoot.Get().Spheres.SingleOrDefault(c => c.GetAbsolutePath() == spherePath);
            }

            var id = spherePath.GetEndingPathPart().GetId();
            var tokenByPath = hornedAxe.GetTokenByPath(spherePath.GetTokenPath());
            if (tokenByPath != null && tokenByPath.Payload != null)
            {
                var sphereById = tokenByPath.Payload.GetSphereById(id);
                if (sphereById.IsValid())
                {
                    return sphereById;
                }
            }

            return NullSphere.Create();
        }
    }
}
