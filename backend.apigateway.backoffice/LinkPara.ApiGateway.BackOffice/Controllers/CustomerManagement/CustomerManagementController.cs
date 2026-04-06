using LinkPara.ApiGateway.BackOffice.Services.CustomerManagement.HttpClients;
using LinkPara.ApiGateway.BackOffice.Services.CustomerManagement.Models.Request;
using LinkPara.ApiGateway.BackOffice.Services.CustomerManagement.Models.Response;
using LinkPara.SharedModels.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.BackOffice.Controllers.CustomerManagement
{
    public class CustomerManagementController : ApiControllerBase
    {
        private readonly ICustomerManagementClient _customerManagementHttpClient;

        public CustomerManagementController(ICustomerManagementClient customerManagementHttpClient)
        {
            _customerManagementHttpClient = customerManagementHttpClient;
        }

        [Authorize(Policy = "Customer:ReadAll")]
        [HttpGet("")]
        public async Task<ActionResult<PaginatedList<CustomerResponse>>> GetAllAsync([FromQuery] GetCustomersRequest request)
        {
            return await _customerManagementHttpClient.GetAllCustomersAsync(request);
        }

        [Authorize(Policy = "Customer:Update")]
        [HttpPut("UpdateCustomerAddress")]
        public async Task UpdateCustomerAddressAsync(UpdateCustomerAddressRequest request)
        {
            await _customerManagementHttpClient.UpdateCustomerAddressAsync(request);
        }
    }
}
