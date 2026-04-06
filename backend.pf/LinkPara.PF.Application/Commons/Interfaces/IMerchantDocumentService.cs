using LinkPara.PF.Application.Commons.Models.Merchants;
using LinkPara.PF.Application.Features.MerchantDocuments.Commands.SaveMerchantDocument;
using LinkPara.PF.Application.Features.MerchantDocuments.Queries.GetMerchantDocumentsByTransactionIdQuery;


namespace LinkPara.PF.Application.Commons.Interfaces
{
    public interface IMerchantDocumentService
    {
        Task SaveMerchantDocumentsByTransactionId(SaveMerchantDocumentsByTransactionIdCommand command); 
        Task<List<MerchantDocumentDto>> GetMerchantDocumentsByTransactionId(GetMerchantDocumentsByTransactionIdQuery query);
    }
}
