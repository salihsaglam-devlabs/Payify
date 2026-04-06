namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Requests
{
    public class DeactivateCorporateWalletUserRequest
    {
        public Guid AccountUserId { get; set; }
        public Guid AccountId { get; set; }
    }
}
