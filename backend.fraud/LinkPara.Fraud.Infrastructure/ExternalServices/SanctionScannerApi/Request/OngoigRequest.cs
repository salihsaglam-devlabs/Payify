namespace LinkPara.Fraud.Infrastructure.ExternalServices.SanctionScannerApi.Request;

public class OngoigRequest
{
    public string ScanId { get; set; }
    public string PeriodId { get; set; }
}

public class OngoigDisableRequest
{
    public string ScanId { get; set; }
}

