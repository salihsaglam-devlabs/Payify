using System.Collections;
using System.Text.Json;
using LinkPara.Card.Application.Commons.Models.Reporting.Dynamic;

namespace LinkPara.Card.Application.Commons.Helpers.Reporting;

public static class DynamicReportingValidator
{
    public static IReadOnlyList<string> Validate(
        DynamicReportRequestContract contract,
        IReadOnlyList<DynamicReportingFilter>? filters)
    {
        var errors = new List<string>();
        if (filters is null || filters.Count == 0) return errors;

        var fieldMap = contract.Filters.ToDictionary(f => f.Field, StringComparer.Ordinal);

        for (var i = 0; i < filters.Count; i++)
        {
            var f = filters[i];

            if (string.IsNullOrWhiteSpace(f.Field))
            {
                errors.Add($"filters[{i}].field is required");
                continue;
            }
            if (string.IsNullOrWhiteSpace(f.Operator))
            {
                errors.Add($"filters[{i}].operator is required");
                continue;
            }
            if (!fieldMap.TryGetValue(f.Field, out var desc))
            {
                errors.Add($"filters[{i}].field '{f.Field}' is not allowed for this report");
                continue;
            }
            if (!desc.Operators.Contains(f.Operator, StringComparer.Ordinal))
            {
                errors.Add($"filters[{i}].operator '{f.Operator}' is not allowed for field '{f.Field}'");
                continue;
            }

            switch (f.Operator)
            {
                case DynamicReportingOperators.IsNull:
                case DynamicReportingOperators.IsNotNull:
                    if (!IsNullValue(f.Value))
                        errors.Add($"filters[{i}] operator '{f.Operator}' must not have a value");
                    break;

                case DynamicReportingOperators.Between:
                {
                    var arr = AsArray(f.Value);
                    if (arr.Count != 2)
                        errors.Add($"filters[{i}] operator 'between' requires an array of exactly 2 values");
                    break;
                }

                case DynamicReportingOperators.In:
                {
                    var arr = AsArray(f.Value);
                    if (arr.Count == 0)
                        errors.Add($"filters[{i}] operator 'in' requires a non-empty array");
                    break;
                }

                case DynamicReportingOperators.Contains:
                case DynamicReportingOperators.StartsWith:
                case DynamicReportingOperators.EndsWith:
                    if (desc.Type != DynamicReportingTypes.String)
                        errors.Add($"filters[{i}] operator '{f.Operator}' is only allowed on string fields");
                    if (IsNullValue(f.Value))
                        errors.Add($"filters[{i}] requires a value");
                    break;

                default:
                    if (IsNullValue(f.Value))
                        errors.Add($"filters[{i}] requires a value");
                    break;
            }
        }

        return errors;
    }

    private static bool IsNullValue(object? v)
        => v is null || (v is JsonElement je && je.ValueKind == JsonValueKind.Null);

    private static List<object?> AsArray(object? v)
    {
        if (v is null) return new();
        if (v is JsonElement je)
            return je.ValueKind == JsonValueKind.Array
                ? je.EnumerateArray().Cast<object?>().ToList()
                : new() { v };
        if (v is string) return new() { v };
        if (v is IEnumerable en) return en.Cast<object?>().ToList();
        return new() { v };
    }
}

