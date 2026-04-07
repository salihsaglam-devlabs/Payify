using LinkPara.Card.Application.Commons.Interfaces;
using LinkPara.Card.Application.Commons.Models.FileIngestion;
using LinkPara.Card.Application.Commons.Constants;
using Microsoft.Extensions.Options;

namespace LinkPara.Card.Infrastructure.Services.FileIngestion.Parsers;

public class CardTransactionsFileParser : IFileParser
{
    private const string RuleKey = FileParsingRuleKeys.CardTransactions;
    private readonly IOptionsMonitor<FileParsingRulesOptions> _rulesOptions;

    public CardTransactionsFileParser(IOptionsMonitor<FileParsingRulesOptions> rulesOptions)
    {
        _rulesOptions = rulesOptions;
    }

    public string FileType => RuleKey;

    public bool CanParse(string fileName)
    {
        return fileName.Contains(FileNameMarkers.CardTransaction, StringComparison.OrdinalIgnoreCase);
    }

    public IReadOnlyCollection<ParsedFileRecord> Parse(string fileName, string[] lines)
    {
        _ = fileName;
        if (!_rulesOptions.CurrentValue.Files.TryGetValue(RuleKey, out var rule))
        {
            throw new InvalidOperationException($"Missing parsing rule: {RuleKey}");
        }

        return FixedWidthParserSupport.ParseLines(lines, rule);
    }
}
