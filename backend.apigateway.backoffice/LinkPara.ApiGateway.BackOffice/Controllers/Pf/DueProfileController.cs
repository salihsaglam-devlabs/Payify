using LinkPara.ApiGateway.BackOffice.Controllers;
using LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.PF.API.Controllers
{
    public class DueProfileController : ApiControllerBase
    {
        private readonly IDueProfileHttpClient _dueProfileHttpClient;

        public DueProfileController(IDueProfileHttpClient dueProfileHttpClient)
        {
            _dueProfileHttpClient = dueProfileHttpClient;
        }
        /// <summary>
        /// Returns filtered due profiles
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize(Policy = "DueProfile:ReadAll")]
        [HttpGet("")]
        public async Task<ActionResult<PaginatedList<DueProfileDto>>> GetFilterAsync([FromQuery] GetFilterDueProfileRequest request)
        {
            return await _dueProfileHttpClient.GetFilterListAsync(request);
        }

        /// <summary>
        /// Returns a due profile
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Policy = "DueProfile:Read")]
        [HttpGet("{id}")]
        public async Task<ActionResult<DueProfileDto>> GetByIdAsync([FromRoute] Guid id)
        {
            return await _dueProfileHttpClient.GetByIdAsync(id);
        }

        /// <summary>
        /// Updates a due profile
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize(Policy = "DueProfile:Update")]
        [HttpPut("")]
        public async Task UpdateAsync(UpdateDueProfileRequest request)
        {
            await _dueProfileHttpClient.UpdateAsync(request);
        }

        /// <summary>
        /// Create a new due profile
        /// </summary>
        /// <param name="request"></param>
        [Authorize(Policy = "DueProfile:Create")]
        [HttpPost]
        public async Task CreateDueProfileAsync(CreateDueProfileRequest request)
        {
            await _dueProfileHttpClient.CreateAsync(request);
        }

        /// <summary>
        /// Delete due profile
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Policy = "DueProfile:Delete")]
        [HttpDelete("{id}")]
        public async Task DeleteAsync(Guid id)
        {
            await _dueProfileHttpClient.DeleteAsync(id);
        }
    }
}
