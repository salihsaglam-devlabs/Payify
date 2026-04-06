using LinkPara.CampaignManagement.Application.Commons.Interfaces;
using LinkPara.CampaignManagement.Application.Commons.Models.Responses;
using LinkPara.CampaignManagement.Domain.Entities;
using LinkPara.SharedModels.Persistence;
using LinkPara.SystemUser;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.CampaignManagement.Infrastructure.Services;

public class AuthorizationService : IAuthorizationService
{
    private readonly IGenericRepository<AuthorizationToken> _repository;
    private readonly IApplicationUserService _applicationUserService;

    public AuthorizationService(IGenericRepository<AuthorizationToken> repository,
        IApplicationUserService applicationUserService)
    {
        _repository = repository;
        _applicationUserService = applicationUserService;
    }

    public async Task<string> GetActiveTokenAsync()
    {
        var dateTimeNow = DateTime.Now;

        var activeToken = await _repository.GetAll()
            .Where(x => x.RecordStatus == RecordStatus.Active
                     && x.RefreshTokenDate < dateTimeNow
                     && x.ExpiryDate > dateTimeNow)
            .FirstOrDefaultAsync();

        if (activeToken is null)
        {
            return string.Empty;
        }

        return activeToken.Token;

    }

    public async Task<string> RefreshTokenAsync(LoginResponse loginResult)
    {
        await DeleteUnactiveTokens();
        var autorizationToken = new AuthorizationToken
        {
            ExpiryDate = DateTime.Now.AddSeconds(loginResult.expires_in),
            Token = loginResult.token,
            RefreshTokenDate = DateTime.Now,
            CreatedBy = _applicationUserService.ApplicationUserId.ToString()
        };

        await _repository.AddAsync(autorizationToken);
        return autorizationToken.Token;
    }

    private async Task DeleteUnactiveTokens()
    {
        var dateTimeNow = DateTime.Now;

        var unactiveTokens = await _repository.GetAll()
            .Where(x => x.RecordStatus == RecordStatus.Active
                     && x.ExpiryDate < dateTimeNow)
            .ToListAsync();

        if (unactiveTokens is null)
        {
            return;
        }

        foreach (var item in unactiveTokens)
        {
            await _repository.DeleteAsync(item);
        }
    }
}
