using System.Globalization;
using System.Text.Json;
using LinkPara.Card.Application.Commons.Exceptions;
using LinkPara.Card.Application.Commons.Extensions;
using Microsoft.Extensions.Localization;

namespace LinkPara.Card.Infrastructure.Services.Reconciliation.Execute.Core;

internal sealed class OperationPayloadAccessor
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly Dictionary<string, List<OperationPayloadEntry>> _payload;
    private readonly IStringLocalizer _localizer;

    public OperationPayloadAccessor(string? payloadJson, IStringLocalizer localizer)
    {
        _localizer = localizer;

        if (string.IsNullOrWhiteSpace(payloadJson))
        {
            throw new ReconciliationPayloadException(ApiErrorCode.ReconciliationOperationPayloadEmpty, _localizer.Get("Reconciliation.OperationPayloadEmpty"));
        }

        _payload = JsonSerializer.Deserialize<Dictionary<string, List<OperationPayloadEntry>>>(payloadJson, JsonOptions)
            ?? throw new ReconciliationPayloadException(ApiErrorCode.ReconciliationOperationPayloadDeserializeFailed, _localizer.Get("Reconciliation.OperationPayloadDeserializeFailed"));
    }

    public T GetRequiredValue<T>(string group, string key)
    {
        if (!_payload.TryGetValue(group, out var items))
        {
            throw new ReconciliationPayloadException(ApiErrorCode.ReconciliationOperationPayloadValueMissing, _localizer.Get("Reconciliation.OperationPayloadValueMissing", group, key));
        }

        var entry = items.FirstOrDefault(x => string.Equals(x.Key, key, StringComparison.Ordinal));
        if (entry is null || string.IsNullOrWhiteSpace(entry.Value))
        {
            throw new ReconciliationPayloadException(ApiErrorCode.ReconciliationOperationPayloadValueMissing, _localizer.Get("Reconciliation.OperationPayloadValueMissing", group, key));
        }

        return ConvertValue<T>(entry.Value);
    }

    public T GetOptionalValue<T>(string group, string key)
    {
        if (!_payload.TryGetValue(group, out var items))
        {
            return default!;
        }

        var entry = items.FirstOrDefault(x => string.Equals(x.Key, key, StringComparison.Ordinal));
        if (entry is null || string.IsNullOrWhiteSpace(entry.Value))
        {
            return default!;
        }

        return ConvertValue<T>(entry.Value);
    }

    private T ConvertValue<T>(string value)
    {
        var targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);

        object? parsed = targetType switch
        {
            _ when targetType == typeof(string) => value,
            _ when targetType == typeof(char) && char.TryParse(value, out var charValue) => charValue,
            _ when targetType == typeof(byte) && byte.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var byteValue) => byteValue,
            _ when targetType == typeof(sbyte) && sbyte.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var sbyteValue) => sbyteValue,
            _ when targetType == typeof(short) && short.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var shortValue) => shortValue,
            _ when targetType == typeof(ushort) && ushort.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var ushortValue) => ushortValue,
            _ when targetType == typeof(int) && int.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var intValue) => intValue,
            _ when targetType == typeof(uint) && uint.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var uintValue) => uintValue,
            _ when targetType == typeof(Guid) && Guid.TryParse(value, out var guidValue) => guidValue,
            _ when targetType == typeof(long) && long.TryParse(value, out var longValue) => longValue,
            _ when targetType == typeof(ulong) && ulong.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var ulongValue) => ulongValue,
            _ when targetType == typeof(float) && float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var floatValue) => floatValue,
            _ when targetType == typeof(double) && double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var doubleValue) => doubleValue,
            _ when targetType == typeof(decimal) && decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var decimalValue) => decimalValue,
            _ when targetType == typeof(bool) && bool.TryParse(value, out var boolValue) => boolValue,
            _ when targetType == typeof(DateTime) && DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dateTimeValue) => dateTimeValue,
            _ when targetType == typeof(DateTimeOffset) && DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dateTimeOffsetValue) => dateTimeOffsetValue,
            _ when targetType == typeof(TimeSpan) && TimeSpan.TryParse(value, CultureInfo.InvariantCulture, out var timeSpanValue) => timeSpanValue,
            _ when targetType == typeof(DateOnly) && DateOnly.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateOnlyValue) => dateOnlyValue,
            _ when targetType == typeof(TimeOnly) && TimeOnly.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var timeOnlyValue) => timeOnlyValue,
            _ when targetType.IsEnum && Enum.TryParse(targetType, value, true, out var enumValue) => enumValue,
            _ => null
        };

        if (parsed is null)
        {
            throw new ReconciliationPayloadException(ApiErrorCode.ReconciliationOperationPayloadConversionFailed, _localizer.Get("Reconciliation.OperationPayloadConversionFailed", value, targetType.Name));
        }

        return (T)parsed;
    }
}

internal sealed class OperationPayloadEntry
{
    public string Key { get; set; } = string.Empty;
    public string? Value { get; set; }
}
