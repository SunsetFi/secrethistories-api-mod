namespace SHRestAPI.Controllers
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using HarmonyLib;
    using Newtonsoft.Json.Linq;
#if BH
    using SecretHistories.Commands;
    using SecretHistories.Commands.SituationCommands;
#endif
    using SecretHistories.Entities;
    using SecretHistories.Enums;
#if BH
    using SecretHistories.Spheres;
    using SecretHistories.States;
    using SecretHistories.Tokens.Payloads;
#endif
    using SecretHistories.UI;
    using SHRestAPI.JsonTranslation;
#if BH
    using SHRestAPI.Payloads;
    using SHRestAPI.Server;
#endif
    using SHRestAPI.Server.Attributes;
    using SHRestAPI.Server.Exceptions;
    using UnityEngine;
    using static SHRestAPI.SafeFucinePath;

    /// <summary>
    /// A controller dealing with Spheres.
    /// </summary>
    [WebController(Path = "api/by-path")]
    internal class ByPathController
    {
        /// <summary>
        /// Gets all spheres at the root.
        /// </summary>
        /// <param name="context">The HTTP context of the request.</param>
        /// <returns>A task that resolves once the request is completed.</returns>
        [WebRouteMethod(Method = "GET", Path = "~/spheres")]
        public async Task GetSpheresAtRoot(IHttpContext context)
        {
            var items = await Dispatcher.DispatchRead(() =>
                (from sphere in FucineRoot.Get().GetSpheres()
                 let json = JsonTranslator.ObjectToJson(sphere)
                 select json).ToArray());

            await context.SendResponse(HttpStatusCode.OK, items);
        }

        /// <summary>
        /// Gets a token from a path.
        /// </summary>
        /// <param name="context">The HTTP context of the request.</param>
        /// <param name="path">The path to get the item at.</param>
        /// <returns>A task that resolves once the request is completed.</returns>
        [WebRouteMethod(Method = "GET", Path = "**path")]
        public async Task GetItemAtPath(IHttpContext context, string path)
        {
            var result = await Dispatcher.DispatchRead(() =>
            {
                return this.WebSafeParse(path).WithItemAtAbsolutePath(
                    token => TokenUtils.TokenToJObject(token),
                    sphere => JsonTranslator.ObjectToJson(sphere));
            });

            await context.SendResponse(HttpStatusCode.OK, result);
        }

        /// <summary>
        /// Modifies an item from a path.
        /// </summary>
        /// <param name="context">The HTTP context of the request.</param>
        /// <param name="path">The path of the item.</param>
        /// <param name="body">The new data for the item.</param>
        /// <returns>A task that resolves once the request is completed.</returns>
        [WebRouteMethod(Method = "PATCH", Path = "**path")]
        public async Task UpdateItemAtPath(IHttpContext context, string path, JObject body)
        {
            var result = await Dispatcher.DispatchWrite(() =>
            {
                return this.WebSafeParse(path).WithItemAtAbsolutePath(
                    token =>
                    {
                        TokenUtils.UpdateToken(body, token);
                        return TokenUtils.TokenToJObject(token);
                    },
                    sphere => throw new BadRequestException("Cannot update a sphere."));
            });

            await Settler.AwaitSettled();

            await context.SendResponse(HttpStatusCode.OK, result);
        }

        /// <summary>
        /// Deletes the item at the specified path.
        /// </summary>
        /// <param name="context">The HTTP context of the request.</param>
        /// <param name="path">The path of the item to delete.</param>
        /// <returns>A task that resolves once the requst is completed.</returns>
        [WebRouteMethod(Method = "DELETE", Path = "**path")]
        public async Task DeleteItemAtPath(IHttpContext context, string path)
        {
            await Dispatcher.DispatchWrite(() =>
            {
                this.WebSafeParse(path).WithItemAtAbsolutePath(
                    token => token.Retire(),
                    sphere => throw new BadRequestException("Cannot delete a sphere."));
            });

            await Settler.AwaitSettled();

            await context.SendResponse(HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Gets the icon in png format from the token.
        /// </summary>
        /// <param name="context">The HTTP context of the request.</param>
        /// <param name="path">The fucine path of the item to get the image of.</param>
        /// <returns>A task that resolves when the request is completed.</returns>
        [WebRouteMethod(Method = "GET", Path = "**path/icon.png")]
        public async Task GetPathIcon(IHttpContext context, string path)
        {
            var result = await Dispatcher.DispatchGraphicsRead(() =>
            {
                var sprite = this.WebSafeParse(path).WithItemAtAbsolutePath(
                    token =>
                    {
                        if (token.Payload is ElementStack stack)
                        {
                            return ResourcesManager.GetAppropriateSpriteForElement(stack.Element);
                        }
                        else if (token.Payload is Situation situation)
                        {
                            return ResourcesManager.GetSpriteForVerbLarge(situation.VerbId);
                        }
                        else
                        {
                            throw new BadRequestException("Cannot get icon for token payload type " + token.Payload.GetType().FullName);
                        }
                    },
                    sphere => throw new BadRequestException("Cannot get icon for sphere."));

                return sprite.ToTexture().EncodeToPNG();
            });

            await context.SendResponse(HttpStatusCode.OK, "image/png", new MemoryStream(result));
        }

        /// <summary>
        /// Get all spheres at the given path.
        /// </summary>
        /// <param name="context">The HTTP context of the request.</param>
        /// <param name="path">The path to get the spheres at.</param>
        /// <returns>A task that resolves once the request is completed.</returns>
        [WebRouteMethod(Method = "GET", Path = "**path/spheres")]
        public async Task GetSpheresAtPath(IHttpContext context, string path)
        {
            var items = await Dispatcher.DispatchRead(() =>
            {
                return this.WebSafeParse(path).WithItemAtAbsolutePath(
                    token => from child in token.Payload.GetSpheres()
                             let json = JsonTranslator.ObjectToJson(child)
                             select json,
                    sphere => throw new BadRequestException("Cannot get spheres of a sphere."));
            });

            await context.SendResponse(HttpStatusCode.OK, items);
        }

        /// <summary>
        /// Gets all tokens at the given path.
        /// </summary>
        /// <param name="context">The HTTP context of the request.</param>
        /// <param name="path">The path of the sphere to get tokens for.</param>
        /// <returns>A task that resolves once the request is completed.</returns>
        /// <exception cref="NotFoundException">The sphere was not found.</exception>
        [WebRouteMethod(Method = "GET", Path = "**path/tokens")]
        public async Task GetTokensAtPath(IHttpContext context, string path)
        {
            context.QueryString.TryGetValue("payloadType", out var payloadType);
            context.QueryString.TryGetValue("entityId", out var entityId);

            var items = await Dispatcher.DispatchRead(() =>
            {
                return this.WebSafeParse(path).WithItemAtAbsolutePath(
                    token => throw new BadRequestException("Cannot get tokens of a token."),
                    sphere => from token in sphere.Tokens
                              where !token.Defunct
                              where payloadType == null || token.PayloadTypeName == payloadType
                              where entityId == null || token.PayloadEntityId == entityId
                              let json = TokenUtils.TokenToJObject(token)
                              select json);
            });

            await context.SendResponse(HttpStatusCode.OK, items);
        }

        /// <summary>
        /// Creates a token in the sphere.
        /// </summary>
        /// <param name="context">The HTTP context of the request.</param>
        /// <param name="path">The path of the sphere.</param>
        /// <returns>A task that resolves once the request is completed.</returns>
        /// <exception cref="NotFoundException">The sphere was not found.</exception>
        /// <exception cref="BadRequestException">The request was malformed.</exception>
        [WebRouteMethod(Method = "POST", Path = "**path/tokens")]
        public async Task CreateTokenAtPath(IHttpContext context, string path)
        {
            var result = await Dispatcher.DispatchWrite(() =>
            {
                return this.WebSafeParse(path).WithItemAtAbsolutePath(
                    token => throw new BadRequestException("Cannot create tokens in a token."),
                    sphere =>
                    {
                        var body = context.ParseBody<JToken>();

                        if (body is JArray)
                        {
                            var items = new List<JToken>();
                            foreach (var item in body)
                            {
                                items.Add(TokenUtils.CreateSphereToken(sphere, item as JObject));
                            }

                            return JArray.FromObject(items.ToArray());
                        }
                        else if (body is JObject jObj)
                        {
                            return TokenUtils.CreateSphereToken(sphere, jObj);
                        }

                        throw new BadRequestException("Invalid request body, must be object or array.");
                    });
            });

            await Settler.AwaitSettled();

            await context.SendResponse(HttpStatusCode.Created, result);
        }

        /// <summary>
        /// Deletes all tokens in the sphere.
        /// </summary>
        /// <param name="context">The request context.</param>
        /// <param name="path">The path of the sphere.</param>
        /// <returns>A task that resolves once the request is completed.</returns>
        /// <exception cref="NotFoundException">The sphere was not found.</exception>
        [WebRouteMethod(Method = "DELETE", Path = "**path/tokens")]
        public async Task DeleteAllTokensAtPath(IHttpContext context, string path)
        {
            await Dispatcher.RunOnMainThread(() =>
            {
                var parsed = SafeFucinePath.WebSafeParse(path);
                if (!parsed.TargetSphere)
                {
                    throw new BadRequestException("Cannot delete tokens of a token.");
                }

                var sphere = parsed.TargetSphere;
                if (sphere == null || !sphere.IsValid())
                {
                    throw new NotFoundException($"No sphere found at path \"{path}\".");
                }

                foreach (var token in sphere.Tokens.ToArray())
                {
                    // Don't delete things we dont care about.
                    // Now that Book of Hours is around, there are a lot more possibilities (workstations, terrains, ....),
                    // and none of them should be deleted.
                    // Note that this is quite dangerous in Book of Hours anyway, as critical things like our time passing are situations.
                    if (token.Payload is ElementStack || token.Payload is Situation)
                    {
                        token.Retire();
                    }
                }
            });
            await Settler.AwaitSettled();
            await context.SendResponse(HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Evict the token at the given path.
        /// </summary>
        /// <param name="context">The HTTP context of the request.</param>
        /// <param name="path">The path of the token to evict.</param>
        /// <returns>A task that resolves once the request is completed.</returns>
        [WebRouteMethod(Method = "POST", Path = "**path/evict")]
        public async Task EvictItemAtPath(IHttpContext context, string path)
        {
            await Dispatcher.DispatchWrite(() =>
            {
                var parsedPath = this.WebSafeParse(path);

                var token = parsedPath.TargetToken;
                if (token != null)
                {
                    // TODO: We should be smarter about when we can and cannot evict.
                    // Dont evict from tabletop, portage, or the player's hand.
                    token.Sphere.EvictToken(token, new Context(Context.ActionSource.UI));
                }

                var sphere = parsedPath.TargetSphere;
                if (sphere != null)
                {
                    sphere.EvictAllTokens(new Context(Context.ActionSource.UI));
                }
            });

            await Settler.AwaitSettled();
            await context.SendResponse(HttpStatusCode.NoContent);
        }

#if BH
        /// <summary>
        /// Focuses the item at the given path.
        /// </summary>
        /// <param name="context">The HTTP context of the request.</param>
        /// <param name="path">The path of the item to focus.</param>
        /// <returns>A task that resolves once the request is completed.</returns>
        [WebRouteMethod(Method = "POST", Path = "**path/focus")]
        public async Task FocusItemAtPath(IHttpContext context, string path)
        {
            await Dispatcher.DispatchWrite(async () =>
            {
                var token = this.WebSafeParse(path).GetToken();
                if (token == null)
                {
                    throw new NotFoundException($"No token found at path \"{path}\".");
                }

                await BHCamera.FocusToken(token);
            });

            await context.SendResponse(HttpStatusCode.NoContent);
        }
#endif

        /// <summary>
        /// Opens the item at the given path.
        /// </summary>
        /// <param name="context">The HTTP context of the request.</param>
        /// <param name="path">The path of the item to open.</param>
        /// <returns>A task that resolves once the request is completed.</returns>
        [WebRouteMethod(Method = "POST", Path = "**path/open")]
        public async Task OpenItemAtPath(IHttpContext context, string path)
        {
            await Dispatcher.DispatchWrite(() =>
            {
                var token = this.WebSafeParse(path).GetToken();
                if (token == null)
                {
                    throw new NotFoundException($"No token found at path \"{path}\".");
                }

                if (token.Payload is Situation situation)
                {
                    situation.OpenAt(situation.Token.Location);
                }
#if BH
                else if (token.Payload is ConnectedTerrain terrain)
                {
                    if (!terrain.IsShrouded)
                    {
                        throw new ConflictException("Cannot open detail window for terrain that is already unlocked.");
                    }

                    var screenPoint = RectTransformUtility.WorldToScreenPoint(Camera.main, token.gameObject.transform.position);
                    Watchman.Get<TerrainDetailWindow>().Show((Vector3)screenPoint, terrain);
                }
#endif
                else
                {
                    throw new NotFoundException($"Token payload \"{token.PayloadTypeName}\" is not openable.");
                }
            });

            await Settler.AwaitSettled();

            await context.SendResponse(HttpStatusCode.NoContent);
        }

#if BH
        [WebRouteMethod(Method = "GET", Path = "**path/ambit-recipes")]
        public async Task GetAmbitRecipesAtPath(IHttpContext context, string path)
        {
            var payload = await Dispatcher.DispatchRead(() =>
            {
                var token = this.WebSafeParse(path).GetToken();
                if (token == null || token.Payload is not Situation situation)
                {
                    throw new NotFoundException($"No situation found at path \"{path}\".");
                }

                if (situation.State.Identifier != StateEnum.Unstarted)
                {
                    throw new ConflictException("Can only fetch ambit recipes for unstarted situations.");
                }

                var open = situation.CurrentOpenAmbitRelevantRecipes();
                var locked = situation.CurrentLockedAmbitRelevantRecipes();

                return new AmbitRecipesPayload
                {
                    OpenAmbitRecipeIds = open.Select(x => x.Id).ToList(),
                    LockedAmbitRecipeIds = locked.Select(x => x.Id).ToList(),
                };
            });

            await context.SendResponse(HttpStatusCode.OK, payload);
        }
#endif

        /// <summary>
        /// Sets the recipe of the unstarted situation at the given path.
        /// </summary>
        /// <param name="context">The HTTP context of the request.</param>
        /// <param name="path">The path of the situation token.</param>
        /// <param name="body">The payload of the request.</param>
        /// <returns>A task that resolves once the request is completed.</returns>
        [WebRouteMethod(Method = "POST", Path = "**path/recipe")]
        public async Task SetRecipeAtPath(IHttpContext context, string path, SetRecipePayload body)
        {
            await Dispatcher.DispatchWrite(() =>
            {
                var token = this.WebSafeParse(path).GetToken();
                if (token == null || token.Payload is not Situation situation)
                {
                    throw new NotFoundException($"No situation found at path \"{path}\".");
                }

                var hasRecipe = !string.IsNullOrEmpty(body.RecipeId);
                Recipe recipe = null;
                if (hasRecipe)
                {
                    recipe = Watchman.Get<Compendium>().GetEntityById<Recipe>(body.RecipeId);
                    if (recipe == null || !recipe.IsValid())
                    {
                        throw new BadRequestException($"No recipe found with id \"{body.RecipeId}\".");
                    }
                }

                if (body.RequireState.HasValue && situation.StateIdentifier != body.RequireState.Value)
                {
                    throw new ConflictException($"Situation {situation.VerbId} is not in the correct state to set a recipe.");
                }

                if (situation.StateIdentifier == StateEnum.Unstarted)
                {
                    if (hasRecipe)
                    {
                        situation.OverrideCurrentRecipeForUnstarted(recipe);
                    }
                    else
                    {
                        situation.TryRevertToOriginalRecipe();
                    }
                }
                else if (situation.StateIdentifier == StateEnum.Ongoing)
                {
                    var nullRecipe = NullRecipe.Create();

                    situation.State = SituationState.Rehydrate(StateEnum.Unstarted, situation);
                    situation.SetRecipeActive(nullRecipe);
                    situation.SetCurrentRecipe(nullRecipe);

                    if (hasRecipe)
                    {
                        situation.CommandQueue.RemoveAll(x => x is TryActivateRecipeCommand);
                        situation.CommandQueue.Add(new TryActivateRecipeCommand(recipe.Id));
                    }
                }
                else
                {
                    throw new ConflictException($"Situation {situation.VerbId} is not in the correct state to set a recipe.");
                }
            });

            await context.SendResponse(HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Executes the recipe of the situation at the given path.
        /// </summary>
        /// <param name="context">The HTTP context of the request.</param>
        /// <param name="path">The path of the situation token.</param>
        /// <returns>A task that resolves once the request is completed.</returns>
        [WebRouteMethod(Method = "POST", Path = "**path/execute")]
        public async Task ExecuteSituationAtPath(IHttpContext context, string path)
        {
            var result = await Dispatcher.DispatchWrite(() =>
            {
                var token = this.WebSafeParse(path).GetToken();
                if (token == null)
                {
                    throw new NotFoundException($"No token found at path \"{path}\".");
                }

                if (token.Payload is Situation situation)
                {
                    if (situation.StateIdentifier != StateEnum.Unstarted)
                    {
                        throw new ConflictException($"Situation {situation.VerbId} is not in the correct state to begin a recipe.");
                    }

                    situation.TryStart();
                    if (situation.StateIdentifier == StateEnum.Unstarted)
                    {
                        throw new ConflictException($"Situation {situation.VerbId} could not begin it's recipe.");
                    }

                    var fallbackRecipe = situation.GetFallbackRecipe();
                    return new
                    {
                        executedRecipeId = fallbackRecipe.Id,
                        executedRecipeLabel = fallbackRecipe.Label,
                        timeRemaining = situation.TimeRemaining,
                    };
                }
#if BH
                else if (token.Payload is ConnectedTerrain terrain)
                {
                    var detailWindow = Watchman.Get<TerrainDetailWindow>();

                    // Reimplementation of TryOpenTerrain, reimplemented so we can get the created situation and throw errors.
                    var inputSphere = typeof(TerrainDetailWindow).GetField("inputSphere", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(detailWindow) as Sphere;
                    if (inputSphere.IsEmpty() || terrain.IsSealed)
                    {
                        throw new ConflictException("Cannot unlock terrain that is sealed or has no input.");
                    }

                    inputSphere.Tokens.First().Retire(RetirementVFX.None);
                    var terrainSphere = terrain.Token.Sphere;
                    var infoRecipe = terrain.GetInfoRecipe();
                    var sphereSpace = terrain.Token.Sphere.TransformWorldPositionToSphereSpace(terrain.GetPositionForUnlockToken());
                    var unlockSituationToken = new TokenCreationCommand(
                        new SituationCreationCommand("terrain.unlock").WithRecipeAboutToActivate(infoRecipe.Id),
                        new TokenLocation(sphereSpace, terrainSphere)).Execute(global::Context.Unknown(), terrainSphere);
                    detailWindow.Hide();

                    var unlockSituation = unlockSituationToken.Payload as Situation;
                    var fallbackRecipe = unlockSituation.GetFallbackRecipe();
                    return new
                    {
                        executedRecipeId = fallbackRecipe.Id,
                        executedRecipeLabel = fallbackRecipe.Label,
                        timeRemaining = unlockSituation.TimeRemaining,
                    };
                }
                else if (token.Payload is WisdomNodeTerrain wisdom)
                {
                    var inputSphere = Traverse.Create(wisdom).Field<Sphere>("inputSphere").Value;
                    var inputToken = inputSphere.Tokens.FirstOrDefault();
                    if (inputToken == null)
                    {
                        throw new ConflictException("Cannot commit to a wisdom node without an input.");
                    }

                    var commitRecipe = wisdom.GetCurrentCommitmentRecipe();
                    if (commitRecipe == null || !commitRecipe.IsValid())
                    {
                        throw new ConflictException("Wisdom tree node cannot accept the current input.");
                    }

                    // The token arg is actually not used, but we reproduce the game's behavior here anyway.
                    wisdom.TryCommitToken(inputToken);

                    // Check to see if we succeeded.
                    var commitmentSphere = Traverse.Create(wisdom).Field<CommitmentSphere>("commitmentSphere").Value;
                    if (!commitmentSphere.Tokens.Any())
                    {
                        throw new ConflictException("Wisdom tree node could not commit to the input.");
                    }

                    return new
                    {
                        executedRecipeId = commitRecipe.Id,
                        executedRecipeLabel = commitRecipe.Label,
                        timeRemaining = 0.0f,
                    };
                }
#endif
                else
                {
                    throw new NotFoundException($"Token payload \"{token.PayloadTypeName}\" is not executable.");
                }
            });

            await Settler.AwaitSettled();

            await context.SendResponse(HttpStatusCode.OK, result);
        }

        /// <summary>
        /// Concludes the situation at the given path.
        /// </summary>
        /// <param name="context">The HTTP context of the request.</param>
        /// <param name="path">The path of the situation token.</param>
        /// <returns>A task that resolves once the request is completed.</returns>
        [WebRouteMethod(Method = "POST", Path = "**path/conclude")]
        public async Task ConcludeSituationAtPath(IHttpContext context, string path)
        {
            var result = await Dispatcher.DispatchWrite(() =>
            {
                var situation = this.WebSafeParse(path).GetPayload<Situation>();
                if (situation == null)
                {
                    throw new NotFoundException($"No situation found at path \"{path}\".");
                }

                if (situation.StateIdentifier != StateEnum.Complete)
                {
                    throw new ConflictException($"Situation {situation.VerbId} is not in the correct state to conclude.");
                }

                var outputResult = (from sphere in situation.GetSpheresByCategory(SphereCategory.Output)
                                    from token in sphere.Tokens
                                    let json = TokenUtils.TokenToJObject(token)
                                    select json).ToArray();

                situation.Conclude();

                return outputResult;
            });

            await Settler.AwaitSettled();

            await context.SendResponse(HttpStatusCode.OK, result);
        }

#if BH
        /// <summary>
        /// Starts the unlock process of the terrain at the given path.
        /// </summary>
        /// <param name="context">The HTTP context of the request.</param>
        /// <param name="path">The path of the terrain token.</param>
        /// <param name="body">The body of the request.</param>
        /// <returns>A task that resolves once the request is completed.</returns>
        [WebRouteMethod(Method = "POST", Path = "**path/unlock")]
        public async Task UnlockTerrainAtPath(IHttpContext context, string path, MaybeInstantPayload body)
        {
            await Dispatcher.DispatchWrite(() =>
            {
                var terrain = this.WebSafeParse(path).GetPayload<ConnectedTerrain>();
                if (terrain == null)
                {
                    throw new NotFoundException($"No terrain found at path \"{path}\".");
                }

                if (!terrain.IsShrouded)
                {
                    return;
                }

                if (terrain.IsSealed)
                {
                    terrain.Unseal();
                }

                if (body.Instant)
                {
                    terrain.Unshroud(true);
                }
                else
                {
                    // From TerrainDetailWindow.TryOpenTerrain
                    var sphere = terrain.Token.Sphere;
                    var sphereSpace = sphere.TransformWorldPositionToSphereSpace(terrain.GetPositionForUnlockToken());
                    var command = new TokenCreationCommand(
                        new SituationCreationCommand("terrain.unlock").WithRecipeAboutToActivate(terrain.GetInfoRecipe().Id),
                        new TokenLocation(sphereSpace, sphere));
                    command.Execute(global::Context.Unknown(), sphere);
                }
            });

            await Settler.AwaitSettled();

            await context.SendResponse(HttpStatusCode.NoContent);
        }
#endif

        private SafeFucinePath WebSafeParse(string path)
        {
            try
            {
                return new SafeFucinePath(path);
            }
            catch (PathElementNotFoundException)
            {
                throw new NotFoundException($"No item found at path \"{path}\".");
            }
            catch (SafeFucinePathException)
            {
                throw new BadRequestException($"Invalid path \"{path}\".");
            }
        }
    }
}
