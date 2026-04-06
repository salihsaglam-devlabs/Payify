using LinkPara.PF.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.PF.Application.Features.MerchantBusinessPartners.Queries.GetMerchantBusinessPartnerById
{
    public class GetMerchantBusinessPartnerByIdQuery : IRequest<MerchantBusinessPartnerDto>
    {
        public Guid Id { get; set; }
    }

    public class GetMerchantBusinessPartnerByIdQueryHandler : IRequestHandler<GetMerchantBusinessPartnerByIdQuery, MerchantBusinessPartnerDto>
    {
        private readonly IMerchantBusinessPartnerService _merchantBusinessPartnerService;

        public GetMerchantBusinessPartnerByIdQueryHandler(IMerchantBusinessPartnerService merchantBusinessPartnerService)
        {
            _merchantBusinessPartnerService = merchantBusinessPartnerService;
        }
        public async Task<MerchantBusinessPartnerDto> Handle(GetMerchantBusinessPartnerByIdQuery request, CancellationToken cancellationToken)
        {
            return await _merchantBusinessPartnerService.GetByIdAsync(request.Id);
        }
    }
}
