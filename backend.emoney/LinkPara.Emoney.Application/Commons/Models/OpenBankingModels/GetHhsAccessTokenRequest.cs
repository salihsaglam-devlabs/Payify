using LinkPara.Emoney.Application.Commons.Enums;
using LinkPara.Emoney.Application.Commons.Mappings;
using LinkPara.Emoney.Application.Features.OpenBankingOperations.Queries.GetHhsAccessToken;

namespace LinkPara.Emoney.Application.Commons.Models.OpenBankingModels;

public class GetHhsAccessTokenRequest : IMapFrom<GetHhsAccessTokenQuery>
{
    public string RizaNo { get; set; }
    public string RizaTip { get; set; }
    public string YetTip { get; set; }
    public string YetKod { get; set; }
}
