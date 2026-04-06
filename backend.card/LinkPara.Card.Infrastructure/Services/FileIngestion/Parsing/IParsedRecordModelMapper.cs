namespace LinkPara.Card.Infrastructure.Services.FileIngestion.Parsing;

public interface IParsedRecordModelMapper
{
    object Create(string profileKey, ParsedFileLine parsedLine);
}
