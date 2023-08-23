namespace SHRestAPI.JsonTranslation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Translate objects to and from JSON.
    /// </summary>
    public static class JsonTranslator
    {
        private static List<IJsonTranslatorStrategy> strategies = new List<IJsonTranslatorStrategy>();

        /// <summary>
        /// Scans the assembly for all <see cref="IJsonTranslatorStrategy"/> implementations and <see cref="JsonTranslatorStrategyAttribute"/> classes and add them to the list of strategies.
        /// </summary>
        /// <param name="assembly">The assembly to scan.</param>
        public static void LoadJsonTranslatorStrategies(Assembly assembly)
        {
            var payloadStrategyInterface = typeof(IJsonTranslatorStrategy);
            var payloadStrategyAttribute = typeof(JsonTranslatorStrategyAttribute);
            var strategies = (from type in assembly.GetTypes()
                              where type.IsClass && type.GetCustomAttribute(payloadStrategyAttribute) != null
                              let strategy = CreateTranslatorStrategy(type)
                              select strategy).ToArray();
            JsonTranslator.strategies.AddRange(strategies);

            Logging.LogInfo($"Loaded {strategies.Length} JSON translator strategies from {assembly.FullName}.");
        }

        /// <summary>
        /// Checks to see if any json translation strategies exist for the given target.
        /// </summary>
        /// <param name="target">The target to check for translation strategies for.</param>
        /// <returns>True if we have translation strategies for this object, or False otherwise.</returns>
        public static bool HasStrategyFor(object target)
        {
            var targetType = target.GetType();

            var strategies = from strategy in JsonTranslator.strategies
                             where strategy.TargetType.IsAssignableFrom(targetType)
                             select strategy;

            return strategies.Any();
        }

        /// <summary>
        /// Checks to see if any json translation strategies exist for the given type.
        /// </summary>
        /// <param name="targetType">The target type to check for translation strategies for.</param>
        /// <returns>True if we have translation strategies for this object, or False otherwise.</returns>
        public static bool HasStrategyForType(Type targetType)
        {
            var strategies = from strategy in JsonTranslator.strategies
                             where strategy.TargetType.IsAssignableFrom(targetType)
                             select strategy;

            return strategies.Any();
        }

        /// <summary>
        /// Converts the given object to a JSON object.
        /// </summary>
        /// <param name="target">The object to convert.</param>
        /// <returns>The JSON representation of the object.</returns>
        public static JObject ObjectToJson(object target)
        {
            var jObj = new JObject();
            ObjectToJson(target, jObj);
            return jObj;
        }

        /// <summary>
        /// Converts the given object to a JSON object.
        /// </summary>
        /// <param name="target">The object to convert.</param>
        /// <param name="jObj">The jobject to write into.</param>
        public static void ObjectToJson(object target, JObject jObj)
        {
            var targetType = target.GetType();

            var strategies = (from strategy in JsonTranslator.strategies
                              where strategy.TargetType.IsAssignableFrom(targetType)
                              select strategy).ToArray();

            if (strategies.Length == 0)
            {
                throw new Exception($"No serializer strategies exist for object of type '{targetType.FullName}'.");
            }

            foreach (var strategy in strategies)
            {
                strategy.WriteObjectToJson(target, jObj);
            }
        }

        /// <summary>
        /// Validate that the given JSON object can be written to the given target.
        /// </summary>
        /// <param name="payload">The payload to write.</param>
        /// <param name="target">The target to write to.</param>
        /// <exception cref="JsonTranslationException">The payload cannot be written to the target.</exception>
        public static void ValidateKnownProperties(JObject payload, object target)
        {
            var targetType = target.GetType();

            var remainingProperties = new HashSet<string>(payload.Properties().Select(x => x.Name));

            var strategies = from strategy in JsonTranslator.strategies
                             where strategy.TargetType.IsAssignableFrom(targetType)
                             select strategy;

            foreach (var strategy in strategies)
            {
                foreach (var supportedProperty in strategy.SupportedProperties)
                {
                    remainingProperties.Remove(supportedProperty);
                }
            }

            if (remainingProperties.Count > 0)
            {
                throw new JsonTranslationException(string.Format(@"Property '{0}' is invalid for object type '{1}'.", remainingProperties.First(), targetType.Name));
            }
        }

        /// <summary>
        /// Write the given JSON object can be written to the given target.
        /// </summary>
        /// <param name="payload">The payload to write.</param>
        /// <param name="target">The target to write to.</param>
        /// <param name="errorOnUnknownProperties">Whether or not to throw an exception if the payload contains unknown properties.</param>
        public static void UpdateObjectFromJson(JObject payload, object target, bool errorOnUnknownProperties = true)
        {
            if (errorOnUnknownProperties)
            {
                ValidateKnownProperties(payload, target);
            }

            var targetType = target.GetType();
            var strategies = (from strategy in JsonTranslator.strategies
                              where strategy.TargetType.IsAssignableFrom(targetType)
                              select strategy).ToArray();

            if (strategies.Length == 0)
            {
                throw new Exception(string.Format("No serializer strategies exist for object of type '{0}'.", targetType.Name));
            }

            foreach (var strategy in strategies)
            {
                strategy.UpdateObjectFromJson(target, payload);
            }
        }

        private static IJsonTranslatorStrategy CreateTranslatorStrategy(Type type)
        {
            if (type.GetCustomAttribute(typeof(JsonTranslatorTargetAttribute)) != null)
            {
                return AutoJsonTranslatorStrategy.FromInstance(Activator.CreateInstance(type));
            }
            else if (type.GetInterfaces().Contains(typeof(IJsonTranslatorStrategy)))
            {
                return (IJsonTranslatorStrategy)Activator.CreateInstance(type);
            }

            throw new Exception($"Cannot create translation strategy \"{type.FullName}\": Strategy must either be IJsonTranslatorStrategy or have a JsonTranslatorTargetAttribute.");
        }
    }
}
