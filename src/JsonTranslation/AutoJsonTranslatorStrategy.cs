namespace SHRestAPI.JsonTranslation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Implements a <see cref="IJsonTranslatorStrategy"/> by using attributes on an instance.
    /// </summary>
    internal class AutoJsonTranslatorStrategy : IJsonTranslatorStrategy
    {
        private static Dictionary<Type, string> expectedTypeErrors = new()
        {
            { typeof(int), "integer" },
            { typeof(float), "decimal" },
            { typeof(double), "decimal" },
            { typeof(string), "string" },
            { typeof(JObject), "object" },
            { typeof(JToken), "json value" },
        };

        private object instance;

        private HashSet<string> supportedProperties = new();
        private Dictionary<string, MethodInfo> propertyGetters = new();
        private Dictionary<string, MethodInfo> propertySetters = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoJsonTranslatorStrategy"/> class.
        /// <summary>
        /// <param name="targetType">The type of object this translator will produce.</param>
        /// <param name="instance">The instance of the object to produce a translator from.</param>
        public AutoJsonTranslatorStrategy(Type targetType, object instance)
        {
            var instanceType = instance.GetType();
            var instanceMethods = instanceType.GetMethods(BindingFlags.Public | BindingFlags.Instance);

            this.TargetType = targetType;
            this.instance = instance;

            var getters =
                from method in instanceMethods
                let attribute = (JsonPropertyGetterAttribute)Attribute.GetCustomAttribute(method, typeof(JsonPropertyGetterAttribute))
                where attribute != null
                select new { Method = method, Attribute = attribute };

            foreach (var getter in getters)
            {
                var method = getter.Method;
                var methodParams = method.GetParameters();
                if (methodParams.Length != 1)
                {
                    throw new Exception("Expected getter method to have exactly 1 parameter");
                }

                var firstParamType = methodParams[0].ParameterType;
                if (!firstParamType.IsAssignableFrom(targetType))
                {
                    throw new Exception($"Getter method {method.Name} first parameter of type {firstParamType.FullName} is not assignable from TargetType \"{targetType.FullName}\".");
                }

                this.propertyGetters.Add(getter.Attribute.PropertyName, method);
                this.supportedProperties.Add(getter.Attribute.PropertyName);
            }

            var setters =
                from method in instanceMethods
                let attribute = (JsonPropertySetterAttribute)Attribute.GetCustomAttribute(method, typeof(JsonPropertySetterAttribute))
                where attribute != null
                select new { Method = method, Attribute = attribute };
            foreach (var setter in setters)
            {
                var method = setter.Method;
                var methodParams = method.GetParameters();
                if (methodParams.Length != 2)
                {
                    throw new Exception("Expected setter method to have exactly 2 parameters");
                }

                var firstParamType = methodParams[0].ParameterType;
                if (!firstParamType.IsAssignableFrom(targetType))
                {
                    throw new Exception($"Setter method {method.Name} first parameter of type {firstParamType.FullName} is not assignable from TargetType \"{targetType.FullName}\".");
                }

                this.propertySetters.Add(setter.Attribute.PropertyName, setter.Method);
                this.supportedProperties.Add(setter.Attribute.PropertyName);
            }
        }

        /// <inheritdoc/>
        public Type TargetType { get; private set; }

        /// <inheritdoc/>
        public IReadOnlyCollection<string> SupportedProperties
        {
            get
            {
                return this.supportedProperties.ToArray();
            }
        }

        /// <summary>
        /// Creates a new <see cref="AutoJsonTranslatorStrategy"/> from an instance.
        /// </summary>
        /// <param name="instance">The instance of the object to produce a translator from.</param>
        /// <returns>The translator strategy for this object.</returns>
        public static IJsonTranslatorStrategy FromInstance(object instance)
        {
            var type = instance.GetType();
            var targetAttribute = (JsonTranslatorTargetAttribute)Attribute.GetCustomAttribute(type, typeof(JsonTranslatorTargetAttribute));
            if (targetAttribute == null)
            {
                throw new Exception("Instance passed to AutoJsonTranslatorStrategy must have a JsonTranslatorTargetAttribute.");
            }

            return new AutoJsonTranslatorStrategy(targetAttribute.TargetType, instance);
        }

        /// <inheritdoc/>
        public void VerifyJsonUpdate(object target, JObject input)
        {
            foreach (var property in this.supportedProperties)
            {
                // FIXME: JObject implements IDictionary, which has ContainsKey.
                // Decompiler shows JObject has ContainsKey
                // We have ContainsKey
                // Why the hell does the compiler say ContainsKey not found
                if (!input.TryGetValue(property, out var _))
                {
                    continue;
                }

                // Do a dry-run of casting the value, to throw exceptions for incompatible types
                this.CastSetterValue(property, input[property]);
            }
        }

        /// <inheritdoc/>
        public void UpdateObjectFromJson(object target, JObject input)
        {
            // Handle all properties, so we can generate read only exceptions if needed.
            foreach (var property in this.supportedProperties)
            {
                if (!input.TryGetValue(property, out var jsonValue))
                {
                    continue;
                }

                var value = this.CastSetterValue(property, jsonValue);
                this.propertySetters[property].Invoke(this.instance, new[] { target, value });
            }
        }

        /// <inheritdoc/>
        public void WriteObjectToJson(object target, JObject output)
        {
            // We only care about writing properties we can read.  No WriteOnlyExceptions here.
            foreach (var pair in this.propertyGetters)
            {
                var value = pair.Value.Invoke(this.instance, new[] { target });
                if (value == null)
                {
                    output[pair.Key] = null;
                }
                else if (JsonTranslator.HasStrategyFor(value))
                {
                    JsonTranslator.ObjectToJson(value, output);
                }
                else if (value is JToken token)
                {
                    output[pair.Key] = token;
                }
                else
                {
                    output[pair.Key] = JToken.FromObject(value);
                }
            }
        }

        private object CastSetterValue(string property, JToken token)
        {
            if (!this.propertySetters.TryGetValue(property, out var setterInfo))
            {
                throw new ReadOnlyPropertyException($"Property \"{property}\" is read-only.");
            }

            var expectedType = setterInfo.GetParameters()[1].ParameterType;

            // Not sure if this null handling is required, token.ToObject might just handle it.
            var underlyingType = Nullable.GetUnderlyingType(expectedType);
            if (underlyingType != null)
            {
                if (token.Type == JTokenType.Null)
                {
                    return null;
                }

                expectedType = underlyingType;
            }
            else if (token.Type == JTokenType.Null)
            {
                throw new PropertyTypeException($"Property \"{property}\" cannot be null.");
            }

            // Try to get the value.
            //  If we fail, try to produce a sensible error message.
            //  Failure is assumed to be due to data type.  We could use a better Exception type here.
            try
            {
                return token.ToObject(expectedType);
            }
            catch (Exception)
            {
                string typeName;
                if (!expectedTypeErrors.TryGetValue(expectedType, out typeName))
                {
                    typeName = expectedType.Name;
                }

                throw new PropertyTypeException($"Property \"{property}\" should be of type \"{typeName}\".");
            }
        }
    }
}
