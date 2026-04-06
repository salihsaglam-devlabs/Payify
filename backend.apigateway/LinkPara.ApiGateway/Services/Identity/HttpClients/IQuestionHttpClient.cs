using LinkPara.ApiGateway.Services.Identity.Models.Requests;
using LinkPara.ApiGateway.Services.Identity.Models.Responses;

namespace LinkPara.ApiGateway.Services.Identity.HttpClients;

public interface IQuestionHttpClient
{
    Task<List<SecurityQuestionDto>> GetAllQuestionsAsync();
    Task<SecurityQuestionDto> GetUserQuestionAsync(string userId);
    Task SaveAnswerAsync(UserAnswerRequest request);
    Task<bool> ValidateAnswerAsync(UserAnswerRequest user);
    Task UpdateAnswerAsync(UpdateUserAnswerRequest request);
    Task<bool> ValidateUserAnswerAsync(ValidateUserQuestionAnswerRequest request);
    Task<string> GetUserQuestionByPhoneNumberAsync (string phoneNumber);
}