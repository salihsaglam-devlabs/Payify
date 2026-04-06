namespace LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Requests
{
    public class LogoutRequest
    {
        public Guid UserId { get; set; }
        public string RefreshToken { get; set; }
    }
}
