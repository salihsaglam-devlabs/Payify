using LinkPara.IKS.Application.Commons.Models.IKSModels;
using LinkPara.IKS.Application.Commons.Models.IKSModels.Terminal.Response;
using LinkPara.IKS.Application.Features.Terminal;
using LinkPara.IKS.Application.Features.Terminal.Command.CreateTerminal;
using LinkPara.IKS.Application.Features.Terminal.Command.SaveTerminal;
using LinkPara.IKS.Application.Features.Terminal.Command.UpdateTerminal;
using LinkPara.IKS.Application.Features.Terminal.Queries.GetAllTerminal;
using LinkPara.IKS.Application.Features.Terminal.Queries.GetAllTerminalHistory;
using LinkPara.IKS.Application.Features.Terminal.Queries.GetTerminalStatusByReferenceCode;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.IKS.API.Controllers
{
    public class TerminalsController : ApiControllerBase
    {
        /// <summary>
        /// Returns all terminals.
        /// </summary>
        /// <returns></returns>
        [Authorize(Policy = "MerchantIksTerminal:ReadAll")]
        [HttpGet("get-terminals")]
        public async Task<PaginatedList<IksTerminalDto>> GetAllAsync([FromQuery] GetAllTerminalQuery request)
        {
            return await Mediator.Send(request);
        }

        /// <summary>
        /// Returns all terminal histories.
        /// </summary>
        /// <returns></returns>
        [Authorize(Policy = "MerchantIksTerminal:ReadAll")]
        [HttpGet("get-terminal-histories")]
        public async Task<PaginatedList<IksTerminalHistoryDto>> GetAllAsync([FromQuery] GetAllTerminalHistoryQuery request)
        {
            return await Mediator.Send(request);
        }

        [Authorize(Policy = "MerchantIksTerminal:Create")]
        [HttpPost("")]
        public async Task<IKSResponse<TerminalResponse>> SaveTerminalAsync(SaveTerminalCommand command)
        {
            return await Mediator.Send(command);
        }
        
        [Authorize(Policy = "MerchantIksTerminal:Create")]
        [HttpPost("create-terminal")]
        public async Task<IKSResponse<TerminalResponse>> CreateTerminalAsync(CreateTerminalCommand command)
        {
            return await Mediator.Send(command);
        }
        
        [Authorize(Policy = "MerchantIksTerminal:Create")]
        [HttpGet("{referenceCode}")]
        public async Task<IKSResponse<TerminalResponse>> GetTerminalStatusByReferenceCodeAsync([FromRoute] string referenceCode)
        {
            return await Mediator.Send(new GetTerminalStatusByReferenceCodeQuery{ReferenceCode = referenceCode});
        }

        /// <summary>
        /// Updates a terminal
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [Authorize(Policy = "MerchantIksTerminal:Update")]
        [HttpPut("")]
        public async Task<IKSResponse<TerminalResponse>> UpdateAsync(UpdateTerminalCommand command)
        {
            return await Mediator.Send(command);
        }
    }
}
