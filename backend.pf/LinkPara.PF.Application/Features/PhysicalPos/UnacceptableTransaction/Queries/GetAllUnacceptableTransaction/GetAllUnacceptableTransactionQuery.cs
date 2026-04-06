using LinkPara.PF.Application.Commons.Interfaces.PhysicalPos;
using LinkPara.PF.Domain.Enums.PhysicalPos;
using LinkPara.SharedModels.Pagination;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LinkPara.PF.Application.Features.PhysicalPos.UnacceptableTransaction.Queries.GetAllUnacceptableTransaction;

public class GetAllUnacceptableTransactionQuery : SearchQueryParams, IRequest<PaginatedList<PhysicalPosUnacceptableTransactionDto>>
{
    public string PaymentId { get; set; }
    public string BatchId { get; set; }
    public DateTime? DateStart { get; set; }
    public DateTime? DateEnd { get; set; }
    public string Type { get; set; }
    public string Status { get; set; }
    public string Currency { get; set; }
    public string MerchantId { get; set; }
    public string TerminalId { get; set; }
    public string BinNumber { get; set; }
    public string ProvisionNo { get; set; }
    public string Vendor { get; set; }
    public string Rrn { get; set; }
    public string Stan { get; set; }
    public string BankRef { get; set; }
    public string OriginalRef { get; set; }
    public Guid? PfMerchantId { get; set; }
    public string ConversationId { get; set; }
    public string SerialNumber { get; set; }
    public UnacceptableTransactionStatus? CurrentStatus {get; set;}
    public Guid? PhysicalPosEodId { get; set; }
    public EndOfDayStatus? EndOfDayStatus { get; set; }
}

public class GetAllUnacceptableTransactionQueryHandler : IRequestHandler<GetAllUnacceptableTransactionQuery, PaginatedList<PhysicalPosUnacceptableTransactionDto>>
{
    private readonly IUnacceptableTransactionService _unacceptableTransactionService;
    private readonly ILogger<GetAllUnacceptableTransactionQuery> _logger;

    public GetAllUnacceptableTransactionQueryHandler(IUnacceptableTransactionService unacceptableTransactionService, ILogger<GetAllUnacceptableTransactionQuery> logger)
    {
        _unacceptableTransactionService = unacceptableTransactionService;
        _logger = logger;
    }
    
    public async Task<PaginatedList<PhysicalPosUnacceptableTransactionDto>> Handle(GetAllUnacceptableTransactionQuery request, CancellationToken cancellationToken)
    {
        try
        {
            return await _unacceptableTransactionService.GetAllUnacceptableTransactionsAsync(request);
        }
        catch (Exception exception)
        {
            _logger.LogError($"Get All Unacceptable Transaction Failed: {exception}");
            throw;
        }
    }
}
