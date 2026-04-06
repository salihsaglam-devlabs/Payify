
using AutoMapper;
using LinkPara.PF.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.PF.Application.Features.Merchants.Queries.GetApiKeysByMerchantNumber;

public class GetApiKeysByMerchantNumberQuery : IRequest<MerchantApiKeyDto>
{
    public string MerchantNumber { get; set; }
}

public class GetApiKeysByMerchantIdQueryHandler : IRequestHandler<GetApiKeysByMerchantNumberQuery, MerchantApiKeyDto>
{
    private readonly IGenericRepository<MerchantApiKey> _repository;
    private readonly IGenericRepository<Merchant> _merchantRepository;
    private readonly IMapper _mapper;

    public GetApiKeysByMerchantIdQueryHandler(IGenericRepository<MerchantApiKey> repository,
        IGenericRepository<Merchant> merchantRepository,
        IMapper mapper)
    {
        _repository = repository;
        _merchantRepository = merchantRepository;
        _mapper = mapper;
    }

    public async Task<MerchantApiKeyDto> Handle(GetApiKeysByMerchantNumberQuery request, CancellationToken cancellationToken)
    {
        var merchant = await _merchantRepository.GetAll()
            .FirstOrDefaultAsync(q => q.Number == request.MerchantNumber, cancellationToken);

        if (merchant is null)
        {
            throw new NotFoundException(nameof(Merchant), request.MerchantNumber);
        }

        var apiKeys = await _repository.GetAll()
            .SingleOrDefaultAsync(w => w.MerchantId == merchant.Id
                , cancellationToken: cancellationToken);

        if (apiKeys is null)
        {
            throw new NotFoundException(nameof(MerchantApiKey), merchant.Id);
        }

        var map = _mapper.Map<MerchantApiKey, MerchantApiKeyDto>(apiKeys);
        map.MerchantNumber = merchant.Number;

        return map;
    }
}