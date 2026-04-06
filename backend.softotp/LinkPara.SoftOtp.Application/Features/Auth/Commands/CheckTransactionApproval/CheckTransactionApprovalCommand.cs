using LinkPara.SoftOtp.Application.Common.Interfaces;
using LinkPara.SoftOtp.Application.Common.Models.PowerFactorModels.Request;
using LinkPara.SoftOtp.Application.Common.Models.PowerFactorModels.Response;
using MediatR;

namespace LinkPara.SoftOtp.Application.Features.Auth.Commands.CheckTransactionApproval;

public class CheckTransactionApprovalCommand : BaseRequest, IRequest<CheckTransactionApprovalResponse>
{
    public string TransactionToken { get; set; }
}

public class CheckTransactionApprovalCommandHandler : IRequestHandler<CheckTransactionApprovalCommand, CheckTransactionApprovalResponse>
{
    private readonly IMultifactorService _multifactorService;

    public CheckTransactionApprovalCommandHandler(IMultifactorService multifactorService)
    {
        _multifactorService = multifactorService;
    }
    public async Task<CheckTransactionApprovalResponse> Handle(CheckTransactionApprovalCommand request, CancellationToken cancellationToken)
    {
        return await _multifactorService.CheckTransactionApproval(request);
    }
}