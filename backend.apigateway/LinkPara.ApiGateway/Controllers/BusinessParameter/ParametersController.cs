using LinkPara.ApiGateway.Services.BusinessParameter.HttpClients;
using LinkPara.ApiGateway.Services.BusinessParameter.Models.Request;
using LinkPara.ApiGateway.Services.BusinessParameter.Models.Response;
using LinkPara.SharedModels.Pagination;
using MassTransit.Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.Controllers.BusinessParameter
{
    public class ParametersController : ApiControllerBase
    {
        private readonly IParameterHttpClient _parameterHttpClient;

        public ParametersController(IParameterHttpClient parameterHttpClient)
        {
            _parameterHttpClient = parameterHttpClient;
        }

        /// <summary>
        /// Returns all parameter list
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize(Policy = "Parameter:ReadAll")]
        [HttpGet("")]
        public async Task<ActionResult<PaginatedList<ParameterDto>>> GetAllAsync([FromQuery] GetAllParameterRequest request)
        {
            return await _parameterHttpClient.GetAllParameterAsync(request);
        }

        [Authorize(Policy = "Parameter:Read")]
        [HttpGet("{groupCode}")]
        public async Task<ActionResult<List<ParameterDto>>> GetAllAsync([FromRoute] string groupCode)
        {
            return await _parameterHttpClient.GetParametersAsync(groupCode);
        }

        /// <summary>
        /// Returns profession parameters
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("profession")]
        public async Task<List<ParameterDto>> GetProfessionParametersAsync()
        {
            return await _parameterHttpClient.GetProfessionParametersAsync();
        }

        /// <summary>
        /// Returns company information parameters
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("company-info")]
        public async Task<List<ParameterDto>> GetCompanyInfoParametersAsync()
        {
            return await _parameterHttpClient.GetCompanyInfoParametersAsync();
        }
    }
}
