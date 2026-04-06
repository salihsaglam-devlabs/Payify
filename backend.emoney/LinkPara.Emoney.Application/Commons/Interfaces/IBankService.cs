using LinkPara.Emoney.Domain.Entities;

namespace LinkPara.Emoney.Application.Commons.Interfaces;

public interface IBankService
{
    Task<List<Bank>> ResolveBankFromIbanAsync(string iban);
    Task<List<Bank>> GetBanksAsync();
    Task<Bank> GetBankAsync(string idOrCode);
}