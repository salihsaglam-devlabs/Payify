using LinkPara.Card.Application.Commons.Models.FileIngestion;

namespace LinkPara.Card.Application.Commons.Interfaces;

public interface IFileParser
{
    bool CanParse(string fileName);
    string FileType { get; }
    IReadOnlyCollection<ParsedFileRecord> Parse(string fileName, string[] lines);
}
