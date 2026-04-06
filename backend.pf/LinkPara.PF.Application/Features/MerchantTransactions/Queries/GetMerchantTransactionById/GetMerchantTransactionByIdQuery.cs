using LinkPara.PF.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.PF.Application.Features.MerchantTransactions.Queries.GetMerchantTransactionById;

public class GetMerchantTransactionByIdQuery : IRequest<MerchantTransactionDto>
{
    public Guid Id { get; set; }
}

public class GetMerchantTransactionByIdQueryHandler : IRequestHandler<GetMerchantTransactionByIdQuery, MerchantTransactionDto>
{
    private readonly IMerchantService _merchantService;

    public GetMerchantTransactionByIdQueryHandler(IMerchantService merchantService)
    {

        _merchantService = merchantService;
    }
    public async Task<MerchantTransactionDto> Handle(GetMerchantTransactionByIdQuery request, CancellationToken cancellationToken)
    {
        return await _merchantService.GetMerchantTransactionByIdAsync(request.Id);
    }
}
