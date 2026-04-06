using LinkPara.IKS.Application.Commons.Interfaces;
using LinkPara.IKS.Application.Commons.Models.IKSModels;
using MediatR;
using LinkPara.IKS.Application.Commons.Models.IKSModels.Annulments.Response;
using LinkPara.IKS.Application.Commons.Models.IKSModels.Annulments.Request;
using AutoMapper;
using LinkPara.IKS.Application.Enums;

namespace LinkPara.IKS.Application.Features.Annuments.Command.SaveAnnulment
{
    public class SaveAnnulmentCommand : IRequest<IKSResponse<AnnulmentResponse>>
    {
        public Guid MerchantId { get; set; }
        public string GlobalMerchantId { get; set; }
        public string Code { get; set; }
        public string OwnerIdentityNo { get; set; }
        public string Explanation { get; set; }
        public string Partner2IdentityNo { get; set; }
        public string Partner3IdentityNo { get; set; }
        public string Partner4IdentityNo { get; set; }
        public string Partner5IdentityNo { get; set; }
    }
    public class SaveAnnulmentCommandHandler : IRequestHandler<SaveAnnulmentCommand, IKSResponse<AnnulmentResponse>>
    {
        private readonly IIKSService _iKSService;
        private readonly IMapper _mapper;

        public SaveAnnulmentCommandHandler(IIKSService ıKSService,
            IMapper mapper)
        {
            _iKSService = ıKSService;
            _mapper = mapper;
        }
        public async Task<IKSResponse<AnnulmentResponse>> Handle(SaveAnnulmentCommand request, CancellationToken cancellationToken)
        {
            var annulmentMap = _mapper.Map<SaveAnnulmentRequest>(request);
            annulmentMap.ActivityType = ((int)ActivitType.ActivityType).ToString();

            annulmentMap.InformType = ((int)InformType.InformTypeAnnulment).ToString();

            annulmentMap.Date = DateTime.Now.ToString("dd.MM.yyyy");

            var result = await _iKSService.SaveAnnulmentAsync(annulmentMap);

            var data = _mapper.Map<AnnulmentResponse>(result?.Data?.annulment);

            return new IKSResponse<AnnulmentResponse>()
            {
                Error = result.Error,
                Data = data
            };
        }
    }
}
