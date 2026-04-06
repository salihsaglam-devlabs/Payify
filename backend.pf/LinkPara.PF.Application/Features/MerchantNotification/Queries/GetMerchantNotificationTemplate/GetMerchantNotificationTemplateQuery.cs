using LinkPara.HttpProviders.BusinessParameter;
using LinkPara.HttpProviders.Vault;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace LinkPara.PF.Application.Features.MerchantNotification.Queries.GetMerchantNotificationTemplate;

public class GetMerchantNotificationTemplateQuery : IRequest<MerchantNotificationTemplateDto>
{
    public Guid Id { get; set; }
    public MerchantContentSource ContentSource { get; set; }
    public string Language { get; set; }
}

public class GetMerchantNotificationTemplateQueryHandler : IRequestHandler<GetMerchantNotificationTemplateQuery, MerchantNotificationTemplateDto>
{
    private readonly IGenericRepository<Link> _linkRepository;
    private readonly IVaultClient _vaultClient;
    private readonly IParameterService _parameterService;
    private const string OrderLinkTr = "Sipariş Linki";
    private const string OrderLinkEn = "Purchase Order Link";

    public GetMerchantNotificationTemplateQueryHandler(
        IGenericRepository<Link> linkRepository,
        IVaultClient vaultClient,
        IParameterService parameterService)
    {
        _linkRepository = linkRepository;
        _vaultClient = vaultClient;
        _parameterService = parameterService;
    }

    public async Task<MerchantNotificationTemplateDto> Handle(GetMerchantNotificationTemplateQuery request, CancellationToken cancellationToken)
    {
        switch (request.ContentSource)
        {
            case MerchantContentSource.Link:
                var link = await _linkRepository.GetByIdAsync(request.Id);

                if (link == null)
                {
                    throw new NotFoundException(nameof(Link), request.Id);
                }

                var baseUrl = await _vaultClient.GetSecretValueAsync<string>("SharedSecrets", "ServiceUrls", "LinkPaymentBaseUrl");
                var template = (await _parameterService.GetParameterAsync("PFLinkNotificationTemplate", request.Language.ToLower())).ParameterValue;

                return new MerchantNotificationTemplateDto
                {
                    Subject = link.MerchantName + " " + (request.Language.ToLower() == "tr" ? OrderLinkTr : OrderLinkEn),
                    PreHeader = link.MerchantName + " " + OrderLinkEn,
                    Template = template.Replace("@@linkUrl", baseUrl + "/" + link.LinkCode)
                };
            default:
                throw new InvalidOperationException();
        }
    }
}