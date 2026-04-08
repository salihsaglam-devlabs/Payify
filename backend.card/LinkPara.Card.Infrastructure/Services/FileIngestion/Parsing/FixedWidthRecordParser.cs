using LinkPara.Card.Application.Commons.Models.FileIngestion;
using LinkPara.Card.Application.Commons.Helpers;
using LinkPara.Card.Application.Commons.Interfaces.Localization;

namespace LinkPara.Card.Infrastructure.Services.FileIngestion.Parsing;

public class FixedWidthRecordParser : IFixedWidthRecordParser
{
    private readonly ICardResourceLocalizer _localizer;

    public FixedWidthRecordParser(ICardResourceLocalizer localizer)
    {
        _localizer = localizer;
    }

    public ParsedFileLine Parse(string line, ParsingOptions parsingRule)
    {
        if (string.IsNullOrWhiteSpace(line))
            throw new InvalidOperationException(_localizer.Get("FileIngestion.ParserLineEmpty"));
        
        var recordType = string.IsNullOrWhiteSpace(line)
            ? string.Empty
            : line[0] switch
            {
                'H' => "H",
                'F' => "F",
                _ => "D"
            };
        
        if (!parsingRule.Records.TryGetValue(recordType, out var recordRule))
            throw new InvalidOperationException(_localizer.Get("FileIngestion.ParserUnsupportedRecordType", recordType));

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
                _localizer.Get("FileIngestion.ParserRecordTypeError", recordType, ex.Message),
                ex);
        }

        return parsed;
    }
}
