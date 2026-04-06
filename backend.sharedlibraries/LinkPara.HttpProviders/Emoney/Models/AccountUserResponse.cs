using LinkPara.SharedModels.Persistence;

namespace LinkPara.HttpProviders.Emoney.Models
{
    public class AccountUserResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Email { get; set; }
        public string PhoneCode { get; set; }
        public string PhoneNumber { get; set; }   
        public DateTime CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string CreatedBy { get; set; }
        public string LastModifiedBy { get; set; }
        public RecordStatus RecordStatus { get; set; }
        public Guid AccountId { get; set; }
    }
}
