using AutoMapper;
using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Application.Features.Partners;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Emoney.Infrastructure.Services;

public class ApiKeyService : IApiKeyService
{
    private readonly IGenericRepository<ApiKey> _repository;
    private readonly IMapper _mapper;

    public ApiKeyService(IGenericRepository<ApiKey> repository,
        IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<ApiKeyDto> GetApiKey(string publicKey)
    {
        var apiKey = await _repository
            .GetAll()
            .Include(s => s.Partner)
            .SingleOrDefaultAsync(s => s.PublicKey == publicKey);

        if (apiKey is null)
        {
            throw new NotFoundException(nameof(ApiKey), publicKey);
        }

        return _mapper.Map<ApiKeyDto>(apiKey);
    }
}
