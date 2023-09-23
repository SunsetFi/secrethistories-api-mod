namespace SHRestAPI.Controllers
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using SecretHistories.Constants;
    using SecretHistories.Entities;
#if BH
    using SecretHistories.Infrastructure;
#elif CS
    // LocalNexus is located in Fucine in CS for some baffling reason.
    using SecretHistories.Fucine;
#endif
    using SecretHistories.UI;
    using SHRestAPI.Payloads;
    using SHRestAPI.Server;
    using SHRestAPI.Server.Attributes;
    using SHRestAPI.Server.Exceptions;

    /// <summary>
    /// Web request controller for time-related requests.
    /// </summary>
    [WebController(Path = "api/time")]
    public class TimeController
    {
        /// <summary>
        /// Gets the game speed.
        /// </summary>
        /// <param name="context">The HTTP context of the request.</param>
        /// <returns>A task resolving when the request is completed.</returns>
        /// <exception cref="ConflictException">The game is not in a running state.</exception>
        [WebRouteMethod(Method = "GET", Path = "speed")]
        public async Task GetSpeed(IHttpContext context)
        {
            var speed = await Dispatcher.RunOnMainThread(() =>
                {
                    var heart = Watchman.Get<Heart>();
                    if (!heart)
                    {
                        throw new ConflictException("The game is not in a running state.");
                    }

                    return heart.GetEffectiveGameSpeed().ToString();
                });

            await context.SendResponse(HttpStatusCode.OK, new { speed });
        }

        /// <summary>
        /// Sets the game speed.
        /// </summary>
        /// <param name="context">The HTTP context of the request.</param>
        /// <returns>A task resolving when the request is completed.</returns>
        [WebRouteMethod(Method = "POST", Path = "speed")]
        public async Task SetSpeed(IHttpContext context)
        {
            var payload = context.ParseBody<SetSpeedPayload>();
            payload.Validate();

            await Dispatcher.RunOnMainThread(() =>
            {
                if (payload.GameSpeed == SecretHistories.Enums.GameSpeed.Paused)
                {
                    // Pause the game at the user's pause level.
                    Watchman.Get<LocalNexus>().PauseGame(true);
                }
                else
                {
                    // Unpause from the user's pause level if needed.
                    Watchman.Get<LocalNexus>().UnPauseGame(true);

                    var controlEventArgs = new SpeedControlEventArgs()
                    {
                        ControlPriorityLevel = 1,
                        GameSpeed = payload.GameSpeed,
                    };
                    Watchman.Get<LocalNexus>().SpeedControlEvent.Invoke(controlEventArgs);
                }
            });

            await Settler.AwaitSettled();

            await context.SendResponse(HttpStatusCode.OK);
        }

        /// <summary>
        /// Sets the fixed beat time.
        /// </summary>
        /// <param name="context">The HTTP context of the request.</param>
        /// <returns>A task resolving when the request is completed.</returns>
        [WebRouteMethod(Method = "POST", Path = "beat")]
        public async Task ElapseFixedBeat(IHttpContext context)
        {
            var payload = context.ParseBody<PassTimePayload>();
            payload.Validate();

            await Dispatcher.RunOnMainThread(() =>
            {
                var heart = Watchman.Get<Heart>();
                heart.Beat(payload.Seconds, 0);
            });

            await Settler.AwaitSettled();

            await context.SendResponse(HttpStatusCode.OK);
        }

        /// <summary>
        /// Gets the next in-game events.
        /// </summary>
        /// <param name="context">The HTTP context of the request.</param>
        /// <returns>A task resolving when the request is completed.</returns>
        [WebRouteMethod(Method = "GET", Path = "events")]
        public async Task GetNextEvents(IHttpContext context)
        {
            var result = await Dispatcher.RunOnMainThread(() =>
            {
                return new
                {
                    nextCardDecay = GetNextCardTime().NanToNull(),
                    nextRecipeCompletion = GetNextVerbTime().NanToNull(),
                };
            });

            await context.SendResponse(HttpStatusCode.OK, result);
        }

        /// <summary>
        /// Fast forwards to the next in-game event.
        /// </summary>
        /// <param name="context">The HTTP context of the request.</param>
        /// <returns>A task resolving when the request is completed.</returns>
        [WebRouteMethod(Method = "POST", Path = "events/beat")]
        public async Task BeatNextEvent(IHttpContext context)
        {
            var payload = context.ParseBody<BeatNextEventPayload>();
            payload.Validate();

            float timeToBeat = 0f;

            await Dispatcher.RunOnMainThread(() =>
            {
                if (payload.Event == "CardDecay")
                {
                    timeToBeat = GetNextCardTime();
                }
                else if (payload.Event == "RecipeCompletion")
                {
                    timeToBeat = GetNextVerbTime();
                }
                else if (payload.Event == "Either")
                {
                    timeToBeat = Math.Min(
                        GetNextCardTime().NanToDefault(float.PositiveInfinity),
                        GetNextVerbTime().NanToDefault(float.PositiveInfinity));
                }

                if (float.IsPositiveInfinity(timeToBeat) || timeToBeat <= 0)
                {
                    throw new ConflictException("No events are available to jump to.");
                }

                var heart = Watchman.Get<Heart>();
                heart.Beat(timeToBeat, 0);
            });

            await Settler.AwaitSettled();

            await context.SendResponse(HttpStatusCode.OK, new
            {
                secondsElapsed = timeToBeat,
            });
        }

        // This code courtesy of KatTheFox
        // see: https://github.com/KatTheFox/The-Wheel/blob/main/TheWheel.cs
        private static float GetNextCardTime()
        {
            var elementStacks = Watchman.Get<HornedAxe>().GetExteriorSpheres().Where(x => x.TokenHeartbeatIntervalMultiplier > 0.0f).SelectMany(x => x.GetTokens()).Select(x => x.Payload).OfType<ElementStack>();

            var lowest = float.PositiveInfinity;
            foreach (var stack in elementStacks)
            {
                if (stack.Decays && stack.LifetimeRemaining < lowest)
                {
                    lowest = stack.LifetimeRemaining;
                }
            }

            return float.IsPositiveInfinity(lowest) ? float.NaN : lowest;
        }

        // This code courtesy of KatTheFox
        // see: https://github.com/KatTheFox/The-Wheel/blob/main/TheWheel.cs
        private static float GetNextVerbTime()
        {
            var verbList = Watchman.Get<HornedAxe>().GetRegisteredSituations();

            if (verbList.Count == 0)
            {
                return float.NaN;
            }

            float lowest = float.PositiveInfinity;
            foreach (Situation verb in verbList)
            {
                if (verb.StateIdentifier != SecretHistories.Enums.StateEnum.Ongoing)
                {
                    continue;
                }

                if (verb.TimeRemaining < lowest)
                {
                    lowest = verb.TimeRemaining;
                }
            }

            if (float.IsPositiveInfinity(lowest))
            {
                return float.NaN;
            }

            return lowest;
        }
    }
}
