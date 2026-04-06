using LinkPara.ApiGateway.Merchant.Commons.Models.IdentityModels;
using LinkPara.ApiGateway.Merchant.Services.Pf.HttpClients;

namespace LinkPara.ApiGateway.Merchant.Commons.Helpers;

public interface IUserNameGenerator
{
    Task<string> GetUserName(string userName, string channel);
    Task<string> GetUserName(string prefix, string phoneCode, string phoneNumber);
}

public class UserNameGenerator : IUserNameGenerator
{
    private const string MerchantPortal = "MerchantPortal";
    private readonly IMerchantUserHttpClient _merchantUserHttpClient;
    public UserNameGenerator(IMerchantUserHttpClient merchantUserHttpClient)
    {
        _merchantUserHttpClient = merchantUserHttpClient;
    }

    public async Task<string> GetUserName(string userName, string channel)
    {
        if (channel is not MerchantPortal)
        {
            return await Task.FromResult(string.Concat(UserTypePrefix.Internal, userName.Replace("+", "")));
        }
        var checkUserName = await _merchantUserHttpClient.GetUserNameAsync(userName);
        return checkUserName.UserName;
    }

    public async Task<string> GetUserName(string prefix, string phoneCode, string phoneNumber)
    {
        return await Task.FromResult(string.Concat(prefix, phoneCode, phoneNumber).Replace("+", ""));
    }
}
