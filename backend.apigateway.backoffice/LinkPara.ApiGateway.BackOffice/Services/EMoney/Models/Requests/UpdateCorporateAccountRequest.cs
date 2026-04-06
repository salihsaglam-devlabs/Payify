namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Requests
{
    public class UpdateCorporateAccountRequest
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string PhoneCode { get; set; }
        public string PhoneNumber { get; set; }
        public string LandPhone { get; set; }
        public string WebSiteUrl { get; set; }
        public string Name { get; set; }
        public string PostalCode { get; set; }
        public string Address { get; set; }
        public int Country { get; set; }
        public string CountryName { get; set; }
        public int City { get; set; }
        public string CityName { get; set; }
        public int District { get; set; }
        public string DistrictName { get; set; }
    }
}
