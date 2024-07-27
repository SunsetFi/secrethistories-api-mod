namespace SHRestAPI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using HarmonyLib;
    using SecretHistories.Abstract;
    using SecretHistories.Core;
    using SecretHistories.Entities;
    using SecretHistories.Spheres;
    using SecretHistories.UI;

    internal static class ThreadSafeHornedAxe
    {
        private static readonly FieldInfo SphereAllTokens = typeof(Sphere).GetField("_allTokens", BindingFlags.NonPublic | BindingFlags.Instance);

        public static AspectsInContext GetAspectsInContext(IHasElementTokens hasTokens)
        {
            var aspects = GetTotalAspects(hasTokens, true);
            return GetAspectsInContext(aspects);
        }

        public static AspectsInContext GetAspectsInContext(IHasAspects hasAspects)
        {

            var aspects = GetAspects(hasAspects, true);
            return GetAspectsInContext(aspects);
        }

        /// <summary>
        /// A thread safe GetAspectsInContext that does not rely on global cache dictionaries.
        /// </summary>
        /// <param name="localAspects">The local aspects.</param>
        /// <returns>The aspects in context.</returns>
        public static AspectsInContext GetAspectsInContext(AspectsDictionary localAspects)
        {
            try
            {
                var hornedAxe = Watchman.Get<HornedAxe>();
                var hornedAxeTraverse = Traverse.Create(hornedAxe);

                var tabletopAspects = new AspectsDictionary();

                var elementStackList = hornedAxeTraverse.Field<HashSet<Sphere>>("_registeredSpheres")
                    .Value.Where(tc => tc.IsExteriorSphere)
                    .SelectMany(sphere => sphere.GetElementStacks().Where(s => s.IsValidElementStack()));

                foreach (var elementStack in elementStackList)
                {
                    tabletopAspects.CombineAspects(GetAspects(elementStack));
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
                            allAspectsExtant.CombineAspects(GetTotalAspects(sphere));
                        }
                    }
                }

                return new AspectsInContext(localAspects, allAspectsExtant);
            }
            catch (InvalidOperationException)
            {
                Logging.LogTrace("ThreadSafeHornedAxe.GetAspectsInContext: InvalidOperationException occurred.  Assuming we hit a threading collision; returning no aspects.");
                // This code runs on a background thread, and aspects may be updated by the game's main thread.
                // This will cause our traversal to fail, so we catch the exception and return an empty AspectsInContext.
                return new AspectsInContext(new AspectsDictionary(), new AspectsDictionary());
            }
        }

        public static AspectsDictionary GetTotalAspects(IHasElementTokens hasElementTokens, bool includeSelf = false)
        {
            // GetElementTokens is not thread safe
            if (hasElementTokens is Sphere sphere)
            {
                var tokens = SphereAllTokens.GetValue(sphere) as List<Token>;
                var stacks = tokens.Select(x => x.Payload).Where(t => t is ElementStack e && e.IsValidElementStack()).Cast<ElementStack>();

                var fromStacks = new AspectsDictionary();
                foreach (var stack in stacks)
                {
                    var aspects = GetAspects(stack, includeSelf);
                    fromStacks.CombineAspects(aspects);
                    fromStacks.AspectGroups.Add(aspects);
                }

                return fromStacks;
            }

            throw new System.NotImplementedException($"ThreadSafeGetTotalAspects not implemented for {hasElementTokens.GetType()}");
        }

        public static AspectsDictionary GetAspects(IHasAspects hasAspects, bool includeSelf = false)
        {
            if (hasAspects is ElementStack elementStack)
            {
                var coreAspects = new AspectsDictionary();
                coreAspects.CombineAspects(elementStack.Element.Aspects);
                coreAspects.ApplyMutations(elementStack.Mutations);
                coreAspects.ApplyImmanences(elementStack.Element);

                if (!includeSelf)
                {
                    return coreAspects;
                }

                var aspectsAndSelf = new AspectsDictionary();
                aspectsAndSelf[elementStack.Element.Id] = elementStack.Quantity;
                aspectsAndSelf.CombineAspects(coreAspects);
                return aspectsAndSelf;
            }
            else if (hasAspects is Situation situation)
            {
                var aspects = new AspectsDictionary();
                foreach (var interiorSphere in situation.GetInteriorSpheres())
                {
                    aspects.CombineAspects(GetTotalAspects(interiorSphere, includeSelf));
                }

                aspects.CombineAspects(situation.Verb.Aspects);
                return aspects;

            }
            else if (hasAspects is AbstractPermanentPayload payload)
            {
                // This is thread safe
                return payload.GetAspects(includeSelf);
            }

            throw new System.NotImplementedException($"ThreadSafeGetAspectsIncludingSelf not implemented for {hasAspects.GetType()}");
        }
    }
}
