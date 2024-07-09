namespace SHRestAPI.Payloads
{
    using System.Collections.Generic;
    using System.Linq;
    using HarmonyLib;
    using Newtonsoft.Json.Linq;
    using SecretHistories.Commands;
    using SecretHistories.Commands.SituationCommands;
    using SecretHistories.Entities;
    using SecretHistories.Enums;
    using SecretHistories.Spheres;
    using SecretHistories.States;
    using SecretHistories.UI;
    using SHRestAPI.JsonTranslation;
    using SHRestAPI.Server.Exceptions;

    /// <summary>
    /// Translation strategy for Situations.
    /// </summary>
    [JsonTranslatorStrategy]
    [JsonTranslatorTarget(typeof(Situation))]
    public class SituationTranslationStrategy
    {
        /// <summary>
        /// Gets the verb id of this situation.
        /// </summary>
        /// <param name="situation">The situation.</param>
        /// <returns>The verb id.</returns>
        [JsonPropertyGetter("verbId")]
        public string GetVerbId(Situation situation)
        {
            return situation.VerbId;
        }

        /// <summary>
        /// Gets a value indicating if this spontaneous is a spontanious verb.
        /// </summary>
        /// <param name="situation">The situation.</param>
        /// <returns>A value indicating if this situation is spontaneous.</returns>
        [JsonPropertyGetter("spontaneous")]
        public bool GetSpontanious(Situation situation)
        {
            return situation.Verb.Spontaneous;
        }

        /// <summary>
        /// Gets all aspects native to this verb.
        /// </summary>
        /// <param name="situation">The situation.</param>
        /// <returns>The situation aspects.</returns>
        [JsonPropertyGetter("aspects")]
        public JObject GetAspects(Situation situation)
        {
            var value = new JObject();
            foreach (var pair in situation.Verb.Aspects)
            {
                value.Add(pair.Key, pair.Value);
            }

            return value;
        }

        /// <summary>
        /// Gets the aspect hints for this situation.
        /// </summary>
        /// <param name="situation">The situation.</param>
        /// <returns>The aspect hints.</returns>
        [JsonPropertyGetter("hints")]
        public string[] GetHints(Situation situation)
        {
            return situation.Verb.Hints.ToArray();
        }

        /// <summary>
        /// Gets all input thresholds of this situation.
        /// </summary>
        /// <param name="situation">The situation.</param>
        /// <returns>The input thresholds.</returns>
        [JsonPropertyGetter("thresholds")]
        public JObject[] GetThresholds(Situation situation)
        {
            if (situation.StateIdentifier == StateEnum.Ongoing)
            {
                return situation.GetDominion(SituationDominionEnum.RecipeThresholds).Spheres.Select(x => x.GoverningSphereSpec).Select(JsonTranslator.ObjectToJson).ToArray();
            }
            else if (situation.StateIdentifier == StateEnum.Unstarted)
            {
                return situation.GetDominion(SituationDominionEnum.VerbThresholds).Spheres.Select(x => x.GoverningSphereSpec).Select(JsonTranslator.ObjectToJson).ToArray();
            }
            else
            {
                return new JObject[0];
            }
        }

        /// <summary>
        /// Gets the current threshold content token ids of this situation by threshold sphere id.
        /// </summary>
        /// <param name="situation">The situation.</param>
        /// <returns>The threshold contents.</returns>
        [JsonPropertyGetter("thresholdContents")]
        public Dictionary<string, string> GetThresholdContents(Situation situation)
        {
            IList<Sphere> spheres;
            if (situation.StateIdentifier == StateEnum.Ongoing)
            {
                spheres = situation.GetDominion(SituationDominionEnum.RecipeThresholds).Spheres;
            }
            else if (situation.StateIdentifier == StateEnum.Unstarted)
            {
                spheres = situation.GetDominion(SituationDominionEnum.VerbThresholds).Spheres;
            }
            else
            {
                return new Dictionary<string, string>();
            }

            return spheres.ToDictionary(x => x.GoverningSphereSpec.Id, x => x.Tokens.FirstOrDefault()?.PayloadId);
        }

#if BH
        [JsonPropertyGetter("openAmbitRecipeIds")]
        public List<string> GetOpenAmbitRecipeIds(Situation situation)
        {
            return situation.CurrentOpenAmbitRelevantRecipes().Select(x => x.Id).ToList();
        }

        [JsonPropertyGetter("lockedAmbitRecipeIds")]
        public List<string> GetLockedAmbitRecipeIds(Situation situation)
        {
            return situation.CurrentLockedAmbitRelevantRecipes().Select(x => x.Id).ToList();
        }
#endif

        /// <summary>
        /// Gets the time remaining in the situation's current recipe.
        /// </summary>
        /// <param name="situation">The situation.</param>
        /// <returns>The time remaining.</returns>
        [JsonPropertyGetter("timeRemaining")]
        public float GetTimeRemaining(Situation situation)
        {
            return situation.TimeRemaining;
        }

        /// <summary>
        /// Gets the recipe ID of the situation's fallback recipe.
        /// </summary>
        /// <param name="situation">The situation.</param>
        /// <returns>The fallback recipe ID.</returns>
        [JsonPropertyGetter("recipeId")]
        public string GetFallbackRecipeId(Situation situation)
        {
            var fallbackRecipe = situation.GetFallbackRecipe();
            if (fallbackRecipe == null || !fallbackRecipe.IsValid())
            {
                return null;
            }

            return fallbackRecipe.Id;
        }

        /// <summary>
        /// Sets the fallback recipe ID of the situation.
        /// </summary>
        /// <param name="situation">The situation.</param>
        /// <param name="value">The recipe id.</param>
        [JsonPropertySetter("recipeId")]
        public void SetFallbackRecipeId(Situation situation, string value)
        {
            var hasRecipe = !string.IsNullOrEmpty(value);

            var recipe = hasRecipe ? Watchman.Get<Compendium>().GetEntityById<Recipe>(value) : null;
            if (hasRecipe && !recipe.IsValid())
            {
                throw new BadRequestException($"Recipe ID {value} not found.");
            }

            if (situation.StateIdentifier == StateEnum.Ongoing)
            {
                var nullRecipe = NullRecipe.Create();

                situation.State = SituationState.Rehydrate(StateEnum.Unstarted, situation);
                situation.SetRecipeActive(nullRecipe);
                situation.SetCurrentRecipe(nullRecipe);
            }
            else if (situation.StateIdentifier != StateEnum.Unstarted)
            {
                throw new ConflictException($"Cannot set fallback recipe ID when situation is in state {situation.StateIdentifier}.");
            }

            if (!hasRecipe)
            {
                return;
            }

            situation.CommandQueue.RemoveAll(x => x is TryActivateRecipeCommand);
            situation.CommandQueue.Add(new TryActivateRecipeCommand(recipe.Id));
        }

        /// <summary>
        /// Gets the fallback recipe label.
        /// </summary>
        /// <param name="situation">The situation.</param>
        /// <returns>The fallback recipe label.</returns>
        [JsonPropertyGetter("recipeLabel")]
        public string GetFallbackRecipeName(Situation situation)
        {
            var fallbackRecipe = situation.GetFallbackRecipe();
            if (fallbackRecipe == null || !fallbackRecipe.IsValid())
            {
                return null;
            }

            return fallbackRecipe.Label;
        }

        /// <summary>
        /// Gets the recipe ID of the situation's current recipe.
        /// </summary>
        /// <param name="situation">The situation.</param>
        /// <returns>The current recipe ID.</returns>
        [JsonPropertyGetter("currentRecipeId")]
        public string GetCurrentRecipeId(Situation situation)
        {
            var currentRecipe = situation.GetCurrentRecipe();
            if (currentRecipe == null || !currentRecipe.IsValid())
            {
                return null;
            }

            return currentRecipe.Id;
        }

        /// <summary>
        /// Sets the current recipe ID of the situation.
        /// </summary>
        /// <param name="situation">The situation.</param>
        /// <param name="recipeId">The recipe id.</param>
        [JsonPropertySetter("currentRecipeId")]
        public void SetCurrentRecipeId(Situation situation, string recipeId)
        {
            var hasRecipe = !string.IsNullOrEmpty(recipeId);
            var recipe = hasRecipe ? Watchman.Get<Compendium>().GetEntityById<Recipe>(recipeId) : null;
            if (hasRecipe && !recipe.IsValid())
            {
                throw new BadRequestException($"Recipe ID {recipeId} not found.");
            }

            if (situation.State.Identifier != StateEnum.Unstarted)
            {
                throw new ConflictException("Cannot set current recipe ID when situation is not idle.");
            }

            if (recipe != null)
            {
                situation.OverrideCurrentRecipeForUnstarted(recipe);
            }
            else
            {
                situation.TryRevertToOriginalRecipe();
            }
        }

        /// <summary>
        /// Gets the current recipe label.
        /// </summary>
        /// <param name="situation">The situation.</param>
        /// <returns>The current recipe label.</returns>
        [JsonPropertyGetter("currentRecipeLabel")]
        public string GetCurrentRecipeLabel(Situation situation)
        {
            var currentRecipe = situation.GetCurrentRecipe();
            if (currentRecipe == null || !currentRecipe.IsValid())
            {
                return null;
            }

            return currentRecipe.Label;
        }

        /// <summary>
        /// Gets the situation's state.
        /// </summary>
        /// <param name="situation">The situation.</param>
        /// <returns>The situation's state.</returns>
        [JsonPropertyGetter("state")]
        public string GetState(Situation situation)
        {
            return situation.StateIdentifier.ToString();
        }

        /// <summary>
        /// Gets whether the situation can start given its current configuration.
        /// </summary>
        /// <param name="situation">The situation.</param>
        /// <returns>True if the situation can execute, or False if not cannot.</returns>
        [JsonPropertyGetter("canExecute")]
        public bool GetCanExecute(Situation situation)
        {
            if (situation.StateIdentifier != StateEnum.Unstarted)
            {
                return false;
            }

            // FIXME: GetAspectsInContext is not thread safe!
            // Disabling multithreaded reads for now.
            var aspectsInContext = ThreadSafeHornedAxe.GetAspectsInContext(situation);
            return situation.GetCurrentRecipe().Craftable && situation.GetCurrentRecipe().CanExecuteInContext(aspectsInContext);
        }

        /// <summary>
        /// Gets the situation's icon.
        /// </summary>
        /// <param name="situation">The situation.</param>
        /// <returns>The icon.</returns>
        [JsonPropertyGetter("icon")]
        public string GetIcon(Situation situation)
        {
            return situation.Icon;
        }

        /// <summary>
        /// Gets the situation's label.
        /// </summary>
        /// <param name="situation">The situation.</param>
        /// <returns>The label.</returns>
        [JsonPropertyGetter("label")]
        public string GetLabel(Situation situation)
        {
            return situation.Label;
        }

        /// <summary>
        /// Gets the situation's description.
        /// </summary>
        /// <param name="situation">The situation.</param>
        /// <returns>The description.</returns>
        [JsonPropertyGetter("description")]
        public string GetDescription(Situation situation)
        {
            return situation.Description;
        }

        /// <summary>
        /// Gets the verb label of this situation.
        /// </summary>
        /// <param name="situation">The sittuation.</param>
        /// <returns>The verb label.</returns>
        [JsonPropertyGetter("verbLabel")]
        public string GetVerbLabel(Situation situation)
        {
            return situation.Verb.Label;
        }

        /// <summary>
        /// Gets the verb description of this situation.
        /// </summary>
        /// <param name="situation">The situation.</param>
        /// <returns>The verb description.</returns>
        [JsonPropertyGetter("verbDescription")]
        public string GetVerbDescription(Situation situation)
        {
#if BH
            return situation.Verb.Desc;
#else
            return situation.Verb.Description;
#endif
        }

        /// <summary>
        /// Gets a value indicating if the situation is open.
        /// </summary>
        /// <param name="situation">The situation.</param>
        /// <returns>A value indicating if the situation is open.</returns>
        [JsonPropertyGetter("open")]
        public bool GetIsOpen(Situation situation)
        {
            return situation.IsOpen;
        }

        /// <summary>
        /// Sets if the situation is open.
        /// </summary>
        /// <param name="situation">The situation.</param>
        /// <param name="value">True to open the situation, False to close it.</param>
        [JsonPropertySetter("open")]
        public void SetIsOpen(Situation situation, bool value)
        {
            if (value)
            {
                // Note: In book of hours, situations tend to be wildly offscreen.
                // Sometimes, the opening situation window gets constrained to the screen,
                // sometimes not...
                situation.OpenAt(situation.Token.Location);
            }
            else
            {
                situation.Close();
            }
        }
    }
}
