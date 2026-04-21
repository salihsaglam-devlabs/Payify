using System.Collections;
using System.Globalization;
using System.Text.Json;
using LinkPara.Card.Application.Commons.Interfaces.Reporting;
using LinkPara.Card.Application.Commons.Models.Reporting.Dynamic;

namespace LinkPara.Card.Infrastructure.Services.Reporting.Dynamic;

internal sealed class DynamicReportingSqlBuilder
{
    private const int SafetyRowLimit = 10_000;

    private readonly IDynamicReportingDialect _dialect;

    public DynamicReportingSqlBuilder(IDynamicReportingDialect dialect) => _dialect = dialect;

    public (string Sql, IReadOnlyDictionary<string, object?> Parameters) Build(
        string fullViewName,
        IReadOnlyList<DynamicReportingFilter> filters,
        DynamicReportRequestContract contract)
    {
        var parameters = new Dictionary<string, object?>();
        var whereParts = new List<string>();
        var pIdx = 0;

        foreach (var f in filters ?? Array.Empty<DynamicReportingFilter>())
        {
            var desc = contract.Filters.FirstOrDefault(x => x.Field == f.Field);
            if (desc is null) continue;

            var col = _dialect.QuoteIdent(desc.Field);

            switch (f.Operator)
            {
                case DynamicReportingOperators.Eq:
                    whereParts.Add($"{col} = @p{pIdx}");
                    parameters[$"p{pIdx++}"] = ConvertScalar(f.Value, desc.Type);
                    break;
                case DynamicReportingOperators.Neq:
                    whereParts.Add($"{col} <> @p{pIdx}");
                    parameters[$"p{pIdx++}"] = ConvertScalar(f.Value, desc.Type);
                    break;
                case DynamicReportingOperators.Gt:
                    whereParts.Add($"{col} > @p{pIdx}");
                    parameters[$"p{pIdx++}"] = ConvertScalar(f.Value, desc.Type);
                    break;
                case DynamicReportingOperators.Gte:
                    whereParts.Add($"{col} >= @p{pIdx}");
                    parameters[$"p{pIdx++}"] = ConvertScalar(f.Value, desc.Type);
                    break;
                case DynamicReportingOperators.Lt:
                    whereParts.Add($"{col} < @p{pIdx}");
                    parameters[$"p{pIdx++}"] = ConvertScalar(f.Value, desc.Type);
                    break;
                case DynamicReportingOperators.Lte:
                    whereParts.Add($"{col} <= @p{pIdx}");
                    parameters[$"p{pIdx++}"] = ConvertScalar(f.Value, desc.Type);
                    break;

                case DynamicReportingOperators.Contains:
                    whereParts.Add(_dialect.BuildLikeClause(col, $"@p{pIdx}"));
                    parameters[$"p{pIdx++}"] = "%" + EscapeLike(AsString(f.Value)) + "%";
                    break;
                case DynamicReportingOperators.StartsWith:
                    whereParts.Add(_dialect.BuildLikeClause(col, $"@p{pIdx}"));
                    parameters[$"p{pIdx++}"] = EscapeLike(AsString(f.Value)) + "%";
                    break;
                case DynamicReportingOperators.EndsWith:
                    whereParts.Add(_dialect.BuildLikeClause(col, $"@p{pIdx}"));
                    parameters[$"p{pIdx++}"] = "%" + EscapeLike(AsString(f.Value));
                    break;

                case DynamicReportingOperators.In:
                {
                    var arr = AsArray(f.Value).Select(v => ConvertScalar(v, desc.Type)).ToArray();
                    whereParts.Add($"{col} IN @p{pIdx}");
                    parameters[$"p{pIdx++}"] = arr;
                    break;
                }

                case DynamicReportingOperators.Between:
                {
                    var arr = AsArray(f.Value).Take(2).Select(v => ConvertScalar(v, desc.Type)).ToArray();
                    whereParts.Add($"{col} BETWEEN @p{pIdx} AND @p{pIdx + 1}");
                    parameters[$"p{pIdx++}"] = arr[0];
                    parameters[$"p{pIdx++}"] = arr[1];
                    break;
                }

                case DynamicReportingOperators.IsNull:
                    whereParts.Add($"{col} IS NULL");
                    break;
                case DynamicReportingOperators.IsNotNull:
                    whereParts.Add($"{col} IS NOT NULL");
                    break;
            }
        }

        var where = whereParts.Count == 0 ? string.Empty : " WHERE " + string.Join(" AND ", whereParts);
        var sql = _dialect.BuildSelect(fullViewName, where, SafetyRowLimit);
        return (sql, parameters);
    }
    
    private static string EscapeLike(string? v)
        => (v ?? string.Empty).Replace("/", "//").Replace("%", "/%").Replace("_", "/_");

    private static string? AsString(object? v)
    {
        if (v is null) return null;
        if (v is JsonElement je) return je.ValueKind == JsonValueKind.String ? je.GetString() : je.ToString();
        return Convert.ToString(v, CultureInfo.InvariantCulture);
    }

    private static IEnumerable<object?> AsArray(object? v)
    {
        if (v is null) yield break;
        if (v is JsonElement je && je.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in je.EnumerateArray()) yield return JsonElementToObject(item);
            yield break;
        }
        if (v is string) { yield return v; yield break; }
        if (v is IEnumerable en)
        {
            foreach (var item in en) yield return item;
            yield break;
        }
        yield return v;
    }

    private static object? ConvertScalar(object? v, string apiType)
    {
        if (v is null) return null;
        if (v is JsonElement je) v = JsonElementToObject(je);
        if (v is null) return null;

        return apiType switch
        {
            DynamicReportingTypes.Number   => Convert.ToDecimal(v, CultureInfo.InvariantCulture),
            DynamicReportingTypes.Boolean  => Convert.ToBoolean(v, CultureInfo.InvariantCulture),
            DynamicReportingTypes.DateTime => v is DateTime dt
                ? dt
                : DateTime.Parse(Convert.ToString(v, CultureInfo.InvariantCulture)!,
                                 CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
            _ => Convert.ToString(v, CultureInfo.InvariantCulture)
        };
    }

    private static object? JsonElementToObject(JsonElement e) => e.ValueKind switch
    {
        JsonValueKind.String => e.GetString(),
        JsonValueKind.Number => e.TryGetInt64(out var l) ? l : (object)e.GetDecimal(),
        JsonValueKind.True   => true,
        JsonValueKind.False  => false,
        JsonValueKind.Null   => null,
        JsonValueKind.Array  => e.EnumerateArray().Select(JsonElementToObject).ToArray(),
        _                    => e.GetRawText()
    };
}

