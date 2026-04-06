using LinkPara.ApiGateway.Services.Identity.HttpClients;
using LinkPara.ApiGateway.Services.Identity.Models.Requests;
using LinkPara.ApiGateway.Services.Identity.Models.Responses;
using MassTransit.Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.Controllers.Identity;

public class QuestionsController : ApiControllerBase
{
    private readonly IQuestionHttpClient _questionHttpClient;

    public QuestionsController(IQuestionHttpClient questionHttpClient)
    {
        _questionHttpClient = questionHttpClient;
    }

    /// <summary>
    /// Returns all questions.
    /// </summary>
    [AllowAnonymous]
    [HttpGet("")]
    public async Task<ActionResult<List<SecurityQuestionDto>>> GetAllQuestionsAsync()
    {
        return await _questionHttpClient.GetAllQuestionsAsync();
    }

    /// <summary>
    /// Returns the logged in users question.
    /// </summary>
    [Authorize(Policy = "Question:Read")]
    [HttpGet("me")]
    public async Task<ActionResult<SecurityQuestionDto>> GetQuestionAsync()
    {
        return await _questionHttpClient.GetUserQuestionAsync(UserId);
    }

    /// <summary>
    /// Updates security question answer of the user.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "RequireOtp")]
    [HttpPut("answer")]
    public async Task UpdateAnswerAsync(UpdateUserAnswerRequest request)
    {
        await _questionHttpClient.UpdateAnswerAsync(request);
    }

    /// <summary>
    /// Saves the users answer with the question.
    /// </summary>
    /// <param name="request"></param>
    [Authorize(Policy = "Question:Create")]
    [HttpPost("{questionId}/answer")]
    public async Task SaveAnswerAsync(UserAnswerRequest request)
    {
        await _questionHttpClient.SaveAnswerAsync(request);
    }

    /// <summary>
    /// Validates the trueness of the users answer.
    /// </summary>
    /// <param name="userAnswer"></param>
    [Authorize(Policy = "Question:Create")]
    [HttpPost("{questionId}/answer/validate")]
    public async Task<bool> ValidateAnswerAsync(UserAnswerRequest userAnswer)
    {
        return await _questionHttpClient.ValidateAnswerAsync(userAnswer);
    }

    /// <summary>
    /// Gets user security question by phone number
    /// </summary>
    /// <param name="phoneNumber"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpGet("user")]
    public async Task<string> GetUserQuestionByPhoneNumberAsync([FromQuery] string phoneNumber)
    {
        return await _questionHttpClient.GetUserQuestionByPhoneNumberAsync(phoneNumber);
    }

    /// <summary>
    /// Validates users answer with phone number
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost("validate/answer")]
    public async Task<bool> ValidateUserAnswerAsync(ValidateUserQuestionAnswerRequest request)
    {
        return await _questionHttpClient.ValidateUserAnswerAsync(request);
    }
}
