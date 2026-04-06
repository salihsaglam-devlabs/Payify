using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.Merchants;
using MediatR;

namespace LinkPara.PF.Application.Features.MerchantDocuments.Commands.SaveMerchantDocument
{
    public class SaveMerchantDocumentsByTransactionIdCommand : IRequest
    {
        public Guid TransactionId { get; set; }
        public Guid MerchantId { get; set; }
        public List<MerchantDocumentDto> MerchantDocuments { get; set; }
    }
    public class SaveMerchantDocumentsByTransactionIdCommandHandler : IRequestHandler<SaveMerchantDocumentsByTransactionIdCommand>
    {
        private readonly IMerchantDocumentService _merchantDocumentService;

        public SaveMerchantDocumentsByTransactionIdCommandHandler(IMerchantDocumentService merchantDocumentService)
        {
            _merchantDocumentService = merchantDocumentService;
        }
        public async Task<Unit> Handle(SaveMerchantDocumentsByTransactionIdCommand request, CancellationToken cancellationToken)
        {
            await _merchantDocumentService.SaveMerchantDocumentsByTransactionId(request);

            return Unit.Value;
        }
    }
}
