using LinkPara.PF.Application.Commons.Models.Merchants;
using LinkPara.PF.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.PF.Application.Features.BankAccounts.Queries.GetBankAccountByMerchantId;

public class GetBankAccountByMerchantIdQuery : SearchQueryParams, IRequest<MerchantBankAccountDto>
{
    public Guid MerchantId { get; set; }
}

public class GetBankAccountByMerchantIdQueryHandler : IRequestHandler<GetBankAccountByMerchantIdQuery, MerchantBankAccountDto>
{
    private readonly IGenericRepository<MerchantBankAccount> _merchantBankAccountRepository;

    public GetBankAccountByMerchantIdQueryHandler(IGenericRepository<MerchantBankAccount> merchantBankAccountRepository)
    {
        _merchantBankAccountRepository = merchantBankAccountRepository;
    }

    public async Task<MerchantBankAccountDto> Handle(GetBankAccountByMerchantIdQuery request, CancellationToken cancellationToken)
    {
        var merchantBankAccount = await _merchantBankAccountRepository.GetAll()
            .Where(b => b.MerchantId == request.MerchantId && b.RecordStatus == RecordStatus.Active)
            .OrderByDescending(b => b.CreateDate).FirstOrDefaultAsync(cancellationToken);

        if (merchantBankAccount == null) 
        {
            throw new NotFoundException(nameof(MerchantBankAccount) + " Merchant Id:", request.MerchantId);
        }

        return new MerchantBankAccountDto 
        { 
            Iban = merchantBankAccount.Iban, 
            BankCode = merchantBankAccount.BankCode,
            MerchantId = merchantBankAccount.MerchantId,
            RecordStatus = merchantBankAccount.RecordStatus
        };
    }
}
