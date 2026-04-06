using LinkPara.ApiGateway.Services.Identity.Models.Responses;

namespace LinkPara.ApiGateway.Services.Identity.HttpClients
{
    public interface IUserPictureHttpClient
    {
        Task<UserPictureDto> GetUserPictureAsync(string userId);
        Task PostUserPictureAsync(UserPictureDto userPicture);
    }
}