using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Features.MerchantStatements;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Pagination;
using MediatR;

namespace LinkPara.PF.Application.Features.Statements.Queries.GetMerchantStatementDetail;

public class GetMerchantStatementDetailQuery : SearchQueryParams, IRequest<PaginatedList<MerchantStatementDto>>
{
    public Guid? MerchantId { get; set; }
    public DateTime? StatementStartDate { get; set; }
    public DateTime? StatementEndDate { get; set; }
    public string MailAddress { get; set; }
    public int? StatementMonth { get; set; }
    public int? StatementYear { get; set; }
    public MerchantStatementStatus? StatementStatus { get; set; }
    public MerchantStatementType? StatementType { get; set; }
    public string ReceiptNumber { get; set; }

}

public class GetMerchantStatementDetailQueryHandler : IRequestHandler<GetMerchantStatementDetailQuery, PaginatedList<MerchantStatementDto>>
{    
    private readonly IMerchantStatementService _merchantStatementService;

    public GetMerchantStatementDetailQueryHandler(IMerchantStatementService merchantStatementService)
    {
        _merchantStatementService = merchantStatementService;
    }
    public async Task<PaginatedList<MerchantStatementDto>> Handle(GetMerchantStatementDetailQuery request, CancellationToken cancellationToken)
    {
        return await _merchantStatementService.GetPaginatedMerchantStatementsAsync(request);
    }
}
