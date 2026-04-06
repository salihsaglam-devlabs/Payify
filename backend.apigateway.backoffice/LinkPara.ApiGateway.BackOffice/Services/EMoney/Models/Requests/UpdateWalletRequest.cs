using LinkPara.SharedModels.Persistence;


namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Requests
{
    public class UpdateWalletRequest
    {
        public Guid UserId { get; set; }
        public Guid WalletId { get; set; }
        public string FriendlyName { get; set; }
        public RecordStatus RecordStatus { get; set; }
        public bool IsBlocked { get; set; }
    }
}
