using Microsoft.Extensions.Localization;
using LinkPara.Card.Application.Commons.Exceptions;
using LinkPara.Card.Application.Commons.Extensions;
using LinkPara.Card.Application.Commons.Models.FileIngestion.Configuration;

namespace LinkPara.Card.Infrastructure.Services.FileIngestion.Parsing;

public class FixedWidthRecordParser : IFixedWidthRecordParser
{
    private readonly IStringLocalizer _localizer;

    public FixedWidthRecordParser(Func<LinkPara.Card.Application.Commons.Localization.LocalizerResource, IStringLocalizer> localizerFactory)
    {
        _localizer = localizerFactory(LinkPara.Card.Application.Commons.Localization.LocalizerResource.Messages);
    }

    public ParsedFileLine Parse(string line, ParsingOptions parsingRule)
    {
        if (string.IsNullOrWhiteSpace(line))
            throw new FileIngestionParsingException(ApiErrorCode.FileIngestionParserLineEmpty, _localizer.Get("FileIngestion.ParserLineEmpty"));
        
        var recordType = string.IsNullOrWhiteSpace(line)
            ? string.Empty
            : line[0] switch
            {
                'H' => "H",
                'F' => "F",
                _ => "D"
            };
        
        if (!parsingRule.Records.TryGetValue(recordType, out var recordRule))
            throw new FileIngestionParsingException(ApiErrorCode.FileIngestionParserUnsupportedRecordType, _localizer.Get("FileIngestion.ParserUnsupportedRecordType", recordType));

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
            throw new FileIngestionParsingException(
                ApiErrorCode.FileIngestionBoundaryRecordReadFailed,
                _localizer.Get("FileIngestion.ParserRecordTypeError", recordType, ex.Message),
                ex);
        }

        return parsed;
    }
}
