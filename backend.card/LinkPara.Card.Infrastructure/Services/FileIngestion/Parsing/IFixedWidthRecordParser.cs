using LinkPara.Card.Application.Commons.Models.FileIngestion.Configuration;

namespace LinkPara.Card.Infrastructure.Services.FileIngestion.Parsing;

public interface IFixedWidthRecordParser
{
    ParsedFileLine Parse(string line, ParsingOptions parsingRule);
}
