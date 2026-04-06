namespace LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Requests
{
    public class DeactivateCorporateWalletUserRequest
    {
        public Guid AccountUserId { get; set; }
    }
    public class DeactivateCorporateWalletUserServiceRequest : DeactivateCorporateWalletUserRequest
    {
        public Guid AccountId { get; set; }
    }
}
