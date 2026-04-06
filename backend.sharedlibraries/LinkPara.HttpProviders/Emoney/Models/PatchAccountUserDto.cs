namespace LinkPara.HttpProviders.Emoney.Models
{
    public class PatchAccountUserDto
    {
        public Guid UserId { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Email { get; set; }
        public string PhoneCode { get; set; }
        public string PhoneNumber { get; set; }
    }
}
