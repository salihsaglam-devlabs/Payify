using LinkPara.ApiGateway.Commons.Extensions;
using LinkPara.ApiGateway.Commons.Helpers;
using LinkPara.ApiGateway.Services.Identity.Models.Requests;
using LinkPara.ApiGateway.Services.Identity.Models.Responses;
using System.Text.Json;

namespace LinkPara.ApiGateway.Services.Identity.HttpClients;

public class QuestionHttpClient : HttpClientBase, IQuestionHttpClient
{

    private readonly IServiceRequestConverter _serviceRequestConverter;
    public QuestionHttpClient(HttpClient client, IServiceRequestConverter serviceRequestConverter, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {
        _serviceRequestConverter = serviceRequestConverter;
    }


    public async Task<List<SecurityQuestionDto>> GetAllQuestionsAsync()
    {
        var response = await GetAsync($"v1/Questions");

        var responseString = await response.Content.ReadAsStringAsync();

        var questions = JsonSerializer.Deserialize<List<SecurityQuestionDto>>(responseString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return questions ?? throw new InvalidOperationException();

    }

    public async Task<SecurityQuestionDto> GetUserQuestionAsync(string userId)
    {

        var response = await GetAsync($"v1/Questions/{userId}/question");

        var responseString = await response.Content.ReadAsStringAsync();

        var questions = JsonSerializer.Deserialize<SecurityQuestionDto>(responseString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return questions;
    }

    public async Task<string> GetUserQuestionByPhoneNumberAsync(string phoneNumber)
    {
        var response = await GetAsync($"v1/Questions/user?phoneNumber={phoneNumber}");
        return await response.Content.ReadAsStringAsync();
    }

    public async Task SaveAnswerAsync(UserAnswerRequest request)
    {
        var clientRequest = _serviceRequestConverter.Convert<UserAnswerRequest, UserAnswerServiceRequest>(request);

        await PostAsJsonAsync($"/v1/Questions/{request.SecurityQuestionId}/answer", clientRequest);
    }

    public async Task UpdateAnswerAsync(UpdateUserAnswerRequest request)
    {
        var clientRequest = _serviceRequestConverter.Convert<UpdateUserAnswerRequest, UpdateUserAnswerServiceRequest>(request);

        await PutAsJsonAsync($"/v1/Questions/{clientRequest.UserId}/answer", clientRequest);
    }

    public async Task<bool> ValidateAnswerAsync(UserAnswerRequest user)
    {
        var request = new ValidateUserAnswerRequest { Answer = user.Answer };

        var clientRequest = _serviceRequestConverter.Convert<ValidateUserAnswerRequest, ValidateUserAnswerServiceRequest>(request);

        var response = await PostAsJsonAsync($"/v1/Questions/{user.SecurityQuestionId}/answer/validate", clientRequest);

        var responseString = await response.Content.ReadAsStringAsync();

        var verificationResult = JsonSerializer.Deserialize<bool>(responseString);

        return verificationResult;
    }

    public async Task<bool> ValidateUserAnswerAsync(ValidateUserQuestionAnswerRequest request)
    {
        var response = await PostAsJsonAsync($"/v1/Questions/validate/answer", request);
        var responseString = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<bool>(responseString);
    }
}
