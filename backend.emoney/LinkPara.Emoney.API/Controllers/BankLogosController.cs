﻿using LinkPara.Emoney.Application.Features.Banks.Commands.SaveBankLogo;
using LinkPara.Emoney.Application.Features.Banks.Queries;
using LinkPara.Emoney.Application.Features.Banks.Queries.GetBankLogo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.Emoney.API.Controllers
{
    public class BankLogosController : ApiControllerBase
    {

        /// <summary>
        /// Get bank logo.
        /// </summary>
        /// <param name="query"></param>
        [Authorize(Policy = "Bank:ReadAll")]
        [HttpGet("")]
        public async Task<BankLogoDto> GetBankLogoAsync([FromQuery] GetBankLogoQuery query)
        {
            return await Mediator.Send(query);
        }

        /// <summary>
        /// Upload bank logo.
        /// </summary>
        /// <param name="bankLogo"></param>
        [Authorize(Policy = "Bank:ReadAll")]//TODO: Permisson create edilcek
        [HttpPost("")]
        public async Task UploadBankLogoAsync(BankLogoDto bankLogo)
        {
            var command = new SaveBankLogoCommand() { BankLogo = bankLogo };
            await Mediator.Send(command);
        }
    }
}