using LinkPara.PF.Application.Features.DataEncryptionKey.Commands.DataEncryptionKey;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.PF.API.Controllers
{
    public class DataEncryptionKeyController : ApiControllerBase
    {
        /// <summary>
        /// Set SecretKey Cache
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [Authorize(Policy = "Dek:Create")]
        [HttpPost]
        public async Task DataEncryptionKeyAsync(DataEncryptionKeyCommand command)
        {
            await Mediator.Send(command);
        }

    }
}
