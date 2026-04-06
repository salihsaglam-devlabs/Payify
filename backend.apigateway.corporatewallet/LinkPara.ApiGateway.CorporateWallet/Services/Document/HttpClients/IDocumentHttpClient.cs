using LinkPara.ApiGateway.CorporateWallet.Services.Document.Models;

namespace LinkPara.ApiGateway.CorporateWallet.Services.Document.HttpClients
{
    public interface IDocumentHttpClient
    {
        Task<DocumentDto> GetDocumentAsync(Guid Id);
    }
}