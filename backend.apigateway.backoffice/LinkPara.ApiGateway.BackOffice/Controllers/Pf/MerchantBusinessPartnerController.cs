using LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.Pf
{
    public class MerchantBusinessPartnerController : ApiControllerBase
    {
        private readonly IMerchantBusinessPartnerHttpClient _merchantBusinessPartnerHttpClient;

        public MerchantBusinessPartnerController(IMerchantBusinessPartnerHttpClient merchantBusinessPartnerHttpClient)
        {
            _merchantBusinessPartnerHttpClient = merchantBusinessPartnerHttpClient;
        }

        /// <summary>
        /// Returns all merchant business partner
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize(Policy = "MerchantPartner:ReadAll")]
        [HttpGet("")]
        public async Task<ActionResult<PaginatedList<MerchantBusinessPartnerDto>>> GetAllAsync([FromQuery] GetAllMerchantBusinessPartnerRequest request)
        {
            return await _merchantBusinessPartnerHttpClient.GetAllAsync(request);
        }

        /// <summary>
        /// Returns a business partner
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Policy = "MerchantPartner:Read")]
        [HttpGet("{id}")]
        public async Task<ActionResult<MerchantBusinessPartnerDto>> GetByIdAsync([FromRoute] Guid id)
        {
            return await _merchantBusinessPartnerHttpClient.GetByIdAsync(id);
        }

        /// <summary>
        /// Create a merchant business partner
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="AlreadyInUseException"></exception>
        [Authorize(Policy = "MerchantPartner:Create")]
        [HttpPost("")]
        public async Task SaveAsync(SaveMerchantBusinessPartnerRequest request)
        {
            await _merchantBusinessPartnerHttpClient.SaveAsync(request);
        }

        /// <summary>
        /// Update a merchant business partner
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize(Policy = "MerchantPartner:Update")]
        [HttpPut("")]
        public async Task UpdateAsync(MerchantBusinessPartnerDto request)
        {
            await _merchantBusinessPartnerHttpClient.UpdateAsync(request);
        }
    }
}
