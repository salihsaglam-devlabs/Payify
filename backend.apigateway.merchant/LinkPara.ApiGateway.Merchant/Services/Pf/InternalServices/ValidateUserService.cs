using LinkPara.ApiGateway.Merchant.Services.Identity.Models.Enums;
using LinkPara.ApiGateway.Merchant.Services.Pf.HttpClients;
using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests;
using LinkPara.ContextProvider;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.Merchant.Services.Pf.InternalServices;

public class ValidateUserService : IValidateUserService
{
    private readonly IMerchantHttpClient _merchantHttpClient;
    private readonly IMerchantUserHttpClient _merchantUserHttpClient;
    private readonly ISubMerchantUserHttpClient _subMerchantUserHttpClient;
    private readonly IContextProvider _contextProvider;

    public ValidateUserService(IMerchantHttpClient merchantHttpClient,
        IMerchantUserHttpClient merchantUserHttpClient,
        ISubMerchantUserHttpClient subMerchantUserHttpClient,
        IContextProvider contextProvider)
    {
        _merchantHttpClient = merchantHttpClient;
        _merchantUserHttpClient = merchantUserHttpClient;
        _subMerchantUserHttpClient = subMerchantUserHttpClient;
        _contextProvider = contextProvider;
    }

    public async Task ValidateUserAsync(string publicKey, string userId)
    {
        var userType = _contextProvider.CurrentContext.UserType;

        if (userType == UserType.CorporateSubMerchant.ToString())
        {
            await ValidateSubMerchantUserAsync(publicKey, userId);
        }
        else
        {
            await ValidateMerchantUserAsync(publicKey, userId);
        }

        await Task.CompletedTask;
    }

    private async Task ValidateSubMerchantUserAsync(string publicKey, string userId)
    {
        var apiKeys = await _merchantHttpClient.GetApiKeysAsync(publicKey);

        var merchantUser = await _subMerchantUserHttpClient.GetAllAsync(new GetAllSubMerchantUserRequest
        {
            MerchantId = apiKeys.MerchantId,
            UserId = Guid.Parse(userId),
            RecordStatus = RecordStatus.Active
        });

        if (merchantUser.TotalCount == 0)
        {
            throw new ForbiddenAccessException();
        }

        await Task.CompletedTask;
    }

    private async Task ValidateMerchantUserAsync(string publicKey, string userId)
    {
        var apiKeys = await _merchantHttpClient.GetApiKeysAsync(publicKey);

        var merchantUser = await _merchantUserHttpClient.GetAllAsync(new GetAllMerchantUserRequest
        {
            MerchantId = apiKeys.MerchantId,
            UserId = Guid.Parse(userId),
            RecordStatus = RecordStatus.Active
        });

        if (merchantUser.TotalCount == 0)
        {
            throw new ForbiddenAccessException();
        }
    }
}
