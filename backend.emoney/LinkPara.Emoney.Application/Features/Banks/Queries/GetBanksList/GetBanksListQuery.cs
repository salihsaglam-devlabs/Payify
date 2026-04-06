using AutoMapper;
using LinkPara.Emoney.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.Emoney.Application.Features.Banks.Queries.GetBanksList;

public class GetBanksListQuery : IRequest<List<BankDto>>
{
    public string Iban { get; set; }
}

public class GetBanksListQueryHandler : IRequestHandler<GetBanksListQuery, List<BankDto>>
{
    private readonly IBankService _bankService;
    private readonly IMapper _mapper;

    public GetBanksListQueryHandler(IBankService bankService,
        IMapper mapper)
    {
        _bankService = bankService;
        _mapper = mapper;

    }

    public async Task<List<BankDto>> Handle(GetBanksListQuery query, CancellationToken cancellationToken)
    {
        var banks = query.Iban is null ?
            await _bankService.GetBanksAsync() : 
            await _bankService.ResolveBankFromIbanAsync(query.Iban);

        return _mapper.Map<List<BankDto>>(banks);
    }
}