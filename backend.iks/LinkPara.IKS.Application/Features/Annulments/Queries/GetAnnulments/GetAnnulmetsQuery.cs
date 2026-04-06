using AutoMapper;
using LinkPara.IKS.Application.Commons.Interfaces;
using LinkPara.IKS.Application.Commons.Models.IKSModels.Annulments.Response;
using LinkPara.IKS.Application.Commons.Models.IKSModels;
using MediatR;

namespace LinkPara.IKS.Application.Features.Annulments.Queries.GetAnnulments
{
    public class GetAnnulmetsQuery : IRequest<IKSResponse<AnnulmentsQueryResponse>>
    {
        public string GlobalMerchantId { get; set; }
    }

    public class GetAnnulmetsQueryHandler : IRequestHandler<GetAnnulmetsQuery, IKSResponse<AnnulmentsQueryResponse>>
    {
        private readonly IIKSService _iKSService;
        private readonly IMapper _mapper;

        public GetAnnulmetsQueryHandler(IIKSService iKSService,
                                        IMapper mapper)
        {
            _iKSService = iKSService;
            _mapper = mapper;
        }

        public async Task<IKSResponse<AnnulmentsQueryResponse>> Handle(GetAnnulmetsQuery request, CancellationToken cancellationToken)
        {
            var result = await _iKSService.GetAnnulmentsQueryAsync(request);

            var tenant = Environment.GetEnvironmentVariable("Tenant");

            var data = _mapper.Map<AnnulmentsQueryResponse>(result?.Data);
            var codes = await _iKSService.GetAnnulmentCodesAsync();

            foreach (var item in data?.Annulments)
            {
                item.OwnAnnulmentDescription = item.OwnAnnulment ? $"{tenant}" : "Diğer";
                item.AnnulmentCodeDescription = codes.Data?.annulmentCodes?.FirstOrDefault(x => x.code == item.Code) != null ?
                                                codes.Data?.annulmentCodes?.FirstOrDefault(x => x.code == item.Code).description :
                                                null;
            }

            return new IKSResponse<AnnulmentsQueryResponse>()
            {
                Error = result.Error,
                Data = data
            };
        }
    }
}