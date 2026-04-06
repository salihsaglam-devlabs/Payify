using LinkPara.SoftOtp.Application.Common.Models.PowerFactorModels.Response;
using LinkPara.SoftOtp.Application.Features.Auth.Commands.CheckTransactionApproval;
using LinkPara.SoftOtp.Application.Features.Auth.Commands.DeviceActivation;
using LinkPara.SoftOtp.Application.Features.Auth.Commands.StartClientTransaction;
using LinkPara.SoftOtp.Application.Features.Auth.Commands.StartOneTouchTransaction;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.SoftOtp.API.Controllers;

public class TransactionController : ApiControllerBase
{
    [AllowAnonymous]
    [HttpPost("start-client-transaction")]
    public async Task<StartClientTransactionResponse> StartClientTransaction(StartClientTransactionCommand command)
    {
        return await Mediator.Send(command);
    }

    [AllowAnonymous]
    [HttpPost("check-transaction-approval")]
    public async Task<CheckTransactionApprovalResponse> CheckTransactionApproval(
        CheckTransactionApprovalCommand command)
    {
        return await Mediator.Send(command);
    }
        
    [AllowAnonymous]
    [HttpPost("start-one-touch-transaction")]
    public async Task<OneTouchRequestResponse> StartOneTouchTransaction(
        StartOneTouchTransactionCommand command)
    {
        return await Mediator.Send(command);
    }
}