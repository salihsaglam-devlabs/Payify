using LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.Pf
{
    public class DataEncryptionKeyController : ApiControllerBase
    {
        private readonly IDataEncryptionKeyHttpClient _dataEncryptionKeyHttpClient;

        public DataEncryptionKeyController(IDataEncryptionKeyHttpClient dataEncryptionKeyHttpClient)
        {
            _dataEncryptionKeyHttpClient = dataEncryptionKeyHttpClient;
        }

        [Authorize(Policy = "Dek:Create")]
        [HttpPost]
        public async Task CardSecretKeyAsync(DataEncryptionKeyRequest request)
        {
            await _dataEncryptionKeyHttpClient.DataEncryptionKeyAsync(request);
        }
    }
}
