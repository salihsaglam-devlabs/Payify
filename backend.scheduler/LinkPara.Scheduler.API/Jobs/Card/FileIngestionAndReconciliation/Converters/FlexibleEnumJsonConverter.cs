using System.Text.Json;
using System.Text.Json.Serialization;

namespace LinkPara.Scheduler.API.Jobs.Card.FileIngestionAndReconciliation.Converters;

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

            if (Enum.TryParse<TEnum>(rawValue, true, out var enumValue))
                return enumValue;

            if (int.TryParse(rawValue, out var numericValue) &&
                Enum.IsDefined(typeof(TEnum), numericValue))
                return (TEnum)Enum.ToObject(typeof(TEnum), numericValue);

            throw new JsonException($"Invalid enum value: {rawValue}");
        }

        if (reader.TokenType == JsonTokenType.Number &&
            reader.TryGetInt32(out var number) &&
            Enum.IsDefined(typeof(TEnum), number))
        {
            return (TEnum)Enum.ToObject(typeof(TEnum), number);
        }

        throw new JsonException($"Invalid token for enum {typeof(TEnum).Name}");
    }

    public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}