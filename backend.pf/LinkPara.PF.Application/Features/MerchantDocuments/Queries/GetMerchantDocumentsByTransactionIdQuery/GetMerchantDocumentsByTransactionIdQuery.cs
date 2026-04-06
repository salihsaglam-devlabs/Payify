using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.Merchants;
using MediatR;

namespace LinkPara.PF.Application.Features.MerchantDocuments.Queries.GetMerchantDocumentsByTransactionIdQuery
{
    public class GetMerchantDocumentsByTransactionIdQuery : IRequest<List<MerchantDocumentDto>>
    {
        public Guid Id { get; set; }
    }
    public class GetMerchantDocumentsByTransactionIdQueryHandler : IRequestHandler<GetMerchantDocumentsByTransactionIdQuery, List<MerchantDocumentDto>>
    {
        private readonly IMerchantDocumentService _merchantDocumentService;

        public GetMerchantDocumentsByTransactionIdQueryHandler(IMerchantDocumentService merchantDocumentService)
        {
            _merchantDocumentService = merchantDocumentService;
        }
        public async Task<List<MerchantDocumentDto>> Handle(GetMerchantDocumentsByTransactionIdQuery request, CancellationToken cancellationToken)
        {
            return await _merchantDocumentService.GetMerchantDocumentsByTransactionId(request);
        }
    }
}
