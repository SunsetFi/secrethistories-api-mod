namespace SHRestAPI
{
    using System.Collections.Generic;
    using System.Reflection;
    using SecretHistories.Abstract;
    using SecretHistories.Spheres;
    using SecretHistories.UI;

    public static class HierarchyScanner
    {
        private static readonly FieldInfo SphereContainerField = typeof(Sphere).GetField("_container", BindingFlags.NonPublic | BindingFlags.Instance);

        public static IEnumerable<Sphere> GetAncestorSpheres(Token token)
        {
            do
            {
                yield return token.Sphere;
                token = HierarchyScanner.GetParentToken(token);
            } while (token != null);
        }

        private static Token GetParentToken(Token token)
        {
            var container = (IHasSpheres)SphereContainerField.GetValue(token.Sphere);
            if (container is ITokenPayload payload)
            {
                return payload.GetToken();
            }

            return null;
        }
    }
}