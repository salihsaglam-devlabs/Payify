using LinkPara.PF.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.PF.Application.Features.Installments.Queries.CalculateInstallmentPricing;

public class CalculateInstallmentPricingQuery: IRequest<InstallmentPricingResponse>
{
    public string MerchantNumber { get; set; }
    public string BinNumber { get; set; }
    public decimal Amount { get; set; }
    public string ConversationId { get; set; }
}

public class CalculateInstallmentPricingQueryHandler  : IRequestHandler<CalculateInstallmentPricingQuery,InstallmentPricingResponse>
{
    private readonly IInstallmentService _installmentService;

    public CalculateInstallmentPricingQueryHandler(IInstallmentService installmentService)
    {
        _installmentService = installmentService;
    }
    
    public async Task<InstallmentPricingResponse> Handle(CalculateInstallmentPricingQuery request, CancellationToken cancellationToken)
    {
         var response = await _installmentService.GetInstallmentPricingAsync(request);

         return response;
    }

   
}