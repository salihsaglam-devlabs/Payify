namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Requests
{
    public class ActivateCorporateWalletUserRequest
    {
        public Guid AccountUserId { get; set; }
        public Guid AccountId { get; set; }
    }
}
