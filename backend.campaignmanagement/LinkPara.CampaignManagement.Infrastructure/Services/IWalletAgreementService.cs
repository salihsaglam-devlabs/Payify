using AutoMapper;
using LinkPara.CampaignManagement.Application.Commons.Interfaces;
using LinkPara.CampaignManagement.Application.Commons.Interfaces.HttpClients;
using LinkPara.CampaignManagement.Application.Features.Agreements;

namespace LinkPara.CampaignManagement.Infrastructure.Services;

public class IWalletAgreementService : IIWalletAgreementService
{
    private readonly IIWalletHttpClient _iwalletHttpClient;
    private readonly IMapper _mapper;

    public IWalletAgreementService(IIWalletHttpClient iwalletHttpClient, IMapper mapper)
    {
        _iwalletHttpClient = iwalletHttpClient;
        _mapper = mapper;
    }

    public async Task<List<AgreementDto>> GetAgreementsAsync()
    {
        var aggrements = await _iwalletHttpClient.GetAgreementsAsync();
        return _mapper.Map<List<AgreementDto>>(aggrements);
    }
}
