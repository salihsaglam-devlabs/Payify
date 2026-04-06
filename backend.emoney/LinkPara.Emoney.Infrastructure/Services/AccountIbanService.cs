using LinkPara.ContextProvider;
using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Infrastructure.Persistence;
using LinkPara.HttpProviders.KKB;
using LinkPara.HttpProviders.KKB.Models;
using LinkPara.SharedModels.Persistence;
using LinkPara.SystemUser;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LinkPara.Emoney.Infrastructure.Services;

public class AccountIbanService : IAccountIbanService
{
    private readonly IKKBService _kkbService;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AccountIbanService> _logger;
    private readonly IApplicationUserService _applicationUserService;

    public AccountIbanService(
        IKKBService kkbService, 
        IServiceScopeFactory scopeFactory, 
        ILogger<AccountIbanService> logger,
        IApplicationUserService applicationUserService)
    {
        _kkbService = kkbService;
        _scopeFactory = scopeFactory;
        _logger = logger;
        _applicationUserService = applicationUserService;
    }
    public async Task<bool> ValidateIbanAsync(string identityNo, string iban)
    {
        var accountIban = await GetKkbResultFromTableAsync(identityNo, iban);
        
        if (accountIban != null)
        {
            return true;
        }

        try
        {
            if (string.IsNullOrEmpty(identityNo) || string.IsNullOrEmpty(iban))
            {
                return false;
            }
            
            var kkbResult = await _kkbService.ValidateIban(
                new ValidateIbanRequest
                {
                    Iban = iban,
                    IdentityNo = identityNo
                });
            
            if (kkbResult.IsValid)
            {
                await SaveKkbResultAsync(identityNo, iban);
            }

            return kkbResult.IsValid;
        }
        catch (Exception e)
        {
           _logger.LogError($"ValidateIbanError: {e}");
        }

        return false;
    }

    private async Task<AccountIban> GetKkbResultFromTableAsync(string identityNo, string iban)
    {
        AccountIban accountIban = null;
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<EmoneyDbContext>();
            
            accountIban = await dbContext.AccountIban
                .Where(s =>
                    s.Iban.Equals(iban)
                    && s.IdentityNo.Equals(identityNo))
                .FirstOrDefaultAsync();
        }
        catch (Exception e)
        {
            _logger.LogError($"GetKkbResultFromTableAsync Error: {e}");
        }
        
        return accountIban;
    }

    private async Task SaveKkbResultAsync(string identityNo, string iban)
    {
        try
        {
            var accountIban = new AccountIban
            {
                CreateDate = DateTime.Now,
                UpdateDate = DateTime.Now,
                CreatedBy = _applicationUserService?.ApplicationUserId.ToString(),
                LastModifiedBy = _applicationUserService?.ApplicationUserId.ToString(),
                RecordStatus = RecordStatus.Active,
                IdentityNo = identityNo,
                Iban = iban
            };

            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<EmoneyDbContext>();
            
            dbContext.AccountIban.Add(accountIban);
            await dbContext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            _logger.LogError($"SaveKkbResultAsync Error: {e}");
        }
    }
}