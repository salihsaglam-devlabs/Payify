namespace LinkPara.ApiGateway.Services.Emoney.Models.Responses;

public class PaymentOrderResponseDto : PaymentConsentResultDto
{
    public PaymentOrderInfoType EmrBlg { get; set; }
}

public class PaymentOrderInfoType
{
    public string OdmEmriNo { get; set; }
    public DateTime OdmEmriZmn { get; set; }
}