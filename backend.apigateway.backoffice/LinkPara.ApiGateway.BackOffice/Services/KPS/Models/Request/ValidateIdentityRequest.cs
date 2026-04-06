namespace LinkPara.ApiGateway.BackOffice.Services.KPS.Models.Request
{
    public class ValidateIdentityRequest
    {
        public string IdentityNo { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public DateTime BirthDate { get; set; }
    }
}
