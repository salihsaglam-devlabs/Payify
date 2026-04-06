namespace LinkPara.Fraud.Infrastructure.ExternalServices.SanctionScannerApi.Request;

public class AddMemoNotesToSearchRequest
{
    public string ScanId { get; set; }
    public string Memo { get; set; }
}
