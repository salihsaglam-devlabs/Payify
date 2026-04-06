using LinkPara.Card.Application.Commons.Models.FileIngestion;
using LinkPara.Card.Application.Commons.Helpers;

namespace LinkPara.Card.Infrastructure.Services.FileIngestion.Parsing;

public class FixedWidthRecordParser : IFixedWidthRecordParser
{
    public ParsedFileLine Parse(string line, ParsingOptions parsingRule)
    {
        if (string.IsNullOrWhiteSpace(line))
            throw new InvalidOperationException("Line cannot be empty or whitespace.");
        
        var recordType = string.IsNullOrWhiteSpace(line)
            ? string.Empty
            : line[0] switch
            {
                'H' => "H",
                'F' => "F",
                _ => "D"
            };
        
        if (!parsingRule.Records.TryGetValue(recordType, out var recordRule))
            throw new InvalidOperationException($"Unsupported record type '{recordType}'. Must be H (Header), D (Detail), or F (Footer).");

        var parsed = new ParsedFileLine
        {
            RecordType = recordType,
            RawLine = line
        };

        try
        {
            foreach (var field in recordRule.Fields)
            {
                var startIndex = Math.Max(field.Value.Start - 1, 0);
                if (line.Length <= startIndex)
                {
                    parsed.Fields[field.Key] = string.Empty;
                    continue;
                }

                var maxLength = Math.Min(field.Value.Length, line.Length - startIndex);
                parsed.Fields[field.Key] = line.Substring(startIndex, maxLength).Trim();
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Error parsing record type '{recordType}': {ex.Message}",
                ex);
        }

        return parsed;
    }
}
