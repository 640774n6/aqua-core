﻿// Copyright (c) Christof Senn. All rights reserved. See license.txt in the project root for license information.

namespace Aqua.Newtonsoft.Json.Converters
{
    using Aqua.Dynamic;
    using Aqua.TypeSystem;
    using global::Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using static Aqua.Dynamic.DynamicObjectMapper;
    using DynamicProperty = Aqua.Dynamic.Property;

    public sealed class DynamicObjectConverter : ObjectConverter<DynamicObject>
    {
        protected override void ReadObject(JsonReader reader, DynamicObject result, Dictionary<string, Property> properties, JsonSerializer serializer)
        {
            reader.AssertStartObject(false);
            reader.Advance();

            TypeInfo typeInfo = null;
            void SetResult(IEnumerable<DynamicProperty> properties = null)
            {
                reader.AssertEndObject();

                result.Type = typeInfo;
                if (properties?.Any() == true)
                {
                    result.Properties = new PropertySet(properties);
                }
            }

            if (reader.IsProperty(nameof(DynamicObject.Type)))
            {
                typeInfo = reader.Read<TypeInfo>(serializer);
                reader.Advance();
            }

            if (reader.IsProperty("Value"))
            {
                var value = reader.Read(typeInfo?.Type, serializer);
                SetResult(new[] { new DynamicProperty(string.Empty, value) });
                return;
            }

            if (reader.IsProperty("Values"))
            {
                reader.Advance();
                if (reader.TokenType == JsonToken.Null)
                {
                    SetResult();
                    return;
                }

                if (reader.TokenType != JsonToken.StartArray)
                {
                    throw new JsonSerializationException($"Expected array");
                }

                var elementType = TypeHelper.GetElementType(typeInfo.Type);
                var values = new List<object>();
                while (true)
                {
                    if (!reader.TryRead(elementType, serializer, out object value))
                    {
                        // TODO: is max length quota required?
                        if (reader.TokenType == JsonToken.EndArray)
                        {
                            break;
                        }

                        throw new JsonSerializationException("Unexpected token structure.");
                    }

                    values.Add(value);
                }

                if (values.Any(x => x != null && !elementType.IsAssignableFrom(x.GetType())) &&
                    values.All(x => x is null || x is string))
                {
                    elementType = typeof(string);
                }

                var valueArray = CastCollectionToArrayOfType(elementType, values);
                SetResult(new[] { new DynamicProperty(string.Empty, valueArray) });
                return;
            }

            if (reader.IsProperty(nameof(DynamicObject.Properties)))
            {
                reader.Advance();
                if (reader.TokenType == JsonToken.Null)
                {
                    SetResult();
                    return;
                }

                if (reader.TokenType != JsonToken.StartArray)
                {
                    throw new JsonSerializationException($"Expected array");
                }

                var propertySet = new List<DynamicProperty>();
                while (true)
                {
                    reader.Advance();

                    // TODO: is max length quota required?
                    if (reader.TokenType == JsonToken.EndArray)
                    {
                        break;
                    }

                    reader.AssertStartObject(false);

                    reader.AssertProperty(nameof(DynamicProperty.Name));
                    var name = reader.ReadAsString();

                    reader.AssertProperty(nameof(Type));
                    var type = reader.Read<TypeInfo>(serializer);

                    reader.AssertProperty(nameof(DynamicProperty.Value));
                    var value = reader.Read(type?.Type, serializer);

                    reader.AssertEndObject();
                    propertySet.Add(new DynamicProperty(name, value));
                }

                SetResult(propertySet);
                return;
            }

            throw new JsonSerializationException($"Unexpected token {reader.TokenType}");
        }

        protected override void WriteObject(JsonWriter writer, DynamicObject instance, IReadOnlyCollection<Property> properties, JsonSerializer serializer)
        {
            writer.WriteStartObject();

            if (instance.Properties?.Count() == 1 &&
                string.IsNullOrEmpty(instance.Properties.Single().Name) &&
                instance.Properties.Single().Value != null)
            {
                var property = instance.Properties.Single();
                var type = instance.Type ?? CreateTypeInfo(property);

                writer.WritePropertyName(nameof(DynamicObject.Type));
                serializer.Serialize(writer, type);

                writer.WritePropertyName(type.IsCollection() ? "Values" : "Value");
                serializer.Serialize(writer, property.Value, type?.Type);
            }
            else
            {
                if (instance.Type != null)
                {
                    writer.WritePropertyName(nameof(DynamicObject.Type));
                    serializer.Serialize(writer, instance.Type);
                }

                if (instance.Properties?.Any() == true)
                {
                    writer.WritePropertyName(nameof(DynamicObject.Properties));

                    writer.WriteStartArray();
                    foreach (var property in instance.Properties)
                    {
                        writer.WriteStartObject();

                        writer.WritePropertyName(nameof(DynamicProperty.Name));
                        writer.WriteValue(property.Name);

                        writer.WritePropertyName(nameof(Type));
                        serializer.Serialize(writer, CreateTypeInfo(property));

                        writer.WritePropertyName(nameof(DynamicProperty.Value));
                        if (property.Value is null)
                        {
                            writer.WriteNull();
                        }
                        else
                        {
                            serializer.Serialize(writer, property.Value);
                        }

                        writer.WriteEndObject();
                    }

                    writer.WriteEndArray();
                }
            }

            writer.WriteEndObject();
        }

        private static TypeInfo CreateTypeInfo(DynamicProperty property)
            => property.Value is null
            ? null
            : new TypeInfo(property.Value.GetType(), false, false);
    }
}
