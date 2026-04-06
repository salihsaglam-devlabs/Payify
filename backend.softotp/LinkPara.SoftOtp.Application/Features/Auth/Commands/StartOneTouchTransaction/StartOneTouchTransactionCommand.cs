using LinkPara.SoftOtp.Application.Common.Interfaces;
using LinkPara.SoftOtp.Application.Common.Models.PowerFactorModels.Request;
using LinkPara.SoftOtp.Application.Common.Models.PowerFactorModels.Response;
using MediatR;

namespace LinkPara.SoftOtp.Application.Features.Auth.Commands.StartOneTouchTransaction;

public class StartOneTouchTransactionCommand : BaseRequest, IRequest<OneTouchRequestResponse>
{
    public string TransactionDefinitionKey { get; set; }
    public int[] TransactionApprovementTypeList { get; set; }
    public string TransactionContent { get; set; }
    public string TransactionName { get; set; }
    public int TransactionType { get; set; }
    public int TimeoutDuration { get; set; }
    public TransactionOwner TransactionOwner { get; set; }
    public string UserName { get; set; }
}

public class StartOneTouchTransactionCommandHandler : IRequestHandler<StartOneTouchTransactionCommand, OneTouchRequestResponse>
{
    private IMultifactorService _multifactorService;

    public StartOneTouchTransactionCommandHandler(IMultifactorService multifactorService)
    {
        _multifactorService = multifactorService;
    }

    public async Task<OneTouchRequestResponse> Handle(StartOneTouchTransactionCommand request, CancellationToken cancellationToken)
    {

        if (request.TransactionOwner is null)
        {
            request.TransactionOwner = new TransactionOwner
            {
                CustomerId = Convert.ToInt64(request.UserName)
            };
        }

        var response = await _multifactorService.StartOneTouchTransaction(request);

        return new OneTouchRequestResponse
        {
            IsSuccess = response.Success,
            TransactionToken = response.Success ? response.TransactionToken : string.Empty
        };
    }
}