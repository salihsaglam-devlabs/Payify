using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Interfaces.PhysicalPos;
using LinkPara.PF.Application.Commons.Models.PhysicalPos.Response;
using MediatR;

namespace LinkPara.PF.Application.Features.PhysicalPos.Pax.Command.PaxParameterCommand;

public class PaxParameterCommand : IRequest<PaxParameterResponse>, IClientApiCommand
{
    public int Date { get; set; }
    public string DeviceSerial { get; set; }
    public int InstitutionId { get; set; }
    public string Vendor { get; set; }
    public List<PaxAppInfo> AppInfo { get; set; }
    public Guid PfMerchantId { get; set; }
    public string ConversationId { get; set; }
    public string ClientIpAddress { get; set; }
    public string SerialNumber { get; set; }
    public string Gateway { get; set; }
}

public class PaxParameterCommandHandler : IRequestHandler<PaxParameterCommand, PaxParameterResponse>
{
    private readonly IPaxPosService _paxPosService;

    public PaxParameterCommandHandler(IPaxPosService paxPosService)
    {
        _paxPosService = paxPosService;
    }

    public async Task<PaxParameterResponse> Handle(PaxParameterCommand request, CancellationToken cancellationToken)
    {
        return await _paxPosService.ParametersAsync(request);
    }
}
