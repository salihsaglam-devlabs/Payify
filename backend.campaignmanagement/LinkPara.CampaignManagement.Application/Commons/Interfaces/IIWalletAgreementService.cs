
using LinkPara.CampaignManagement.Application.Features.Agreements;

namespace LinkPara.CampaignManagement.Application.Commons.Interfaces;

public interface IIWalletAgreementService
{
    Task<List<AgreementDto>> GetAgreementsAsync();
}
