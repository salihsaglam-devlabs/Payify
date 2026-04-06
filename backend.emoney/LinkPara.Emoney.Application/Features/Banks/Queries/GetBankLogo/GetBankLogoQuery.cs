using LinkPara.Emoney.Domain.Entities;
using MediatR;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Emoney.Application.Features.Banks.Queries.GetBankLogo;

public class GetBankLogoQuery : IRequest<BankLogoDto>
{
    public Guid BankId { get; set; }
}

public class GetBankLogoQueryHandler : IRequestHandler<GetBankLogoQuery, BankLogoDto>
{
    private readonly IGenericRepository<BankLogo> _bankLogoRepository;

    public GetBankLogoQueryHandler(IGenericRepository<BankLogo> bankLogoRepository)
    {
        _bankLogoRepository = bankLogoRepository;
    }

    public async Task<BankLogoDto> Handle(GetBankLogoQuery query, CancellationToken cancellationToken)
    {
        var bankLogo = await _bankLogoRepository.GetAll().Select(x => new
        {
            x.Bytes,
            x.ContentType,
            x.BankId
        }).FirstOrDefaultAsync(x => x.BankId == query.BankId, cancellationToken);

        return new BankLogoDto()
        {
            Bytes = bankLogo?.Bytes,
            ContentType = bankLogo?.ContentType
        };
    }
}