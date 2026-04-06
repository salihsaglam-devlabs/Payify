using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Interfaces.PhysicalPos;
using LinkPara.PF.Application.Commons.Models.PhysicalPos.Response;
using MediatR;

namespace LinkPara.PF.Application.Features.PhysicalPos.Pax.Command.PaxTransactionCommand;

public class PaxTransactionCommand : IRequest<PaxTransactionResponse>, IClientApiCommand
{
    public string PaymentId { get; set; }
    public string BatchId { get; set; }
    public long Date { get; set; }
    public string Type { get; set; }
    public string Status { get; set; }
    public string Currency { get; set; }
    public string MerchantId { get; set; }
    public string TerminalId { get; set; }
    public int Amount { get; set; }
    public int PointAmount { get; set; }
    public int Installment { get; set; }
    public string MaskedCardNo { get; set; }
    public string BinNumber { get; set; }
    public string ProvisionNo { get; set; }
    public string AcquirerResponseCode { get; set; }
    public int InstitutionId { get; set; }
    public string Vendor { get; set; }
    public string Rrn { get; set; }
    public string Stan { get; set; }
    public string PosEntryMode { get; set; }
    public string PinEntryInfo { get; set; }
    public string BankRef { get; set; }
    public string OriginalRef { get; set; }
    public Guid PfMerchantId { get; set; }
    public string ConversationId { get; set; }
    public string ClientIpAddress { get; set; }
    public string SerialNumber { get; set; }
    public string Gateway { get; set; }
}

public class PaxTransactionCommandHandler : IRequestHandler<PaxTransactionCommand, PaxTransactionResponse>
{
    private readonly IPaxPosService _paxPosService;

    public PaxTransactionCommandHandler(IPaxPosService paxPosService)
    {
        _paxPosService = paxPosService;
    }

    public async Task<PaxTransactionResponse> Handle(PaxTransactionCommand request, CancellationToken cancellationToken)
    {
        return await _paxPosService.TransactionAsync(request);
    }
}
