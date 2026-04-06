namespace LinkPara.ApiGateway.Services.KPS.Models.Request
{
    public class ValidateCustodyInformationRequest
    {
        public long ParentIdentityNumber { get; set; }
        public long ChildIdentityNumber { get; set; }
        public DateTime ChildBirthday { get; set; }
    }
}
