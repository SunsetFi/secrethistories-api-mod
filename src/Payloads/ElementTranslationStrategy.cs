namespace SHRestAPI.Payloads
{
    using Newtonsoft.Json.Linq;
    using SecretHistories.Entities;
    using SHRestAPI.JsonTranslation;

    /// <summary>
    /// Translation strategy for <see cref="Element"/>.
    /// </summary>
    [JsonTranslatorStrategy]
    [JsonTranslatorTarget(typeof(Element))]
    public class ElementTranslationStrategy
    {
        /// <summary>
        /// Gets the ID of the element.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>The element ID.</returns>
        [JsonPropertyGetter("id")]
        public string GetId(Element element)
        {
            return element.Id;
        }

        /// <summary>
        ///  Gets the aspects of the element.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>The aspects of the element.</returns>
        [JsonPropertyGetter("aspects")]
        public JObject GetAspects(Element element)
        {
            var obj = new JObject();
            foreach (var pair in element.Aspects)
            {
                obj[pair.Key] = pair.Value;
            }

            return obj;
        }

        /// <summary>
        /// Gets what the element burns to.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>The element that this element burns to.</returns>
        [JsonPropertyGetter("burnTo")]
        public string GetBurnTo(Element element)
        {
            return element.BurnTo;
        }

        /// <summary>
        /// Gets the comments of the element.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>The comments of the element.</returns>
        [JsonPropertyGetter("comments")]
        public string GetComments(Element element)
        {
            return element.Comments;
        }

        /// <summary>
        /// Gets the commutes of the element.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>The commutes of the element.</returns>
        [JsonPropertyGetter("commute")]
        public string[] GetCommute(Element element)
        {
            return element.Commute.ToArray();
        }

        /// <summary>
        /// Gets whether the element decays.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>Whether the element decays.</returns>
        [JsonPropertyGetter("decays")]
        public bool GetDecays(Element element)
        {
            return element.Decays;
        }

        /// <summary>
        /// Gets what the element decays to.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>What the element decays to.</returns>
        [JsonPropertyGetter("decayTo")]
        public string GetDecayTo(Element element)
        {
            return element.DecayTo;
        }

        /// <summary>
        /// Gets the description of the element.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>The element description.</returns>
        [JsonPropertyGetter("description")]
        public string GetDescription(Element element)
        {
            return element.Description;
        }

#if BH
        /// <summary>
        /// Gets what the element drowns to.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>What the element drowns to.</returns>
        [JsonPropertyGetter("drownTo")]
        public string GetDrownTo(Element element)
        {
            return element.DrownTo;
        }
#endif

        /// <summary>
        /// Gets the element icon name.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>The element icon name.</returns>
        [JsonPropertyGetter("icon")]
        public string GetIcon(Element element)
        {
            return element.Icon;
        }

        // TODO: Inductions.

        /// <summary>
        /// Gets what the element inherits from.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>What the element inherits from.</returns>
        [JsonPropertyGetter("inherits")]
        public string GetInherits(Element element)
        {
            return element.Inherits;
        }

        /// <summary>
        /// Gets whether the element is an aspect.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>Whether the element is an aspect.</returns>
        [JsonPropertyGetter("isAspect")]
        public bool GetIsAspect(Element element)
        {
            return element.IsAspect;
        }

        /// <summary>
        /// Gets whether the element is hidden.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>Whether the element is hidden.</returns>
        [JsonPropertyGetter("isHidden")]
        public bool GetIsHidden(Element element)
        {
            return element.IsHidden;
        }

        /// <summary>
        /// Gets the label of the element.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>The element label.</returns>
        [JsonPropertyGetter("label")]
        public string GetLabel(Element element)
        {
            return element.Label;
        }

        /// <summary>
        /// Gets the lever of the element.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>The element lever.</returns>
        [JsonPropertyGetter("lever")]
        public string GetLever(Element element)
        {
            return element.Lever;
        }

        /// <summary>
        /// Gets the lifetime of the element.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>The element lifetime.</returns>
        [JsonPropertyGetter("lifetime")]
        public float GetLifetime(Element element)
        {
            return element.Lifetime;
        }

        /// <summary>
        /// Gets the lifetime of the element.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>The element lifetime.</returns>
        [JsonPropertyGetter("lifetimeTicks")]
        public string GetManifestationType(Element element)
        {
            return element.ManifestationType;
        }

        /// <summary>
        /// Gets whether the element is metafictional.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>Whether the element is metafictional.</returns>
        [JsonPropertyGetter("metafictional")]
        public bool GetMetafictional(Element element)
        {
            return element.Metafictional;
        }

        /// <summary>
        /// Gets whether no art is needed for this element.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>Whether no art is needed for this element.</returns>
        [JsonPropertyGetter("noArtNeeded")]
        public bool GetNoArtNeeded(Element element)
        {
            return element.NoArtNeeded;
        }

        /// <summary>
        /// Gets whether the element resaturates as it decays.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>Whether the element resaturates as it decays.</returns>
        [JsonPropertyGetter("resaturate")]
        public bool GetResaturate(Element element)
        {
            return element.Resaturate;
        }

        // TODO: slots

        /// <summary>
        /// Gets the sort of the element.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>The element sort.</returns>
        [JsonPropertyGetter("sort")]
        public string GetSort(Element element)
        {
            return element.Sort;
        }

        /// <summary>
        /// Gets whether this element should only exist once.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>Whether this element should only exist once.</returns>
        [JsonPropertyGetter("unique")]
        public bool GetUnique(Element element)
        {
            return element.Unique;
        }

        /// <summary>
        /// Gets the group in which only one element of this group should exist at a time.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>The group in which only one element of this group should exist at a time.</returns>
        [JsonPropertyGetter("uniquenessGroup")]
        public string GetUniquenessGroup(Element element)
        {
            return element.UniquenessGroup;
        }

        /// <summary>
        /// Gets the icon for the verb represented by this element.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>The icon for the verb represented by this element.</returns>
        [JsonPropertyGetter("verbIcon")]
        public string GetVerbIcon(Element element)
        {
            return element.VerbIcon;
        }

#if BH
        /// <summary>
        /// Gets the Xexts of the element.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>The Xexts of the element.</returns>
        [JsonPropertyGetter("xexts")]
        public JObject GetXexts(Element element)
        {
            var obj = new JObject();
            foreach (var pair in element.Xexts)
            {
                obj[pair.Key] = pair.Value;
            }

            return obj;
        }
#endif

        // TODO: XTriggers
    }
}
