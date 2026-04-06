namespace LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Responses
{
    public class ActiveUserDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string PhoneCode { get; set; }
        public string FullName { get; set; }
        public string IdentityNumber { get; set; }
        public string UserName { get; set; }
    }
}
