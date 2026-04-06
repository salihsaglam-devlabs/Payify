using AutoMapper;
using LinkPara.ContextProvider;
using LinkPara.IKS.Application.Commons.Interfaces;
using LinkPara.IKS.Application.Commons.Models.IKSModels;
using LinkPara.IKS.Application.Commons.Models.IKSModels.Terminal.Request;
using LinkPara.IKS.Application.Commons.Models.IKSModels.Terminal.Response;
using LinkPara.IKS.Application.Enums;
using LinkPara.IKS.Domain.Entities;
using LinkPara.IKS.Domain.Enums;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinkPara.IKS.Application.Features.Terminal.Command.UpdateTerminal
{
    public class UpdateTerminalCommand : IRequest<IKSResponse<TerminalResponse>>
    {
        public Guid MerchantId { get; set; }
        public Guid VposId { get; set; }
        public string GlobalMerchantId { get; set; }
        public string PspMerchantId { get; set; }
        public string TerminalId { get; set; }
        public string OwnerTerminalId { get; set; }
        public string BrandSharing { get; set; }
        public int OwnerPspNo { get; set; }
        public string VirtualPosUrl { get; set; }
        public string HostingTaxNo { get; set; }
        public string HostingTradeName { get; set; }
        public string HostingUrl { get; set; }
        public string PaymentGwTaxNo { get; set; }
        public string PaymentGwTradeName { get; set; }
        public string PaymentGwUrl { get; set; }
        public int ServiceProviderPspNo { get; set; }
        public string StatusCode { get; set; }
        public string PfMainMerchantId { get; set; }
        public string ReferenceCode { get; set; }
        public string Type { get; set; }
        public string BrandCode { get; set; }
        public string Model { get; set; }
        public string SerialNo { get; set; }
        public int? Contactless { get; set; }
        public int? PinPad { get; set; }
        public string ConnectionType { get; set; }
        public string FiscalNo { get; set; }
        public int TechPos { get; set; }
    }
    public class UpdateTerminalCommandHandler : IRequestHandler<UpdateTerminalCommand, IKSResponse<TerminalResponse>>
    {
        private readonly IIKSService _iKSService;
        private readonly IGenericRepository<IksTerminal> _iksTerminalRepository;
        private readonly IGenericRepository<IksTerminalHistory> _iksTerminalHistoryRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<UpdateTerminalCommandHandler> _logger;
        public UpdateTerminalCommandHandler(IIKSService iKSService,
            IMapper mapper,
            IGenericRepository<IksTerminal> iksTerminalRepository,
            IGenericRepository<IksTerminalHistory> iksTerminalHistoryRepository,
            ILogger<UpdateTerminalCommandHandler> logger)
        {
            _iKSService = iKSService;
            _mapper = mapper;
            _iksTerminalRepository = iksTerminalRepository;
            _iksTerminalHistoryRepository = iksTerminalHistoryRepository;
            _logger = logger;
        }
        public async Task<IKSResponse<TerminalResponse>> Handle(UpdateTerminalCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var terminalMap = _mapper.Map<UpdateTerminalRequest>(request);
                terminalMap.StatusCode = ((int)StatusCode.PassiveStatusCode).ToString();

                var result = await _iKSService.UpdateTerminalAsync(terminalMap);

                var data = _mapper.Map<TerminalResponse>(result?.Data?.terminal);

                var iksTerminal = await _iksTerminalRepository.GetAll()
                    .Where(s => s.ReferenceCode == request.ReferenceCode && s.RecordStatus == RecordStatus.Active)
                    .FirstOrDefaultAsync(cancellationToken: cancellationToken);

                if (result?.Data?.terminal is not null && iksTerminal is not null)
                {
                    await _iKSService.CreateTerminalHistoryAsync(iksTerminal, result?.Data?.terminal);

                    iksTerminal = _iKSService.UpdatePassiveIksTerminalFields(iksTerminal, result?.Data?.terminal);
                    await _iksTerminalRepository.UpdateAsync(iksTerminal);
                }

                return new IKSResponse<TerminalResponse>()
                {
                    Error = result.Error,
                    Data = data
                };
            }
            catch (Exception exception)
            {
                _logger.LogError($"UpdateTerminalCommandError : {exception}");
                throw;
            }     
        }
    }
}
