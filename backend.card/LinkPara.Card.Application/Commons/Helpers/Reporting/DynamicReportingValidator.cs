using System.Collections;
using System.Text.Json;
using LinkPara.Card.Application.Commons.Extensions;
using LinkPara.Card.Application.Commons.Localization;
using LinkPara.Card.Application.Commons.Models.Reporting.Dynamic;
using Microsoft.Extensions.Localization;

namespace LinkPara.Card.Application.Commons.Helpers.Reporting;

public interface IDynamicReportingValidator
{
    IReadOnlyList<string> Validate(
        DynamicReportRequestContract contract,
        IReadOnlyList<DynamicReportingFilter>? filters);
}

public sealed class DynamicReportingValidator : IDynamicReportingValidator
{
    private readonly IStringLocalizer _localizer;

    public DynamicReportingValidator(Func<LocalizerResource, IStringLocalizer> localizerFactory)
    {
        _localizer = localizerFactory(LocalizerResource.Messages);
    }

    public IReadOnlyList<string> Validate(
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
                errors.Add(_localizer.Get("Reporting.Dynamic.FilterFieldRequired", i));
                continue;
            }
            if (string.IsNullOrWhiteSpace(f.Operator))
            {
                errors.Add(_localizer.Get("Reporting.Dynamic.FilterOperatorRequired", i));
                continue;
            }
            if (!fieldMap.TryGetValue(f.Field, out var desc))
            {
                errors.Add(_localizer.Get("Reporting.Dynamic.FilterFieldNotAllowed", i, f.Field));
                continue;
            }
            if (!desc.Operators.Contains(f.Operator, StringComparer.Ordinal))
            {
                errors.Add(_localizer.Get("Reporting.Dynamic.FilterOperatorNotAllowedForField", i, f.Operator, f.Field));
                continue;
            }

            switch (f.Operator)
            {
                case DynamicReportingOperators.IsNull:
                case DynamicReportingOperators.IsNotNull:
                    if (!IsNullValue(f.Value))
                        errors.Add(_localizer.Get("Reporting.Dynamic.FilterOperatorMustNotHaveValue", i, f.Operator));
                    break;

                case DynamicReportingOperators.Between:
                {
                    var arr = AsArray(f.Value);
                    if (arr.Count != 2)
                        errors.Add(_localizer.Get("Reporting.Dynamic.FilterBetweenRequiresTwoValues", i));
                    break;
                }

                case DynamicReportingOperators.In:
                {
                    var arr = AsArray(f.Value);
                    if (arr.Count == 0)
                        errors.Add(_localizer.Get("Reporting.Dynamic.FilterInRequiresNonEmptyArray", i));
                    break;
                }

                case DynamicReportingOperators.Contains:
                case DynamicReportingOperators.StartsWith:
                case DynamicReportingOperators.EndsWith:
                    if (desc.Type != DynamicReportingTypes.String)
                        errors.Add(_localizer.Get("Reporting.Dynamic.FilterOperatorStringOnly", i, f.Operator));
                    if (IsNullValue(f.Value))
                        errors.Add(_localizer.Get("Reporting.Dynamic.FilterRequiresValue", i));
                    break;

                default:
                    if (IsNullValue(f.Value))
                        errors.Add(_localizer.Get("Reporting.Dynamic.FilterRequiresValue", i));
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
