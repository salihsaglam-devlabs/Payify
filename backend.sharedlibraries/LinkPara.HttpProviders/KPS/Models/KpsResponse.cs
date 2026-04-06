namespace LinkPara.HttpProviders.KPS.Models
{
    public class KpsResponse
    {
        public IDRegistration IDRegistration { get; set; }
        public AddressRegistration AddressRegistration { get; set; }
    }
    public class IDRegistration
    {
        public string FatherName { get; set; }
        public string MotherName { get; set; }
        public string BirthPlace { get; set; }
        public string RegistrationPlace { get; set; }
        public string RegistrationPlaceFamilyRow { get; set; }
        public string RegistrationPlacePersonalRow { get; set; }
        public string SerialNo { get; set; }
        public string RecordNo { get; set; }
        public string IdentityType { get; set; }
        public string IdentityNo { get; set; }
        public string DocumentNo { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Gender { get; set; }
        public string BirthDate { get; set; }
        public string Nationality { get; set; }
        public string IssuedBy { get; set; }
        public string IssuedDate { get; set; }
        public string ExpireDate { get; set; }
    }

    public class AddressRegistration
    {
        public string AddressType { get; set; }
        public string District { get; set; }
        public int DistrictCode { get; set; }
        public string Street { get; set; }
        public int StreetCode { get; set; }
        public int VillageCode { get; set; }
        public string AddressDetail { get; set; }
        public int TownCode { get; set; }
        public string Town { get; set; }
        public string City { get; set; }
        public int CityCode { get; set; }
        public string Country { get; set; }
        public int CountryCode { get; set; }
    }
}
