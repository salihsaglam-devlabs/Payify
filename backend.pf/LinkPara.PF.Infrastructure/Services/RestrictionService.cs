using LinkPara.Cache;
using LinkPara.ContextProvider;
using LinkPara.HttpProviders.Identity.Models.Enums;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinkPara.PF.Infrastructure.Services;

public class RestrictionService : IRestrictionService
{
    private readonly ILogger<RestrictionService> _logger;
    private readonly IContextProvider _contextProvider;
    private readonly IGenericRepository<MerchantUser> _merchantUserRepository;
    private readonly IGenericRepository<SubMerchantUser> _subMerchantUserRepository;
    private readonly ICacheService _cacheService;

    public RestrictionService(
        ILogger<RestrictionService> logger,
        IContextProvider contextProvider,
        IGenericRepository<MerchantUser> merchantUserRepository,
        IGenericRepository<SubMerchantUser> subMerchantUserRepository,
        ICacheService cacheService)
    {
        _logger = logger;
        _contextProvider = contextProvider;
        _merchantUserRepository = merchantUserRepository;
        _subMerchantUserRepository = subMerchantUserRepository;
        _cacheService = cacheService;
    }

    public async Task IsUserAuthorizedAsync(Guid merchantId)
    {
        var userId = _contextProvider.CurrentContext.UserId != null ? 
            Guid.Parse(_contextProvider.CurrentContext.UserId) : Guid.Empty;

        var userType = _contextProvider.CurrentContext.UserType ?? UserType.Individual.ToString();
        
        if (userType == UserType.Internal.ToString() || userType == UserType.ApplicationUser.ToString())
        {
            return;
        }

        if (userType == UserType.CorporateSubMerchant.ToString())
        {
            var subMerchantUser = await _cacheService.GetOrCreateAsync($"SUB-{merchantId.ToString()}-{userId.ToString()}",
                async () =>
                {
                    return await _subMerchantUserRepository
                        .GetAll()
                        .Include(s => s.SubMerchant)
                        .FirstOrDefaultAsync(s => s.UserId == userId && s.SubMerchant.MerchantId == merchantId);
                });

            if (subMerchantUser is null)
            {
                _logger.LogError($"User({userId.ToString()}) tried to access Merchant({merchantId.ToString()}) data without authorization!");
                throw new ForbiddenAccessException();
            }
        }
        else
        {
            var merchantUser = await _cacheService.GetOrCreateAsync($"{merchantId.ToString()}-{userId.ToString()}",
                async () =>
                {
                    var merchantUser = await _merchantUserRepository
                        .GetAll()
                        .Include(s => s.Merchant)
                        .FirstOrDefaultAsync(s => s.UserId == userId);

                    if (merchantUser is not null)
                    {
                        if (merchantUser.MerchantId == merchantId)
                        {
                            return merchantUser; 
                        }

                        var subMerchant = await _merchantUserRepository
                            .GetAll()
                            .Include(m => m.Merchant)
                            .FirstOrDefaultAsync(m => m.MerchantId == merchantId);
                    
                        if (subMerchant.Merchant.ParentMerchantId == merchantUser.MerchantId)
                        {
                            return subMerchant;
                        }
                    }
                    return null;
                });
            
            if (merchantUser is null)
            {
                _logger.LogError($"User({userId.ToString()}) tried to access Merchant({merchantId.ToString()}) data without authorization!");
                throw new ForbiddenAccessException();
            }
        }
    }
    
    public async Task RestrictMerchantTypes(List<MerchantType> merchantTypes)
    {
        var userId = _contextProvider.CurrentContext.UserId != null ? 
            Guid.Parse(_contextProvider.CurrentContext.UserId) : Guid.Empty;

        var userType = _contextProvider.CurrentContext.UserType ?? UserType.Individual.ToString();
        
        if (userType == UserType.Internal.ToString() || userType == UserType.ApplicationUser.ToString())
        {
            return;
        }

        if (userType == UserType.CorporateSubMerchant.ToString())
        {
            if (merchantTypes.Contains(MerchantType.EasyMerchant))
            {
                _logger.LogError($"User({userId.ToString()}) tried to access not authorized MerchantType services!");
                throw new ForbiddenAccessException();
            }
        }
        else
        {
            var currentMerchantType = await _cacheService.GetOrCreateAsync($"MerchantType-{userId.ToString()}",
                async () =>
                {
                    return await _merchantUserRepository
                        .GetAll()
                        .Include(s => s.Merchant)
                        .Where(s => s.UserId == userId)
                        .Select(s => s.Merchant.MerchantType)
                        .FirstOrDefaultAsync();
                });

            if (merchantTypes.Contains(currentMerchantType))
            {
                _logger.LogError($"User({userId.ToString()}) tried to access not authorized MerchantType services!");
                throw new ForbiddenAccessException();
            }
        }
    }
}