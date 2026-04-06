using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Requests
{
    public class ApproveChargebackRequest
    {
        public Guid TransactionId { get; set; }
        public ChargebackStatus Status { get; set; }
        public string Description { get; set; }
    }
}
