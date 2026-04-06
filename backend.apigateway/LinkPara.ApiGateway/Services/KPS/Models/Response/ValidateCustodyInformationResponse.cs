namespace LinkPara.ApiGateway.Services.KPS.Models.Response
{
    public class ValidateCustodyInformationResponse
    {
        public long ChildIdentityNumber { get; set; }
        public string ChildName { get; set; }
        public string ChildSurname { get; set; }
        public DateTime ChildBirthDate { get; set; }
    }
}
