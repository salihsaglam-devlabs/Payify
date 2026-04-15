namespace LinkPara.Card.Application.Commons.Models.Reporting.Dtos;

public class ReconciliationSummaryDailyDto
{
    public int? TransactionDate { get; set; }
    public long TotalCount { get; set; }
    public long MatchedCount { get; set; }
    public long UnmatchedCount { get; set; }
    public long AmountMismatchCount { get; set; }
    public long CurrencyMismatchCount { get; set; }
    public long DateMismatchCount { get; set; }
    public long StatusMismatchCount { get; set; }
    public long ProblemCount { get; set; }
    public long CleanCount { get; set; }
}

public class ReconciliationSummaryByNetworkDto
{
    public string Network { get; set; }
    public long TotalCount { get; set; }
    public long MatchedCount { get; set; }
    public long UnmatchedCount { get; set; }
    public long AmountMismatchCount { get; set; }
    public long CurrencyMismatchCount { get; set; }
    public long DateMismatchCount { get; set; }
    public long StatusMismatchCount { get; set; }
    public long ProblemCount { get; set; }
    public long CleanCount { get; set; }
}

public class ReconciliationSummaryByFileDto
{
    public Guid FileId { get; set; }
    public string FileName { get; set; }
    public long TotalCount { get; set; }
    public long MatchedCount { get; set; }
    public long UnmatchedCount { get; set; }
    public long AmountMismatchCount { get; set; }
    public long CurrencyMismatchCount { get; set; }
    public long DateMismatchCount { get; set; }
    public long StatusMismatchCount { get; set; }
    public long ProblemCount { get; set; }
    public long CleanCount { get; set; }
}

public class ReconciliationSummaryOverallDto
{
    public long TotalCount { get; set; }
    public long MatchedCount { get; set; }
    public long UnmatchedCount { get; set; }
    public long AmountMismatchCount { get; set; }
    public long CurrencyMismatchCount { get; set; }
    public long DateMismatchCount { get; set; }
    public long StatusMismatchCount { get; set; }
    public long ProblemCount { get; set; }
    public long CleanCount { get; set; }
}

