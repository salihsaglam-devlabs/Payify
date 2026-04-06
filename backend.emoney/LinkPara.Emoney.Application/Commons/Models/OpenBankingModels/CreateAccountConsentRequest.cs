using LinkPara.Emoney.Application.Commons.Mappings;
using LinkPara.Emoney.Application.Features.OpenBankingOperations;
using LinkPara.Emoney.Application.Features.OpenBankingOperations.Commands.CreateAccountConsent;

namespace LinkPara.Emoney.Application.Commons.Models.OpenBankingModels;

public class CreateAccountConsentRequest : IMapFrom<CreateAccountConsentCommand>
{
    public string YonTipi { get; set; }
    public string ErisimIzniSonTrh { get; set; }
    public List<string> IznTur { get; set; }
    public string DrmKod { get; set; }
    public AccountConsentIdentityInfo Kmlk { get; set; }
}
