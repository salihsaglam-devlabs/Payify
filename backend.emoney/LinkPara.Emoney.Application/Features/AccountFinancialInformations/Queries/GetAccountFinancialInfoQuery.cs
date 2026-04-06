using AutoMapper;
using LinkPara.Emoney.Application.Commons.Helpers;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinkPara.Emoney.Application.Features.AccountFinancialInformations.Queries;

public class GetAccountFinancialInfoQuery : IRequest<AccountFinancialInfoDto>
{
    public Guid AccountId { get; set; }
}

public class GetAccountFinancialInformationQueryHandler : IRequestHandler<GetAccountFinancialInfoQuery, AccountFinancialInfoDto>
{
    private readonly IGenericRepository<AccountFinancialInformation> _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAccountFinancialInfoQuery> _logger;
    public GetAccountFinancialInformationQueryHandler(
        IGenericRepository<AccountFinancialInformation> repository,
        IMapper mapper,
        ILogger<GetAccountFinancialInfoQuery> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }
    public async Task<AccountFinancialInfoDto> Handle(GetAccountFinancialInfoQuery request, CancellationToken cancellationToken)
    {
        var accountFinancial = await _repository.GetAll().FirstOrDefaultAsync(s => s.AccountId == request.AccountId);

        accountFinancial.CheckAndThrowIfNull(request.AccountId, _logger);

        return _mapper.Map<AccountFinancialInfoDto>(accountFinancial);
    }
}
