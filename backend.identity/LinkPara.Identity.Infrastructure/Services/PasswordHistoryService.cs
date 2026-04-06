using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Domain.Entities;
using LinkPara.SharedModels.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using LinkPara.HttpProviders.Vault;

namespace LinkPara.Identity.Infrastructure.Services;

public class PasswordHistoryService : IPasswordHistoryService
{
    private readonly IRepository<UserPasswordHistory> _repository;
    private readonly IVaultClient _vaultClient;

    public PasswordHistoryService(IRepository<UserPasswordHistory> repository,
         IVaultClient vaultClient)
    {
        _repository = repository;
        _vaultClient = vaultClient;
    }

    public async Task<List<UserPasswordHistory>> GetOldPasswordsAsync(User user)
    {
        return await _repository.GetAll()
            .Where(p =>     
                p.UserId == user.Id && 
                p.RecordStatus == RecordStatus.Active &&
                p.PasswordHash != null)
            .ToListAsync();
    }

    public async Task SavePasswordAsync(User user, string oldHashedPassword)
    {
        var userPasswordHistory = new UserPasswordHistory
        {
            User = user,
            UserId = user.Id,
            PasswordHash = oldHashedPassword,
            CreatedBy = user.Id.ToString()
        };
        
        await _repository.AddAsync(userPasswordHistory);

        var oldHashedPasswords = await _repository.GetAll()
            .Where(p => p.UserId == user.Id
                        && p.RecordStatus == RecordStatus.Active)
            .OrderByDescending(p => p.CreateDate)
            .ToListAsync();
        
        var requiredPastPasswordCount = _vaultClient.GetSecretValue<int>("IdentitySecrets", "PasswordSettings", "RequiredPastPasswordCount");
        if (oldHashedPasswords.Count > requiredPastPasswordCount)
        {
            var _list = oldHashedPasswords
                 .Skip(requiredPastPasswordCount)
                 .ToList();

            foreach (var item in _list)
            {
                await _repository.DeleteAsync(item);
            }
        }
    }
}