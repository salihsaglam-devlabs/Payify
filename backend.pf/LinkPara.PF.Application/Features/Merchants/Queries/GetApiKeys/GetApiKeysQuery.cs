using System.Net;
using AutoMapper;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinkPara.PF.Application.Features.Merchants.Queries.GetApiKeys;

public class GetApiKeysQuery : IRequest<MerchantApiKeyDto>
{
    public string PublicKeyEncoded { get; set; }
}

public class GetApiKeysQueryHandler : IRequestHandler<GetApiKeysQuery, MerchantApiKeyDto>
{
    private readonly IGenericRepository<MerchantApiKey> _repository;
    private readonly IGenericRepository<Merchant> _merchantRepository;
    private readonly IMapper _mapper;
    
    public GetApiKeysQueryHandler(IGenericRepository<MerchantApiKey> repository,
        IGenericRepository<Merchant> merchantRepository,
        IMapper mapper)
    {
        _repository = repository;
        _merchantRepository = merchantRepository;
        _mapper = mapper;
    }

    public async Task<MerchantApiKeyDto> Handle(GetApiKeysQuery request, CancellationToken cancellationToken)
    {
        var encodedBytes = Convert.FromBase64String(request.PublicKeyEncoded);
        var publicKey = System.Text.Encoding.UTF8.GetString(encodedBytes);
               
        var apiKeys = await _repository.GetAll()
            .FirstOrDefaultAsync(w => w.PublicKey == publicKey.Trim()
                , cancellationToken: cancellationToken);

        if (apiKeys is null)
        {
            throw new NotFoundException(nameof(MerchantApiKey), request.PublicKeyEncoded);
        }

        var merchant = await _merchantRepository.GetByIdAsync(apiKeys.MerchantId);

        var map = _mapper.Map<MerchantApiKey, MerchantApiKeyDto>(apiKeys);
        map.MerchantNumber = merchant.Number;

        return map;
    }
}