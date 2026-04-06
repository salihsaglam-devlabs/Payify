using AutoMapper;
using LinkPara.IKS.Application.Commons.Interfaces;
using LinkPara.IKS.Application.Commons.Models.IKSModels;
using LinkPara.IKS.Application.Commons.Models.IKSModels.Terminal.Request;
using LinkPara.IKS.Application.Commons.Models.IKSModels.Terminal.Response;
using LinkPara.IKS.Application.Enums;
using MediatR;

namespace LinkPara.IKS.Application.Features.Terminal.Command.SaveTerminal
{
    public class SaveTerminalCommand : IRequest<IKSResponse<TerminalResponse>>
    {
        public Guid MerchantId { get; set; }
        public string GlobalMerchantId { get; set; }
        public string PspMerchantId { get; set; }
        public string TerminalId { get; set; }
        public string OwnerTerminalId { get; set; }
        public string BrandSharing { get; set; }
        public int OwnerPspNo { get; set; }
        public string VirtualPosUrl { get; set; }
        public string HostingTaxNo { get; set; }
        public string PaymentGwTaxNo { get; set; }
        public int ServiceProviderPspNo { get; set; }
    }
    public class SaveTerminalCommandHandler : IRequestHandler<SaveTerminalCommand, IKSResponse<TerminalResponse>>
    {
        private readonly IIKSService _iKSService;
        private readonly IMapper _mapper;
        private const string Type = "S";
        public SaveTerminalCommandHandler(IIKSService iKSService,
            IMapper mapper)
        {
            _iKSService = iKSService;
            _mapper = mapper;
        }
        public async Task<IKSResponse<TerminalResponse>> Handle(SaveTerminalCommand request, CancellationToken cancellationToken)
        {
            var terminalMap = _mapper.Map<SaveTerminalRequest>(request);
            terminalMap.StatusCode = ((int)StatusCode.StatusCode).ToString();
            terminalMap.TechPos = ((int)TechPos.TechPos);
            terminalMap.Type = Type;

            var result = await _iKSService.SaveTerminalAsync(terminalMap);

            var data = _mapper.Map<TerminalResponse>(result?.Data?.terminal);

            return new IKSResponse<TerminalResponse>()
            {
                Error = result.Error,
                Data = data
            };
        }
    }
}
