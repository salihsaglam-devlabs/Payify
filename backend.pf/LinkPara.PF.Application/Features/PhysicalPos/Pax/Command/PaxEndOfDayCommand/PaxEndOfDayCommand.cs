using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Interfaces.PhysicalPos;
using LinkPara.PF.Application.Commons.Models.PhysicalPos.Response;
using MediatR;

namespace LinkPara.PF.Application.Features.PhysicalPos.Pax.Command.PaxEndOfDayCommand;

public class PaxEndOfDayCommand : IRequest<PaxEndOfDayResponse>, IClientApiCommand
{
    public string BatchId { get; set; }
    public string BankId { get; set; }
    public string MerchantId { get; set; }
    public string TerminalId { get; set; }
    public int Date { get; set; }
    public int SaleCount { get; set; }
    public int VoidCount { get; set; }
    public int RefundCount { get; set; }
    public int InstallmentSaleCount { get; set; }
    public int SaleAmount { get; set; }
    public int VoidAmount { get; set; }
    public int RefundAmount { get; set; }
    public int InstallmentSaleAmount { get; set; }
    public string Currency { get; set; }
    public int InstitutionId { get; set; }
    public string Vendor { get; set; }
    public Guid PfMerchantId { get; set; }
    public string ConversationId { get; set; }
    public string ClientIpAddress { get; set; }
    public string SerialNumber { get; set; }
    public string Gateway { get; set; }
}

public class PaxEndOfDayCommandHandler : IRequestHandler<PaxEndOfDayCommand, PaxEndOfDayResponse>
{
    private readonly IPaxPosService _paxPosService;

    public PaxEndOfDayCommandHandler(IPaxPosService paxPosService)
    {
        _paxPosService = paxPosService;
    }

    public async Task<PaxEndOfDayResponse> Handle(PaxEndOfDayCommand request, CancellationToken cancellationToken)
    {
        return await _paxPosService.EndOfDayAsync(request);
    }
}
