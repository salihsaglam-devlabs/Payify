using LinkPara.IKS.Application.Commons.Interfaces;
using LinkPara.IKS.Application.Commons.Models.IKSModels;
using LinkPara.IKS.Application.Commons.Models.IKSModels.Annulments.Response;
using MediatR;

namespace LinkPara.IKS.Application.Features.Annulments.Queries.GetAnnulmentCodes
{
    public class GetAnnulmentCodesQuery : IRequest<IKSResponse<AnnulmentCodesResponse>>
    {
    }

    public class GetAnnulmentCodesQueryHandler : IRequestHandler<GetAnnulmentCodesQuery, IKSResponse<AnnulmentCodesResponse>>
    {
        private readonly IIKSService _iKSService;

        public GetAnnulmentCodesQueryHandler(IIKSService iKSService)
        {
            _iKSService = iKSService;
        }

        public async Task<IKSResponse<AnnulmentCodesResponse>> Handle(GetAnnulmentCodesQuery request, CancellationToken cancellationToken)
        {
            return await _iKSService.GetAnnulmentCodesAsync();
        }
    }
}
