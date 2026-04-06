using AutoMapper;
using LinkPara.IKS.Application.Commons.Interfaces;
using LinkPara.IKS.Application.Commons.Models.IKSModels;
using LinkPara.IKS.Application.Commons.Models.IKSModels.Terminal.Request;
using LinkPara.IKS.Application.Commons.Models.IKSModels.Terminal.Response;
using LinkPara.IKS.Application.Enums;
using MediatR;

namespace LinkPara.IKS.Application.Features.Terminal.Command.CreateTerminal;

public class CreateTerminalCommand : IRequest<IKSResponse<TerminalResponse>>
{
    public Guid MerchantId { get; set; }
    public Guid VposId { get; set; }
    public Guid PhysicalPosId { get; set; }
    public string GlobalMerchantId { get; set; }
    public string PspMerchantId { get; set; }
    public int OwnerPspNo { get; set; }
    public string HostingTaxNo { get; set; }
    public string HostingTradeName { get; set; }
    public string HostingUrl { get; set; }
    public string PaymentGwTaxNo { get; set; }
    public string PaymentGwTradeName { get; set; }
    public string PaymentGwUrl { get; set; }
    public string VirtualPosUrl { get; set; }
    public int ServiceProviderPspNo { get; set; }
    public string PfMainMerchantId { get; set; }
    public string BrandSharing { get; set; }
    public string Type { get; set; }
    public string BrandCode { get; set; }
    public string Model { get; set; }
    public string SerialNo { get; set; }
    public int? Contactless { get; set; }
    public int? PinPad { get; set; }
    public string ConnectionType { get; set; }
    public string FiscalNo { get; set; }
    public int TechPos { get; set; }
    public string OwnerTerminalId { get; set; }
}

public class CreateTerminalCommandHandler : IRequestHandler<CreateTerminalCommand, IKSResponse<TerminalResponse>>
{
    private readonly IIKSService _iksService;
    private readonly IMapper _mapper;
    
    public CreateTerminalCommandHandler(IIKSService iksService, IMapper mapper)
    {
        _iksService = iksService;
        _mapper = mapper;
    }
    
    public async Task<IKSResponse<TerminalResponse>> Handle(CreateTerminalCommand request, CancellationToken cancellationToken)
    {
        var terminalMap = _mapper.Map<CreateTerminalRequest>(request);
        terminalMap.StatusCode = ((int)StatusCode.StatusCode).ToString();

        var result = await _iksService.CreateTerminalAsync(terminalMap);

        var data = _mapper.Map<TerminalResponse>(result?.Data?.terminal);

        return new IKSResponse<TerminalResponse>()
        {
            Error = result?.Error,
            Data = data
        };
    }
}