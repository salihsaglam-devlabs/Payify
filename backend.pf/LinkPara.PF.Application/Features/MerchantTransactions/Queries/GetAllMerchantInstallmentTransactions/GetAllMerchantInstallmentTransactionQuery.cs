using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.SharedModels.Pagination;
using MediatR;

namespace LinkPara.PF.Application.Features.MerchantTransactions.Queries.GetAllMerchantInstallmentTransactions;

public class GetAllMerchantInstallmentTransactionQuery : SearchQueryParams, IRequest<PaginatedList<MerchantInstallmentTransactionDto>>
{
    public Guid MerchantTransactionId { get; set; }
}

public class GetAllMerchantInstallmentTransactionQueryHandler : IRequestHandler<GetAllMerchantInstallmentTransactionQuery, PaginatedList<MerchantInstallmentTransactionDto>>
{
    private readonly IMerchantService _merchantService;

    public GetAllMerchantInstallmentTransactionQueryHandler(IMerchantService merchantService)
    {

        _merchantService = merchantService;
    }

    public async Task<PaginatedList<MerchantInstallmentTransactionDto>> Handle(GetAllMerchantInstallmentTransactionQuery request, CancellationToken cancellationToken)
    {
        return await _merchantService.GetMerchantInstallmentTransactionList(request);
    }
}
