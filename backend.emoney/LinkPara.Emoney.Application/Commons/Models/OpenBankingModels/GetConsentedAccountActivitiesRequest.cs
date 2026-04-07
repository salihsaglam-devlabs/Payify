using LinkPara.ApiGateway.Services.Emoney.Models.Enums;
using LinkPara.Emoney.Application.Commons.Mappings;
using LinkPara.Emoney.Application.Features.OpenBankingOperations.Commands.CreateAccountConsent;

namespace LinkPara.ApiGateway.Services.Emoney.Models.Requests;

public class GetConsentedAccountActivitiesRequest : IMapFrom<CreateAccountConsentCommand>
{
    public DateTime HesapIslemBslTrh { get; set; }
    public DateTime HesapIslemBtsTrh { get; set; }
    public string MinIslTtr { get; set; }
    public string MksIslTtr { get; set; }
    public string BrcAlc { get; set; }
    public string SyfNo { get; set; }
    public string SrlmKrtr { get; set; }
    public string SrlmYon { get; set; }
    public string SyfKytSayi { get; set; }
}

