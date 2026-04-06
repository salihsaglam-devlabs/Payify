using LinkPara.SoftOtp.Application.Common.Interfaces;
using LinkPara.SoftOtp.Application.Common.Models.PowerFactorModels.Enums;
using LinkPara.SoftOtp.Application.Common.Models.PowerFactorModels.Request;
using LinkPara.SoftOtp.Application.Common.Models.PowerFactorModels.Response;
using MediatR;

namespace LinkPara.SoftOtp.Application.Features.Auth.Commands.StartClientTransaction;

public class StartClientTransactionCommand : BaseRequest, IRequest<StartClientTransactionResponse>
{
    public string ApplicationName { get; set; }
    public List<int> TransactionApprovementTypeList { get; set; }
    public bool CancelPendingTransactions { get; set; }
    public string TransactionContent { get; set; }
    public string TransactionName { get; set; }
    public int TransactionType { get; set; }
    public int TimeoutDuration { get; set; }
    public TransactionOwner TransactionOwner { get; set; }
}

public class StartClientTransactionCommandHandler : IRequestHandler<StartClientTransactionCommand, StartClientTransactionResponse>
{
    private readonly IMultifactorService _multifactorService;

    public StartClientTransactionCommandHandler(IMultifactorService multifactorService)
    {
        _multifactorService = multifactorService;
    }
    
    public async Task<StartClientTransactionResponse> Handle(StartClientTransactionCommand request, CancellationToken cancellationToken)
    {
        var response = await _multifactorService.StartClientTransaction(request);

        return response;
    }
}
