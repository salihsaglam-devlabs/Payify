using LinkPara.ApiGateway.Services.Emoney.Models.Enums;
using LinkPara.Emoney.Application.Commons.Enums;
using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Application.Features.AccountServiceProviders;
using MediatR;

namespace LinkPara.Emoney.Application.Features.OpenBankingOperations.Commands.TriggerFuturePaymentOrder;
public class TriggerFuturePaymentOrderCommand : IRequest<TriggerFuturePaymentOrderResultDto>
{
    public string ConsentId { get; set; }
    public string AppUserId { get; set; }
    public string HhsCode { get; set; }
    public string RizaNo { get; set; }
    public string RizaTip { get; set; }
    public string RizaDrm { get; set; }
    public string YetTip { get; set; }
    public string YetKod { get; set; }
    public string DrmKod { get; set; }
   
    
}


public class TriggerFuturePaymentOrderCommandHandler : IRequestHandler<TriggerFuturePaymentOrderCommand, TriggerFuturePaymentOrderResultDto>
{
    private readonly IOpenBankingOperationsService _openBankingOperationsService;

    public TriggerFuturePaymentOrderCommandHandler(
         IOpenBankingOperationsService openBankingOperationsService)
    {
        _openBankingOperationsService = openBankingOperationsService;
    }

    public async Task<TriggerFuturePaymentOrderResultDto> Handle(TriggerFuturePaymentOrderCommand command,
        CancellationToken cancellationToken)
    {
        return await _openBankingOperationsService.TriggerFuturePaymentOrderAsync(command);
    }
}
