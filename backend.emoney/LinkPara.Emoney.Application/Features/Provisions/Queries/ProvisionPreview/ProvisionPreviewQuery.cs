using LinkPara.Emoney.Application.Commons.Interfaces;
using MediatR;
using LinkPara.Emoney.Application.Commons.Models.ProvisionModels;

namespace LinkPara.Emoney.Application.Features.Provisions.Queries.ProvisionPreview
{
    public class ProvisionPreviewQuery : IRequest<ProvisionPreviewResponse>
    {
        public Guid UserId { get; set; }
        public string WalletNumber { get; set; }
        public decimal Amount { get; set; }
        public string CurrencyCode { get; set; }
        public Guid PartnerId { get; set; }
    }

    public class ProvisionPreviewHandler : IRequestHandler<ProvisionPreviewQuery, ProvisionPreviewResponse>
    {
        private readonly IProvisionService _provisionServicerService;

        public ProvisionPreviewHandler(IProvisionService provisionServicerService)
        {
            _provisionServicerService = provisionServicerService;
        }
        public async Task<ProvisionPreviewResponse> Handle(ProvisionPreviewQuery request, CancellationToken cancellationToken)
        {
            return await _provisionServicerService.ProvisionPreviewAsync(request);
        }
    }
}
