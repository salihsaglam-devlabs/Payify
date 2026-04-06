using LinkPara.ApiGateway.Services.Emoney.HttpClients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.Controllers.Emoney
{
    public class BankLogosController : ApiControllerBase
    {
        private readonly IBankLogoHttpClient _bankLogoHttpClient;

        public BankLogosController(IBankLogoHttpClient bankLogoHttpClient)
        {
            _bankLogoHttpClient = bankLogoHttpClient;
        }

        /// <summary>
        /// Returns bank logo by ID.
        /// </summary>
        /// <param name="bankId"></param>
        ///     
        [Authorize(Policy = "Bank:ReadAll")]
        [HttpGet("")]
        public async Task<IActionResult> GetBankLogoAsync(Guid bankId)
        {
            var bankLogo = await _bankLogoHttpClient.GetBankLogoAsync(bankId);
            if (bankLogo.Bytes is null)
            {
                return null;
            }
            return File(bankLogo.Bytes, bankLogo.ContentType);
        }
    }
}