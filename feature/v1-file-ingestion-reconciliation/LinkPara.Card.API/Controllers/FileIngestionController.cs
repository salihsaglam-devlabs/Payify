using LinkPara.Card.Application.Commons.Constants;
using LinkPara.Card.Application.Commons.Models.FileIngestion;
using LinkPara.Card.Application.Features.FileIngestion.Commands.ImportCardTransactionsFromFtp;
using LinkPara.Card.Application.Features.FileIngestion.Commands.ImportCardTransactionsFromLocal;
using LinkPara.Card.Application.Features.FileIngestion.Commands.ImportClearingFromFtp;
using LinkPara.Card.Application.Features.FileIngestion.Commands.ImportClearingFromLocal;
using LinkPara.Card.API.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.Card.API.Controllers;

[ApiController]
[Route("v1/[controller]")]
public class FileIngestionController : ApiControllerBase
{
    [Authorize(Policy = AuthorizationPolicies.CardFileIngestionCreate)]
    [HttpPost(EndpointRoutes.FileIngestionService.ImportCardTransactionsFromLocal)]
    public Task<ActionResult<FileIngestionOperationResponse>> ImportCardTransactionsFromLocalAsync(
        [FromQuery] ImportCardTransactionsFromLocalCommand command,
        CancellationToken cancellationToken)
    {
        return ExecuteWithProcessLockAsync(
            lockName: ProcessLockNames.CardFileIngestion,
            jobType: nameof(ImportCardTransactionsFromLocalCommand),
            action: token => Mediator.Send(command, token),
            cancellationToken: cancellationToken);
    }

    [Authorize(Policy = AuthorizationPolicies.CardFileIngestionCreate)]
    [HttpPost(EndpointRoutes.FileIngestionService.ImportCardTransactionsFromFtp)]
    public Task<ActionResult<FileIngestionOperationResponse>> ImportCardTransactionsFromFtpAsync(
        [FromQuery] ImportCardTransactionsFromFtpCommand command,
        CancellationToken cancellationToken)
    {
        return ExecuteWithProcessLockAsync(
            lockName: ProcessLockNames.CardFileIngestion,
            jobType: nameof(ImportCardTransactionsFromFtpCommand),
            action: token => Mediator.Send(command, token),
            cancellationToken: cancellationToken);
    }

    [Authorize(Policy = AuthorizationPolicies.CardFileIngestionCreate)]
    [HttpPost(EndpointRoutes.FileIngestionService.ImportClearingFromLocal)]
    public Task<ActionResult<FileIngestionOperationResponse>> ImportClearingFromLocalAsync(
        [FromQuery] ImportClearingFromLocalCommand command,
        CancellationToken cancellationToken)
    {
        return ExecuteWithProcessLockAsync(
            lockName: ProcessLockNames.CardFileIngestion,
            jobType: nameof(ImportClearingFromLocalCommand),
            action: token => Mediator.Send(command, token),
            cancellationToken: cancellationToken);
    }

    [Authorize(Policy = AuthorizationPolicies.CardFileIngestionCreate)]
    [HttpPost(EndpointRoutes.FileIngestionService.ImportClearingFromFtp)]
    public Task<ActionResult<FileIngestionOperationResponse>> ImportClearingFromFtpAsync(
        [FromQuery] ImportClearingFromFtpCommand command,
        CancellationToken cancellationToken)
    {
        return ExecuteWithProcessLockAsync(
            lockName: ProcessLockNames.CardFileIngestion,
            jobType: nameof(ImportClearingFromFtpCommand),
            action: token => Mediator.Send(command, token),
            cancellationToken: cancellationToken);
    }
}
