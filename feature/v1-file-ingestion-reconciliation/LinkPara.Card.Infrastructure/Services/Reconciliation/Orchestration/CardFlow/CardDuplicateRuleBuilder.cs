using LinkPara.Card.Application.Commons.Constants;
using LinkPara.Card.Domain.Entities.FileIngestion;
using System.Globalization;

namespace LinkPara.Card.Infrastructure.Services.Reconciliation.Orchestration.CardFlow;

internal static class CardDuplicateRuleBuilder
{
    public static Dictionary<Guid, DuplicateRule> Build(IReadOnlyCollection<CardTransactionRecord> cards)
    {
        var result = new Dictionary<Guid, DuplicateRule>();

        var duplicateGuidGroups = cards
            .Where(x => !string.IsNullOrWhiteSpace(x.OceanTxnGuid))
            .GroupBy(x => CardFlowText.Normalize(x.OceanTxnGuid))
            .Where(g => g.Count() > 1);

        foreach (var group in duplicateGuidGroups)
        {
            var records = group
                .OrderBy(x => x.CreateDate)
                .ThenBy(x => x.Id)
                .ToList();

            var signatures = records
                .Select(BuildDuplicateSignature)
                .Distinct(StringComparer.Ordinal)
                .ToList();

            if (signatures.Count == 1)
            {
                var first = records[0];
                result[first.Id] = DuplicateRule.Process(
                    duplicateGroupKey: group.Key,
                    duplicateType: CardDuplicateType.SameSignature,
                    shouldRaiseAlarm: true);

                for (var duplicateRecordIndex = 1; duplicateRecordIndex < records.Count; duplicateRecordIndex++)
                {
                    result[records[duplicateRecordIndex].Id] = DuplicateRule.Skip(
                        duplicateGroupKey: group.Key,
                        duplicateType: CardDuplicateType.SameSignature,
                        shouldRaiseAlarm: false);
                }

                continue;
            }

            for (var duplicateRecordIndex = 0; duplicateRecordIndex < records.Count; duplicateRecordIndex++)
            {
                var duplicate = records[duplicateRecordIndex];
                result[duplicate.Id] = DuplicateRule.Skip(
                    duplicateGroupKey: group.Key,
                    duplicateType: CardDuplicateType.ConflictingSignature,
                    shouldRaiseAlarm: duplicateRecordIndex == 0);
            }
        }

        return result;
    }

    private static string BuildDuplicateSignature(CardTransactionRecord card)
    {
        return string.Join(
            CorrelationKeyValues.DuplicateSignatureSeparator,
            CardFlowText.Normalize(card.CardNo),
            CardFlowText.Normalize(card.Otc),
            CardFlowText.Normalize(card.Ots),
            FormatAmount(card.CardHolderBillingAmount));
    }

    private static string FormatAmount(decimal? amount)
    {
        return amount.HasValue
            ? amount.Value.ToString("0.00", CultureInfo.InvariantCulture)
            : string.Empty;
    }
}

internal sealed class DuplicateRule
{
    public bool ShouldProcess { get; init; }
    public bool ShouldRaiseAlarm { get; init; }
    public string DuplicateGroupKey { get; init; } = string.Empty;
    public CardDuplicateType DuplicateType { get; init; } = CardDuplicateType.None;

    public static DuplicateRule Process(string duplicateGroupKey = "", CardDuplicateType duplicateType = CardDuplicateType.None, bool shouldRaiseAlarm = false)
    {
        return new DuplicateRule
        {
            ShouldProcess = true,
            ShouldRaiseAlarm = shouldRaiseAlarm,
            DuplicateGroupKey = duplicateGroupKey ?? string.Empty,
            DuplicateType = duplicateType
        };
    }

    public static DuplicateRule Skip(string duplicateGroupKey = "", CardDuplicateType duplicateType = CardDuplicateType.None, bool shouldRaiseAlarm = false)
    {
        return new DuplicateRule
        {
            ShouldProcess = false,
            ShouldRaiseAlarm = shouldRaiseAlarm,
            DuplicateGroupKey = duplicateGroupKey ?? string.Empty,
            DuplicateType = duplicateType
        };
    }
}

internal enum CardDuplicateType
{
    None = 0,
    SameSignature = 1,
    ConflictingSignature = 2
}
