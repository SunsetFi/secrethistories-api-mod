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
            // All reads are now thread safe
            return Watchman.Get<HornedAxe>().GetAspectsInContext(hasTokens);

            // return GetAspectsInContext(hasTokens.GetTotalAspects(true));
        }

        public static AspectsInContext GetAspectsInContext(IHasAspects hasAspects)
        {
            // All reads are now thread safe
            return Watchman.Get<HornedAxe>().GetAspectsInContext(hasAspects);

            // return GetAspectsInContext(hasAspects.GetAspects(true));
        }

#if false
        /// <summary>
        /// A thread safe GetAspectsInContext that does not rely on global cache dictionaries.
        /// </summary>
        /// <returns>The aspects in context.</returns>
        public static AspectsInContext GetAspectsInContext(AspectsDictionary localAspects)
        {
            var hornedAxe = Watchman.Get<HornedAxe>();
            var hornedAxeTraverse = Traverse.Create(hornedAxe);

            var tabletopAspects = new AspectsDictionary(hornedAxeTraverse.Field<AspectsDictionary>("_tabletopAspects").Value);
            var allAspectsExtant = new AspectsDictionary(hornedAxeTraverse.Field<AspectsDictionary>("_allAspectsExtant").Value);
            var allAspectsDirty = hornedAxeTraverse.Field<bool>("_allAspectsExtantDirty").Value;

            if (hornedAxeTraverse.Field<bool>("_tabletopAspectsDirty").Value)
            {
                tabletopAspects.Clear();

                var elementStackList = hornedAxeTraverse.Field<HashSet<Sphere>>("_registeredSpheres")
                    .Value.Where(tc => tc.IsExteriorSphere)
                    .SelectMany(sphere => sphere.GetElementStacks().Where(s => s.IsValidElementStack()));

                foreach (var elementStack in elementStackList)
                {
                    tabletopAspects.CombineAspects(elementStack.GetAspects());
                }

                allAspectsDirty = true;
            }

            if (allAspectsDirty)
            {
                allAspectsExtant.Clear();
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
            }

            return new AspectsInContext(localAspects, allAspectsExtant);
        }
#endif
    }
}