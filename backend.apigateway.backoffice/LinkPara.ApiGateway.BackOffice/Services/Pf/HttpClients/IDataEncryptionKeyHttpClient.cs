using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients
{
    public interface IDataEncryptionKeyHttpClient
    {
        Task DataEncryptionKeyAsync(DataEncryptionKeyRequest request);
    }
}
