using LinkPara.Card.Application.Commons.Models.FileIngestion;

namespace LinkPara.Card.Infrastructure.Services.FileIngestion.Parsers;

internal static class FixedWidthParserSupport
{
    private const string HeaderType = "H";
    private const string DetailType = "D";
    private const string FooterType = "F";

    public static IReadOnlyCollection<ParsedFileRecord> ParseLines(string[] lines, FixedWidthFileRule rule)
    {
        var records = new List<ParsedFileRecord>(lines.Length);
        var detailCount = 0;

        for (var lineIndex = 0; lineIndex < lines.Length; lineIndex++)
        {
            var lineNo = lineIndex + 1;
            var rawLine = lines[lineIndex].TrimEnd('\r');

            if (IsHeaderLine(rawLine, lineNo, rule))
            {
                records.Add(new ParsedFileRecord
                {
                    LineNo = lineNo,
                    RecordType = HeaderType,
                    RawLine = rawLine,
                    Fields = ExtractFields(rawLine, rule, HeaderType)
                });
                continue;
            }

            if (IsFooterLine(rawLine, rule))
            {
                records.Add(new ParsedFileRecord
                {
                    LineNo = lineNo,
                    RecordType = FooterType,
                    RawLine = rawLine,
                    Fields = ExtractFields(rawLine, rule, FooterType)
                });
                continue;
            }

            if (rule.DetailLength > 0 && rawLine.Length != rule.DetailLength)
            {
                throw new InvalidOperationException($"Detail line length mismatch at line {lineNo}. Expected={rule.DetailLength}, Actual={rawLine.Length}");
            }

            records.Add(new ParsedFileRecord
            {
                LineNo = lineNo,
                RecordType = DetailType,
                RawLine = rawLine,
                Fields = ExtractFields(rawLine, rule, DetailType)
            });
            detailCount++;
        }

        ValidateDeclaredTransactionCount(records, detailCount);
        return records;
    }

    private static void ValidateDeclaredTransactionCount(IReadOnlyCollection<ParsedFileRecord> records, int detailCount)
    {
        var footer = records.FirstOrDefault(x => x.RecordType == FooterType);
        if (footer is null || !footer.Fields.TryGetValue("TxnCount", out var declared) || string.IsNullOrWhiteSpace(declared))
        {
            return;
        }

        var normalized = new string((declared ?? string.Empty).Where(char.IsDigit).ToArray());
        if (!int.TryParse(string.IsNullOrWhiteSpace(normalized) ? declared.Trim() : normalized, out var declaredCount))
        {
            throw new InvalidOperationException($"Footer TxnCount value is invalid. Value='{declared}'");
        }

        if (declaredCount != detailCount)
        {
            throw new InvalidOperationException($"Footer TxnCount mismatch. Declared={declaredCount}, DetailCount={detailCount}");
        }
    }

    private static bool IsHeaderLine(string rawLine, int lineNo, FixedWidthFileRule rule)
    {
        if (string.IsNullOrEmpty(rawLine)) return false;
        if (rule.TreatFirstLineAsHeader && lineNo == 1) return true;
        if (string.IsNullOrEmpty(rule.HeaderPrefix) || !rawLine.StartsWith(rule.HeaderPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var headerLength = GetRecordLength(rule, HeaderType);
        return headerLength == 0 || rawLine.Length == headerLength;
    }

    private static bool IsFooterLine(string rawLine, FixedWidthFileRule rule)
    {
        if (string.IsNullOrEmpty(rawLine) || string.IsNullOrEmpty(rule.FooterPrefix)) return false;
        if (!rawLine.StartsWith(rule.FooterPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var footerLength = GetRecordLength(rule, FooterType);
        return footerLength == 0 || rawLine.Length == footerLength;
    }

    private static int GetRecordLength(FixedWidthFileRule rule, string recordType)
    {
        if (!rule.Records.TryGetValue(recordType, out var recordRule) || recordRule.Fields.Count == 0)
        {
            return 0;
        }

        return recordRule.Fields
            .Select(x => x.Value.Start + x.Value.Length - 1)
            .Max();
    }

    private static Dictionary<string, string> ExtractFields(string rawLine, FixedWidthFileRule rule, string recordType)
    {
        if (!rule.Records.TryGetValue(recordType, out var recordRule))
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var field in recordRule.Fields)
        {
            values[field.Key] = rawLine.SliceOrEmpty(field.Value.Start - 1, field.Value.Length);
        }

        return values;
    }
}
