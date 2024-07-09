namespace SHRestAPI
{
    using System.Collections.Generic;
    using System.Linq;
    using HarmonyLib;
    using SecretHistories.Abstract;
    using SecretHistories.Core;
    using SecretHistories.Entities;
    using SecretHistories.Spheres;
    using SecretHistories.UI;

    internal static class ThreadSafeHornedAxe
    {
        public static AspectsInContext GetAspectsInContext(IHasElementTokens hasTokens)
        {
            // return Watchman.Get<HornedAxe>().GetAspectsInContext(hasTokens);
            return GetAspectsInContext(hasTokens.GetTotalAspects(true));
        }

        public static AspectsInContext GetAspectsInContext(IHasAspects hasAspects)
        {

            return GetAspectsInContext(hasAspects.GetAspects(true));
        }

        /// <summary>
        /// A thread safe GetAspectsInContext that does not rely on global cache dictionaries.
        /// </summary>
        /// <returns>The aspects in context.</returns>
        public static AspectsInContext GetAspectsInContext(AspectsDictionary localAspects)
        {
            var hornedAxe = Watchman.Get<HornedAxe>();
            var hornedAxeTraverse = Traverse.Create(hornedAxe);

            var tabletopAspects = new AspectsDictionary();

            var elementStackList = hornedAxeTraverse.Field<HashSet<Sphere>>("_registeredSpheres")
                .Value.Where(tc => tc.IsExteriorSphere)
                .SelectMany(sphere => sphere.GetElementStacks().Where(s => s.IsValidElementStack()));

            foreach (var elementStack in elementStackList)
            {
                tabletopAspects.CombineAspects(elementStack.GetAspects());
            }

            var allAspectsExtant = new AspectsDictionary();
            allAspectsExtant.CombineAspects(tabletopAspects);
            foreach (var situation in hornedAxe.GetRegisteredSituations())
            {
                allAspectsExtant.CombineAspects(situation.Verb.Aspects);
                allAspectsExtant.CombineAspects(situation.GetCurrentRecipe().Aspects);
                foreach (var sphere in situation.GetSpheres())
                {
                    if (!sphere.IsExteriorSphere)
                    {
                        allAspectsExtant.CombineAspects(sphere.GetTotalAspects());
                    }
                }
            }

            return new AspectsInContext(localAspects, allAspectsExtant);
        }
    }
}
