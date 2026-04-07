using System.Text.Json;
using System.Text.Json.Serialization;

namespace LinkPara.Card.Application.Commons.Helpers.Shared;

public sealed class FlexibleEnumJsonConverter<TEnum> : JsonConverter<TEnum>
    where TEnum : struct, Enum
{
    public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var rawValue = reader.GetString();

            if (string.IsNullOrWhiteSpace(rawValue))
                throw new JsonException($"Empty value is not valid for enum {typeof(TEnum).Name}.");

            if (Enum.TryParse<TEnum>(rawValue, ignoreCase: true, out var enumValue))
                return enumValue;

            if (int.TryParse(rawValue, out var numericValue) &&
                Enum.IsDefined(typeof(TEnum), numericValue))
            {
                return (TEnum)Enum.ToObject(typeof(TEnum), numericValue);
            }

            throw new JsonException($"Value '{rawValue}' is not valid for enum {typeof(TEnum).Name}.");
        }

        if (reader.TokenType == JsonTokenType.Number)
        {
            if (reader.TryGetInt32(out var numericValue) &&
                Enum.IsDefined(typeof(TEnum), numericValue))
            {
                return (TEnum)Enum.ToObject(typeof(TEnum), numericValue);
            }

            throw new JsonException($"Numeric value is not valid for enum {typeof(TEnum).Name}.");
        }

        throw new JsonException($"Token type '{reader.TokenType}' is not valid for enum {typeof(TEnum).Name}.");
    }

    public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(Convert.ToInt32(value));
    }
} 
