using LinkPara.Cache;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.PF.Infrastructure.Services;

public class CurrencyService : ICurrencyService
{
    private readonly IGenericRepository<Currency> _repository;
    private readonly ICacheService _cacheService;


    public CurrencyService(
        IGenericRepository<Currency> repository,
        ICacheService cacheService)
    {
        _repository = repository;
        _cacheService = cacheService;
    }

    public async Task<Currency> GetByCodeAsync(string currencyCode)
    {
        var currency = await _cacheService.GetOrCreateAsync(currencyCode,
             async () =>
             {
                 return await _repository
                .GetAll()
                .FirstOrDefaultAsync(s => s.Code == currencyCode);
             });

        if (currency is null)
        {
            throw new NotFoundException(nameof(Currency), currencyCode);
        }

        return currency;
    }

    public async Task<Currency> GetByNumberAsync(int currencyNumber)
    {
        var currency = await _cacheService.GetOrCreateAsync($"{nameof(Currency)}-{currencyNumber}",
             async () =>
             {
                 return await _repository
                .GetAll()
                .FirstOrDefaultAsync(s => s.Number == currencyNumber);
             });

        if (currency is null)
        {
            throw new NotFoundException(nameof(Currency), currencyNumber);
        }

        return currency;
    }
}
