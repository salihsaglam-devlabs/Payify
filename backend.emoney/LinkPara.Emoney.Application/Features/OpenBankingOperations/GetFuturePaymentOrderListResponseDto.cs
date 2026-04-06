using LinkPara.Emoney.Application.Features.AccountServiceProviders;

namespace LinkPara.Emoney.Application.Features.OpenBankingOperations;

public class GetFuturePaymentOrderListResponseDto : GetFuturePaymentOrderListResultDto
{

    public string RizaNo { get; set; }
    public string RizaDrm { get; set; }
    public string RizaIptDtyKod { get; set; }
    public PriceInfo IslTtr { get; set; }
    public PriceInfo GrckIslTtr { get; set; }
    public PersonInfo Gon { get; set; }
    public PersonInfo Alc { get; set; }
}

