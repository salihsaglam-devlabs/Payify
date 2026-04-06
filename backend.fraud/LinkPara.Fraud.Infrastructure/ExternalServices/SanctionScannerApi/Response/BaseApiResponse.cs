namespace LinkPara.Fraud.Infrastructure.ExternalServices.SanctionScannerApi.Response;

public class BaseApiResponse
{
    public int HttpStatusCode { get; set; }
    public bool IsSuccess { get; set; }
    public string ErrorCode { get; set; }
    public string ErrorMessage { get; set; }
}
