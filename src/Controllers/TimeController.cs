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
        private readonly float MinimumHeartbeatInterval = 0.1f;

        /// <summary>
        /// Gets the game speed.
        /// </summary>
        /// <param name="context">The HTTP context of the request.</param>
        /// <returns>A task resolving when the request is completed.</returns>
        /// <exception cref="ConflictException">The game is not in a running state.</exception>
        [WebRouteMethod(Method = "GET", Path = "speed")]
        public async Task GetSpeed(IHttpContext context)
        {
            var speed = await Dispatcher.DispatchRead(() =>
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

            await Dispatcher.DispatchWrite(() =>
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

            await Dispatcher.DispatchWrite(() =>
            {
                var heart = Watchman.Get<Heart>();

                // I strongly suspect the game does not reprocess the next recipe timer if an event completes early
                // in a beat.
                // To work around this, we split the beats between interesting events to keep the game
                // operating as intended when skipping large amounts of time.
                var timeRemaining = payload.Seconds;
                while (timeRemaining > 0)
                {
                    Logging.LogInfo($"Beat check.  Next card time {GetNextCardTime()}, next verb time {GetNextVerbTime()}");

                    var nextEvent = Math.Min(
                        GetNextCardTime().NanToDefault(float.PositiveInfinity),
                        GetNextVerbTime().NanToDefault(float.PositiveInfinity));

                    Logging.LogInfo($"Beat time remaining: {timeRemaining} next event {nextEvent}");
                    var skip = Math.Min(timeRemaining, nextEvent);

                    // Heart usually enforces this interval, but before we get to Beat()
                    if (skip < this.MinimumHeartbeatInterval)
                    {
                        skip = this.MinimumHeartbeatInterval;
                    }

                    Logging.LogInfo($"Beating for {skip}");

                    heart.Beat(skip, this.MinimumHeartbeatInterval);

                    timeRemaining -= skip;
                    Logging.LogInfo($"Time remaining is now {timeRemaining}");
                }
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
            var result = await Dispatcher.DispatchRead(() =>
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

            await Dispatcher.DispatchWrite(() =>
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

                // Heart usually enforces this interval, but before we get to Beat()
                if (timeToBeat < this.MinimumHeartbeatInterval)
                {
                    timeToBeat = this.MinimumHeartbeatInterval;
                }

                var heart = Watchman.Get<Heart>();
                heart.Beat(timeToBeat, this.MinimumHeartbeatInterval);
            });

            await Settler.AwaitSettled();

            await context.SendResponse(HttpStatusCode.OK, new
            {
                secondsElapsed = timeToBeat,
            });
        }

        // This code based on The Wheel, by KatTheFox
        // see: https://github.com/KatTheFox/The-Wheel/blob/main/TheWheel.cs
        private static float GetNextCardTime()
        {
            var elementStacks = from sphere in Watchman.Get<HornedAxe>().GetExteriorSpheres()
                                where sphere.TokenHeartbeatIntervalMultiplier > 0.0f
                                from token in sphere.GetTokens()
                                let payload = token.Payload
                                let stack = payload as ElementStack
                                where stack != null && stack.Decays
                                // TODO: If our sphere has TokenHeartbeatIntervalMultiplier, apply it.
                                orderby stack.LifetimeRemaining ascending
                                select stack;

            var found = elementStacks.FirstOrDefault();

            if (found == null)
            {
                return float.NaN;
            }

            Logging.LogInfo("Lowest card", found.Element.Id);
            return found.LifetimeRemaining;
        }

        // This code courtesy of KatTheFox
        // see: https://github.com/KatTheFox/The-Wheel/blob/main/TheWheel.cs
        private static float GetNextVerbTime()
        {
            var verbList = Watchman.Get<HornedAxe>().GetRegisteredSituations();
            Situation lowestVerb = null;

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
                    lowestVerb = verb;
                }
            }

            if (float.IsPositiveInfinity(lowest))
            {
                return float.NaN;
            }

            Logging.LogInfo($"Verb {lowestVerb.VerbId} has time remaining {lowestVerb.TimeRemaining}");

            return lowest;
        }
    }
}
