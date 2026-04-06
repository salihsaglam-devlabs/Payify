namespace LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Requests
{
    public class ActivateCorporateWalletUserRequest
    {
        public Guid AccountUserId { get; set; }
    }
    public class ActivateCorporateWalletUserServiceRequest : ActivateCorporateWalletUserRequest
    {
        public Guid AccountId { get; set; }
    }
}
