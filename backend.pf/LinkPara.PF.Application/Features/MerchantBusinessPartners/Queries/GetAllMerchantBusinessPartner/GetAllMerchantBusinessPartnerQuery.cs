using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.SharedModels.Pagination;
using MediatR;

namespace LinkPara.PF.Application.Features.MerchantBusinessPartners.Queries.GetAllMerchantBusinessPartner
{
    public class GetAllMerchantBusinessPartnerQuery : SearchQueryParams, IRequest<PaginatedList<MerchantBusinessPartnerDto>>
    {
        public Guid? MerchantId { get; set; }
    }

    public class GetAllMerchantBusinessPartnerHandler : IRequestHandler<GetAllMerchantBusinessPartnerQuery, PaginatedList<MerchantBusinessPartnerDto>>
    {
        private readonly IMerchantBusinessPartnerService _merchantBusinessPartnerService;

        public GetAllMerchantBusinessPartnerHandler(IMerchantBusinessPartnerService merchantBusinessPartnerService)
        {
            _merchantBusinessPartnerService = merchantBusinessPartnerService;
        }
        public async Task<PaginatedList<MerchantBusinessPartnerDto>> Handle(GetAllMerchantBusinessPartnerQuery request, CancellationToken cancellationToken)
        {
            return await _merchantBusinessPartnerService.GetAllAsync(request);
        }
    }

}