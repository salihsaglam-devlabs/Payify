using LinkPara.ApiGateway.Services.CustomerManagement.HttpClients;
using LinkPara.ApiGateway.Services.CustomerManagement.Models.Request;
using LinkPara.ApiGateway.Services.Emoney.HttpClients;
using LinkPara.HttpProviders.CustomerManagement.Models;
using LinkPara.SharedModels.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.Controllers.CustomerManagement
{
    public class CustomersController : ApiControllerBase
    {
        private readonly ICustomerHttpClient _customerHttpClient;
        private readonly IEmoneyAccountHttpClient _accountHttpClient;

        public CustomersController(ICustomerHttpClient customerHttpClient, IEmoneyAccountHttpClient accountHttpClient)
        {
            _customerHttpClient = customerHttpClient;
            _accountHttpClient = accountHttpClient;
        }

        /// <summary>
        /// Returns the Customer which is logged in by user id.
        /// </summary>
        [Authorize(Policy = "Customer:Read")]
        [HttpGet("me")]
        public async Task<CustomerDto> GetAsync()
        {
            var account = await _accountHttpClient.GetAccountByUserIdAsync(Guid.Parse(UserId));

            if (account == null)
            {
                throw new NotFoundException(nameof(User));
            }

            return await _customerHttpClient.GetCustomerAsync(account.CustomerId);
        }

        [Authorize(Policy = "Customer:Update")]
        [HttpPut("UpdateCustomerAddress")]
        public async Task UpdateCustomerAddressAsync(UpdateCustomerAddressRequest request)
        {
            await _customerHttpClient.UpdateCustomerAddressAsync(request);
        }

        
        [Authorize(Policy = "Customer:Update")]
        [HttpPost("AddCustomerAddress")]
        public async Task AddCustomerAddressAsync(AddCustomerAddressRequest request)
        {
            await _customerHttpClient.AddCustomerAddressAsync(request);
        }
    }
}
