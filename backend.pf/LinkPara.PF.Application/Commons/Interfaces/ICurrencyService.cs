using LinkPara.PF.Domain.Entities;

namespace LinkPara.PF.Application.Commons.Interfaces;

public interface ICurrencyService
{
    Task<Currency> GetByCodeAsync(string currencyCode); 
    Task<Currency> GetByNumberAsync(int currencyNumber);
}
