
namespace LinkPara.CampaignManagement.Application.Commons.Models.Responses;

public class QrCodeResponse : BaseResponse
{
    public QrCodeServiceResponse Data { get; set; }
    public string message { get; set; }
}

public class QrCodeServiceResponse
{
    public int qr_code { get; set; }
    public DateTime expires_in { get; set; }
    public string cc_number { get; set; }
}

