using LinkPara.PF.Application.Features.Links.Queries.GetCreateLinkRequirement;
using LinkPara.PF.Application.Features.Links;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinkPara.PF.Application.Commons.Interfaces;

namespace LinkPara.PF.Application.Features.Installments.Queries.GetManualPaymentPageInstallments
{
    public class GetManualPaymentPageInstallmentsQuery : IRequest<ManualPaymentPageInstallmentsResponse>
    {
        public Guid MerchantId { get; set; }
    }

    public class GetManualPaymentPageInstallmentsQueryHandler : IRequestHandler<GetManualPaymentPageInstallmentsQuery, ManualPaymentPageInstallmentsResponse>
    {
        private readonly IInstallmentService _installmentService;
        public GetManualPaymentPageInstallmentsQueryHandler(IInstallmentService installmentService)
        {
            _installmentService = installmentService;
        }

        public async Task<ManualPaymentPageInstallmentsResponse> Handle(GetManualPaymentPageInstallmentsQuery request, CancellationToken cancellationToken)
        {
            return await _installmentService.GetManualPaymentPageInstallmentsAsync(request);
        }
    }
}
