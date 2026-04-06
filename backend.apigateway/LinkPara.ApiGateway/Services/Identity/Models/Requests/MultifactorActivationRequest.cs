namespace LinkPara.ApiGateway.Services.Identity.Models.Requests
{
    public class MultifactorActivationRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string PhoneNumber { get; set; }
    }
}
