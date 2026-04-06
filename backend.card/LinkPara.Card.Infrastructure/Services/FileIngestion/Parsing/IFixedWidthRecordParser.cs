using LinkPara.Card.Application.Commons.Models.FileIngestion;
using LinkPara.Card.Infrastructure.Services.FileIngestion.Parsing;

namespace LinkPara.Card.Infrastructure.Services.FileIngestion.Parsing;

public interface IFixedWidthRecordParser
{
    ParsedFileLine Parse(string line, ParsingOptions parsingRule);
}
