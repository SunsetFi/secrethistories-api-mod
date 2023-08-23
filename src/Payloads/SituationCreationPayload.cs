namespace SHRestAPI.Payloads
{
    using SecretHistories.Commands;
    using SecretHistories.Entities;
    using SecretHistories.Spheres;
    using SecretHistories.UI;
    using SHRestAPI.Server.Exceptions;

    /// <summary>
    /// A payload for creating a situation.
    /// </summary>
    public class SituationCreationPayload
    {
        /// <summary>
        /// Gets or sets the ID of the verb to create a situation of.
        /// </summary>
        public string VerbId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the recipe to start the situation with.
        /// </summary>
        public string RecipeId { get; set; }

        /// <summary>
        /// Validates the payload.
        /// </summary>
        /// <exception cref="BadRequestException">The payload is invalid.</exception>
        public void Validate()
        {
            if (string.IsNullOrEmpty(this.RecipeId) && string.IsNullOrEmpty(this.VerbId))
            {
                throw new BadRequestException("Either verbId or recipeId must be supplied.");
            }

            if (!string.IsNullOrEmpty(this.RecipeId))
            {
                var recipe = Watchman.Get<Compendium>().GetEntityById<Recipe>(this.RecipeId);
                if (!recipe.IsValid())
                {
                    throw new BadRequestException($"RecipeId {this.RecipeId} does not exist.");
                }

                if (!string.IsNullOrEmpty(this.VerbId))
                {
                    if (this.VerbId != recipe.ActionId)
                    {
                        throw new BadRequestException($"VerbId {this.VerbId} does not match recipe verb {recipe.ActionId} for recipe {this.RecipeId}");
                    }
                }
            }
            else
            {
                var verb = Watchman.Get<Compendium>().GetEntityById<Verb>(this.VerbId);
                if (!verb.IsValid())
                {
                    throw new BadRequestException($"VerbId {this.VerbId} does not exist.");
                }
            }
        }

        /// <summary>
        /// Creates a situation from the payload.
        /// </summary>
        /// <param name="sphere">The sphere to create the situation in.</param>
        /// <returns>The created token.</returns>
        /// <exception cref="BadRequestException">The request was invalid.</exception>
        public Token Create(Sphere sphere)
        {
            if (sphere is TabletopSphere == false)
            {
                throw new BadRequestException("Situations can only be created in a tabletop sphere");
            }

            var cmd = new SituationCreationCommand();
            if (!string.IsNullOrEmpty(this.RecipeId))
            {
                var recipe = Watchman.Get<Compendium>().GetEntityById<Recipe>(this.RecipeId);
                cmd.WithVerbId(recipe.ActionId).WithRecipeAboutToActivate(recipe.Id);
            }
            else if (!string.IsNullOrEmpty(this.VerbId))
            {
                cmd.WithVerbId(this.VerbId);
            }
            else
            {
                throw new BadRequestException("VerbId or RecipeId is required");
            }

            var location = new TokenLocation(0.0f, 0.0f, 0.0f, sphere.GetAbsolutePath());
            var token = new TokenCreationCommand(cmd, location).Execute(new Context(Context.ActionSource.Debug), sphere);
            if (!token.IsValid())
            {
                throw new InternalServerErrorException($"Failed to create situation {this.VerbId}");
            }

            return token;
        }
    }
}
