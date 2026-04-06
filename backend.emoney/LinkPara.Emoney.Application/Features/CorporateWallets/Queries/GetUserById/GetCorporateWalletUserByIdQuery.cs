using AutoMapper;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.SharedModels.Persistence;
using MediatR;

namespace LinkPara.Emoney.Application.Features.CorporateWallets.Queries.GetUserById;

public class GetCorporateWalletUserByIdQuery : IRequest<CorporateWalletUserDto>
{
    public Guid Id { get; set; }
}

public class GetCorporateWalletUserByIdQueryHandler : IRequestHandler<GetCorporateWalletUserByIdQuery, CorporateWalletUserDto>
{
    private readonly IGenericRepository<AccountUser> _repository;
    private readonly IMapper _mapper;

    public GetCorporateWalletUserByIdQueryHandler(IMapper mapper, IGenericRepository<AccountUser> repository)
    {
        _mapper = mapper;
        _repository = repository;
    }

    public async Task<CorporateWalletUserDto> Handle(GetCorporateWalletUserByIdQuery request, CancellationToken cancellationToken)
    {
        var corporateWalletUser = await _repository.GetByIdAsync(request.Id);

        var result = _mapper.Map<CorporateWalletUserDto>(corporateWalletUser);
        return result;
    }
}
