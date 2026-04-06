using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinkPara.Emoney.Infrastructure.Services;

public class BankService : IBankService
{
    private readonly IGenericRepository<Bank> _repository;
    private readonly IGenericRepository<BankLogo> _bankLogoRepository;

    public BankService(ILogger<BankService> logger,
        IGenericRepository<Bank> repository,
        IGenericRepository<BankLogo> bankLogoRepository)
    {
        _repository = repository;
        _bankLogoRepository = bankLogoRepository;
    }

    public async Task<Bank> GetBankAsync(string idOrCode)
    {
        return await _repository.GetAll()
            .SingleOrDefaultAsync(x => x.Id.ToString() == idOrCode
                                    || x.Code.ToString() == idOrCode);
    }
    public async Task<List<Bank>> GetBanksAsync()
    {
        var bankLogos = _bankLogoRepository.GetAll();

        return await _repository.GetAll()
            .Select(x => new Bank
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                HasLogo = bankLogos.Any(q => q.BankId == x.Id),
                CreatedBy = x.CreatedBy,
                CreateDate = x.CreateDate,
                LastModifiedBy = x.LastModifiedBy,
                RecordStatus = x.RecordStatus,
                UpdateDate = x.UpdateDate
            }).ToListAsync();
    }

    public async Task<List<Bank>> ResolveBankFromIbanAsync(string iban)
    {
        iban = iban.Trim().Replace(" ", "")
            .ToUpper().Replace("TR", "")
            .Substring(4, 3);

        var bankLogos = _bankLogoRepository.GetAll();

        var result = await _repository.GetAll()
            .Where(x => x.Code == Convert.ToInt32(iban))
            .Select(q => new Bank
            {
                Id = q.Id,
                Code = q.Code,
                Name = q.Name,
                HasLogo = bankLogos.Any(y => y.BankId == q.Id),
                CreatedBy = q.CreatedBy,
                CreateDate = q.CreateDate,
                LastModifiedBy = q.LastModifiedBy,
                RecordStatus = q.RecordStatus,
                UpdateDate = q.UpdateDate
            }).ToListAsync();

        return result;
    }
}