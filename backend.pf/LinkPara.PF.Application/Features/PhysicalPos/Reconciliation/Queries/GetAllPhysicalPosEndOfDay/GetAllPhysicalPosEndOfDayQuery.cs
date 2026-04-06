using LinkPara.PF.Application.Commons.Interfaces.PhysicalPos;
using LinkPara.PF.Domain.Enums.PhysicalPos;
using LinkPara.SharedModels.Pagination;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LinkPara.PF.Application.Features.PhysicalPos.Reconciliation.Queries.GetAllPhysicalPosEndOfDay;

public class GetAllPhysicalPosEndOfDayQuery : SearchQueryParams, IRequest<PaginatedList<PhysicalPosEndOfDayDto>>
{
    public Guid? MerchantId { get; set; }
    public string BatchId { get; set; }
    public string PosMerchantId { get; set; }
    public string PosTerminalId { get; set; }
    public DateTime? DateStart { get; set; }
    public DateTime? DateEnd { get; set; }
    public int? SaleCount { get; set; }
    public int? VoidCount { get; set; }
    public int? RefundCount { get; set; }
    public int? InstallmentSaleCount { get; set; }
    public int? FailedCount { get; set; }
    public decimal? SaleAmount { get; set; }
    public decimal? VoidAmount { get; set; }
    public decimal? RefundAmount { get; set; }
    public decimal? InstallmentSaleAmount { get; set; }
    public string Currency { get; set; }
    public int? InstitutionId { get; set; }
    public string Vendor { get; set; }
    public string SerialNumber { get; set; }
    public EndOfDayStatus? Status { get; set; }
}

public class GetAllPhysicalPosEndOfDayQueryHandler : IRequestHandler<GetAllPhysicalPosEndOfDayQuery, PaginatedList<PhysicalPosEndOfDayDto>>
{
    private readonly IEndOfDayService _endOfDayService;
    private readonly ILogger<GetAllPhysicalPosEndOfDayQuery> _logger;

    public GetAllPhysicalPosEndOfDayQueryHandler(ILogger<GetAllPhysicalPosEndOfDayQuery> logger, IEndOfDayService endOfDayService)
    {
        _logger = logger;
        _endOfDayService = endOfDayService;
    }
    
    public async Task<PaginatedList<PhysicalPosEndOfDayDto>> Handle(GetAllPhysicalPosEndOfDayQuery request, CancellationToken cancellationToken)
    {
        try
        {
            return await _endOfDayService.GetAllPhysicalPosEndOfDayAsync(request);
        }
        catch (Exception exception)
        {
            _logger.LogError($"Get All End Of Day Records Failed: {exception}");
            throw;
        }
    }
}