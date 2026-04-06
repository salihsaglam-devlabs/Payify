using AutoMapper;
using LinkPara.PF.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.PF.Application.Features.Banks.Queries.GetBankApiKey;

public class GetBankApiKeyQuery : IRequest<List<BankApiKeyDto>>
{
    public Guid AcquireBankId { get; set; }
}

public class GetBankApiKeyQueryHandler : IRequestHandler<GetBankApiKeyQuery, List<BankApiKeyDto>>
{
    private readonly IGenericRepository<BankApiKey> _repository;
    private readonly IGenericRepository<AcquireBank> _acquireRepository;
    private readonly IMapper _mapper;

    public GetBankApiKeyQueryHandler(IGenericRepository<BankApiKey> repository, IGenericRepository<AcquireBank> acquireRepository, IMapper mapper)
    {
        _repository = repository;
        _acquireRepository = acquireRepository;
        _mapper = mapper;
    }
    public async Task<List<BankApiKeyDto>> Handle(GetBankApiKeyQuery request, CancellationToken cancellationToken)
    {
        var acquireBank = await _acquireRepository.GetByIdAsync(request.AcquireBankId);

        if (acquireBank is null)
        {
            throw new NotFoundException(nameof(AcquireBank), request.AcquireBankId);
        }

        var bankApiKeys = await _repository.GetAll()
            .Where(b => b.AcquireBankId == request.AcquireBankId)
            .OrderBy(b=>b.Key).ToListAsync(cancellationToken);

        return _mapper.Map<List<BankApiKeyDto>>(bankApiKeys);
    }
}
