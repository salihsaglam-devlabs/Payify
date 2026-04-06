using LinkPara.ApiGateway.BackOffice.Services.Identity.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.Identity
{
    public class QuestionsController : ApiControllerBase
    {
        private readonly IQuestionHttpClient _questionHttpClient;

        public QuestionsController(IQuestionHttpClient questionHttpClient)
        {
            _questionHttpClient = questionHttpClient;
        }

        [HttpGet("")]
        [Authorize(Policy = "Question:ReadAll")]
        public async Task<ActionResult<PaginatedList<SecurityQuestionDto>>> GetAllAsync([FromQuery] GetAllSecurityQuestionRequest request)
        {
            return await _questionHttpClient.GetAllSecurityQuestionAsync(request);
        }

        [HttpPost("")]
        [Authorize(Policy = "Question:Create")]
        public async Task SaveAsync(SecurityQuestionRequest request)
        {
            await _questionHttpClient.SaveAsync(request);
        }

        [HttpPut("")]
        [Authorize(Policy = "Question:Update")]
        public async Task UpdateAsync(UpdateSecurityQuestionRequest request)
        {
            await _questionHttpClient.UpdateAsync(request);
        }

        [HttpDelete("")]
        [Authorize(Policy = "Question:Delete")]
        public async Task DeleteAsync(Guid id)
        {
            await _questionHttpClient.DeleteSecurityQuestionAsync(id);
        }

        [HttpGet("{id:guid}")]
        [Authorize(Policy = "Question:Read")]
        public async Task<ActionResult<SecurityQuestionDto>> GetByIdAsync([FromRoute] Guid id)
        {
            return await _questionHttpClient.GetSecurityQuestionByIdAsync(id);
        }
    }
}
