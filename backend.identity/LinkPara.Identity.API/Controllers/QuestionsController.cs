using LinkPara.Identity.Application.Features.SecurityQuestions.Commands.CreateSecurityQuestion;
using LinkPara.Identity.Application.Features.SecurityQuestions.Commands.DeleteSecurityQuestion;
using LinkPara.Identity.Application.Features.SecurityQuestions.Commands.UpdateSecurityQuestion;
using LinkPara.Identity.Application.Features.SecurityQuestions.Queries.GetAllSecurityQuestion;
using LinkPara.Identity.Application.Features.SecurityQuestions.Queries.GetSecurityQuestionById;
using LinkPara.Identity.Application.Features.UserQuestions.Commands.CreateUserAnswer;
using LinkPara.Identity.Application.Features.UserQuestions.Commands.UpdateUserAnswer;
using LinkPara.Identity.Application.Features.UserQuestions.Commands.ValidateUserAnswer;
using LinkPara.Identity.Application.Features.UserQuestions.Commands.ValidateUserQuestionAnswer;
using LinkPara.Identity.Application.Features.UserQuestions.Queries;
using LinkPara.Identity.Application.Features.UserQuestions.Queries.GetAllQuestions;
using LinkPara.Identity.Application.Features.UserQuestions.Queries.GetUserQuestion;
using LinkPara.Identity.Application.Features.UserQuestions.Queries.GetUserQuestionByPhoneNumber;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.Identity.API.Controllers;

public class QuestionsController : ApiControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<List<SecurityQuestionDto>>> GetAllAsync()
    {
        return await Mediator.Send(new GetAllQuestionsQuery());
    }

    [Authorize(Policy = "Question:Read")]
    [HttpGet("{userId}/question")]
    public async Task<ActionResult<SecurityQuestionDto>> GetUserQuestionAsync([FromRoute] Guid userId)
    {
        return await Mediator.Send(new GetUserQuestionQuery { UserId = userId });
    }

    [Authorize(Policy = "Question:Update")]
    [HttpPut("{userId}/answer")]
    public async Task UpdateAnswerAsync(UpdateUserAnswerCommand command)
    {
        await Mediator.Send(command);
    }

    [Authorize(Policy = "Question:Create")]
    [HttpPost("{questionId}/answer")]
    public async Task<ActionResult> SaveAnswerAsync(CreateUserAnswerCommand command)
    {
        await Mediator.Send(command);

        return NoContent();
    }

    [Authorize(Policy = "Question:Create")]
    [HttpPost("{questionId}/answer/validate")]
    public async Task<ActionResult<bool>> ValidateAnswerAsync(ValidateUserAnswerCommand command)
    {
        return await Mediator.Send(command);
    }

    [Authorize(Policy = "Question:Create")]
    [HttpPost("")]
    public async Task SaveAsync(CreateSecurityQuestionCommand command)
    {
        await Mediator.Send(command);
    }
    [Authorize(Policy = "Question:Update")]
    [HttpPut("")]
    public async Task UpdateAsync(UpdateSecurityQuestionCommand command)
    {
        await Mediator.Send(command);
    }

    [Authorize(Policy = "Question:Read")]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SecurityQuestionDto>> GetByIdAsync([FromRoute] Guid id)
    {
        return await Mediator.Send(new GetSecurityQuestionByIdQuery { Id = id });
    }

    [Authorize(Policy = "Question:Delete")]
    [HttpDelete("{id}")]
    public async Task DeleteAsync(Guid id)
    {
        await Mediator.Send(new DeleteSecurityQuestionCommand { Id = id });
    }

    /// <summary>
    /// get all question
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    [Authorize(Policy = "Question:ReadAll")]
    [HttpGet("getAllQuestion")]
    public async Task<ActionResult<PaginatedList<SecurityQuestionDto>>> GetAllQuestionAsync([FromQuery] GetAllSecurityQuestionQuery query)
    {
        return await Mediator.Send(query);
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
        return await Mediator.Send(new GetUserQuestionByPhoneNumberQuery { PhoneNumber = phoneNumber });
    }

    /// <summary>
    /// Validates users answer with phone number
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost("validate/answer")]
    public async Task<bool> ValidateUserAnswerAsync(ValidateUserQuestionAnswerCommand command)
    {
        return await Mediator.Send(command);
    }
}