using LinkPara.Card.Application.Commons.Interfaces;
using LinkPara.Card.Application.Commons.Models.FileIngestion;
using LinkPara.Card.Application.Commons.Constants;
using Microsoft.Extensions.Options;

namespace LinkPara.Card.Infrastructure.Services.FileIngestion.Parsers;

public class ClearingTransactionsFileParser : IFileParser
{
    private const string BkmRuleKey = FileParsingRuleKeys.ClearingBkm;
    private const string MscRuleKey = FileParsingRuleKeys.ClearingMsc;
    private const string VisaRuleKey = FileParsingRuleKeys.ClearingVisa;

    private readonly IOptionsMonitor<FileParsingRulesOptions> _rulesOptions;

    public ClearingTransactionsFileParser(IOptionsMonitor<FileParsingRulesOptions> rulesOptions)
    {
        _rulesOptions = rulesOptions;
    }

    public string FileType => FileIngestionValues.ClearingFileType;

    public bool CanParse(string fileName)
    {
        return fileName.Contains(FileNameMarkers.BkmAcc, StringComparison.OrdinalIgnoreCase)
               || fileName.Contains(FileNameMarkers.Bkm, StringComparison.OrdinalIgnoreCase)
               || fileName.Contains(FileNameMarkers.Msc, StringComparison.OrdinalIgnoreCase)
               || fileName.Contains(FileNameMarkers.Visa, StringComparison.OrdinalIgnoreCase)
               || fileName.Contains(FileNameMarkers.Clearing, StringComparison.OrdinalIgnoreCase);
    }

    public IReadOnlyCollection<ParsedFileRecord> Parse(string fileName, string[] lines)
    {
        var rule = ResolveRule(fileName, lines);
        return FixedWidthParserSupport.ParseLines(lines, rule);
    }

    private FixedWidthFileRule ResolveRule(string fileName, string[] lines)
    {
        var files = _rulesOptions.CurrentValue.Files;
        var upper = fileName.ToUpperInvariant();

        if ((upper.Contains(FileNameMarkers.BkmAcc) || upper.Contains(FileNameMarkers.Bkm)) && files.TryGetValue(BkmRuleKey, out var bkm)) return bkm;
        if (upper.Contains(FileNameMarkers.Msc) && files.TryGetValue(MscRuleKey, out var msc)) return msc;
        if (upper.Contains(FileNameMarkers.Visa) && files.TryGetValue(VisaRuleKey, out var visa)) return visa;

        var firstDetail = lines.Select(x => x.TrimEnd('\r')).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x) && !x.StartsWith("H") && !x.StartsWith("F"));
        if (firstDetail is not null)
        {
            foreach (var candidate in new[] { BkmRuleKey, MscRuleKey, VisaRuleKey })
            {
                if (files.TryGetValue(candidate, out var rule) && rule.DetailLength == firstDetail.Length)
                {
                    return rule;
                }
            }
        }

        throw new InvalidOperationException("Unable to resolve parsing rule for clearing file.");
    }
}
