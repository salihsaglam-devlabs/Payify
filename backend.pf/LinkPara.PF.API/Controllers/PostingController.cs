using LinkPara.PF.Application.Features.Posting;
using LinkPara.PF.Application.Features.Posting.Queries;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.PF.API.Controllers;

public class PostingController : ApiControllerBase
{
    [Authorize(Policy = "PostingError:ReadAll")]
    [HttpGet("transfer-error")]
    public async Task<PaginatedList<PostingTransferErrorDto>> GetPostingTransferErrorAsync([FromQuery] PostingTransferErrorQuery request)
    {
        return await Mediator.Send(request);
    }

    [Authorize(Policy = "Posting:ReadAll")]
    [HttpGet("bills")]
    public async Task<PaginatedList<PostingBillDto>> GetPostingBillsAsync([FromQuery] PostingBillQuery request)
    {
        return await Mediator.Send(request);
    }
}