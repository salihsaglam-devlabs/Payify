using LinkPara.Emoney.Application.Commons.Models;

namespace LinkPara.Emoney.Application.Commons.Helpers;

public static class GetUserNameHelper
{
    public static async Task<string> GetUserNameAsync(string phoneCode, string phoneNumber)
    {
        return await Task.FromResult(
            string.Concat(UserTypePrefix.Individual, phoneCode, phoneNumber)
            .Replace("+", "")
            );
    }
    public static async Task<string> GetUserNameAsync(string userPrefix, string phoneCode, string phoneNumber)
    {
        return await Task.FromResult(
            string.Concat(userPrefix, phoneCode, phoneNumber)
            .Replace("+", "")
            );
    }
}
