namespace SHRestAPI.Payloads
{
    using Newtonsoft.Json.Linq;
    using SecretHistories.UI;
    using SHRestAPI.JsonTranslation;

    /// <summary>
    /// Translation strategy for <see cref="ElementStack"/>.
    /// </summary>
    [JsonTranslatorStrategy]
    [JsonTranslatorTarget(typeof(ElementStack))]
    public class ElementStackTranslationStrategy
    {
        /// <summary>
        /// Gets the ID of the element.
        /// </summary>
        /// <param name="elementStack">The element stack.</param>
        /// <returns>The element ID.</returns>
        [JsonPropertyGetter("elementId")]
        public string GetElementId(ElementStack elementStack)
        {
            return elementStack.Element.Id;
        }

        /// <summary>
        /// Gets the quantity of the token.
        /// </summary>
        /// <param name="stack">The element stack.</param>
        /// <returns>The quantity of the stack.</returns>
        [JsonPropertyGetter("quantity")]
        public int GetQuantity(ElementStack stack)
        {
            return stack.Quantity;
        }

        /// <summary>
        /// Sets the quantity of the element stack.
        /// </summary>
        /// <param name="elementStack">The element stack to set.</param>
        /// <param name="quantity">The quantity to set.</param>
        [JsonPropertySetter("quantity")]
        public void SetQuantity(ElementStack elementStack, int quantity)
        {
            elementStack.SetQuantity(quantity);
        }

        /// <summary>
        /// Gets the time remaining in seconds.
        /// </summary>
        /// <param name="elementStack">The element stack to get the lifetime remaining of.</param>
        /// <returns>The lifetime remaining of the ElementStack.</returns>
        [JsonPropertyGetter("lifetimeRemaining")]
        public float GetLifetimeRemaining(ElementStack elementStack)
        {
            return elementStack.LifetimeRemaining;
        }

        /// <summary>
        /// Gets the aspects of the element stack.
        /// </summary>
        /// <param name="elementStack">The element stack to get the aspects of.</param>
        /// <returns>The aspects of the element stack.</returns>
        [JsonPropertyGetter("elementAspects")]
        public JObject GetAspects(ElementStack elementStack)
        {
            var obj = new JObject();
            foreach (var pair in elementStack.Element.Aspects)
            {
                obj[pair.Key] = pair.Value;
            }

            return obj;
        }

        /// <summary>
        /// Gets the mutations of the element stack.
        /// </summary>
        /// <param name="elementStack">The element stack to get the mutations of.</param>
        /// <returns>The mutations of the element stack.</returns>
        [JsonPropertyGetter("mutations")]
        public JObject GetMutations(ElementStack elementStack)
        {
            var obj = new JObject();
            foreach (var pair in elementStack.Mutations)
            {
                obj[pair.Key] = pair.Value;
            }

            return obj;
        }

        /// <summary>
        /// Sets the mutations of the element stack.
        /// </summary>
        /// <param name="elementStack">The element stack.</param>
        /// <param name="mutations">The mutations to set.</param>
        [JsonPropertySetter("mutations")]
        public void SetMutations(ElementStack elementStack, JObject mutations)
        {
            foreach (var pair in mutations)
            {
                elementStack.SetMutation(pair.Key, pair.Value.ToObject<int>(), false);
            }
        }

        /// <summary>
        /// Gets a value indicating if the token is shrouded.
        /// </summary>
        /// <param name="elementStack">The element stack.</param>
        /// <returns>The token's shrouded status.</returns>
        [JsonPropertyGetter("shrouded")]
        public bool GetShrouded(ElementStack elementStack)
        {
#if BH
            return elementStack.IsShrouded;
#else
            return elementStack.Token.Shrouded;
#endif
        }

        /// <summary>
        /// Sets the token's shrouded status.
        /// </summary>
        /// <param name="elementStack">The element stack.</param>
        /// <param name="shrouded">The shrouded status to set.</param>
        [JsonPropertySetter("shrouded")]
        public void SetShrouded(ElementStack elementStack, bool shrouded)
        {
            if (shrouded)
            {
#if BH
                elementStack.Shroud(true);
#else
                elementStack.Token.Shroud(true);
#endif
            }
            else
            {
#if BH
                elementStack.Unshroud(true);
#else
                elementStack.Token.Unshroud(true);
#endif
            }
        }

        /// <summary>
        /// Gets the label of the element stack.
        /// </summary>
        /// <param name="elementStack">The element stack.</param>
        /// <returns>The element stack label.</returns>
        [JsonPropertyGetter("label")]
        public string GetLabel(ElementStack elementStack)
        {
            if (elementStack.Metafictional)
            {
                return elementStack.MetafictionalLabel;
            }

            return elementStack.Label;
        }

        /// <summary>
        /// Gets the description of the element stack.
        /// </summary>
        /// <param name="elementStack">The element stack.</param>
        /// <returns>The element stack description.</returns>
        [JsonPropertyGetter("description")]
        public string GetDescription(ElementStack elementStack)
        {
            if (elementStack.Metafictional)
            {
                return elementStack.MetafictionalDescription;
            }

            return elementStack.Description;
        }

        /// <summary>
        /// Gets the icon of the element stack.
        /// </summary>
        /// <param name="elementStack">The element stack.</param>
        /// <returns>The element stack icon.</returns>
        [JsonPropertyGetter("icon")]
        public string GetIcon(ElementStack elementStack)
        {
            return elementStack.Icon;
        }

        /// <summary>
        /// Gets the uniqueness group of the element stack.
        /// </summary>
        /// <param name="elementStack">The element stack.</param>
        /// <returns>The uniqueness group of the element stack.</returns>
        [JsonPropertyGetter("uniquenessGroup")]
        public string GetUniquenessGroup(ElementStack elementStack)
        {
            if (string.IsNullOrEmpty(elementStack.UniquenessGroup))
            {
                return null;
            }

            return elementStack.UniquenessGroup;
        }

        /// <summary>
        /// Determines whether the element stack decays.
        /// </summary>
        /// <param name="elementStack">The element stack.</param>
        /// <returns>True if the element stack decays, otherwise false.</returns>
        [JsonPropertyGetter("decays")]
        public bool GetDecays(ElementStack elementStack)
        {
            return elementStack.Decays;
        }

        /// <summary>
        /// Determines whether the element stack is metafictional.
        /// </summary>
        /// <param name="elementStack">The element stack.</param>
        /// <returns>True if the element stack is metafictional, otherwise false.</returns>
        [JsonPropertyGetter("metafictional")]
        public bool GetMetafictional(ElementStack elementStack)
        {
            return elementStack.Metafictional;
        }

        /// <summary>
        /// Determines whether the element stack is unique.
        /// </summary>
        /// <param name="elementStack">The element stack.</param>
        /// <returns>True if the element stack is unique, otherwise false.</returns>
        [JsonPropertyGetter("unique")]
        public bool GetUnique(ElementStack elementStack)
        {
            return elementStack.Unique;
        }
    }
}
