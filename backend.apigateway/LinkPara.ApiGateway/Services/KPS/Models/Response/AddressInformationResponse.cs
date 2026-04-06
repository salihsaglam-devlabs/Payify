namespace LinkPara.ApiGateway.Services.KPS.Models.Response
{
    public class AddressInformationResponse
    {
        public string Explanation { get; set; }
        public long? AddressNo { get; set; }
        public AddressDetail TownAddress { get; set; }
        public AddressDetail CityAddress { get; set; }
        public AddressDetail VillageAddress { get; set; }
        public AddressParameter Error { get; set; }
        public AddressParameter Type { get; set; }
    }
    public class AddressParameter
    {
        public string Explanation { get; set; }
        public int? Code { get; set; }
    }

    public class AddressDetail
    {
        public AddressParameter IndependentSectionStatus { get; set; }
        public AddressParameter IndependentSectionType { get; set; }
        public string BuildingIsle { get; set; }
        public string BuildingBlockName { get; set; }
        public AddressParameter BuildingStatus { get; set; }
        public long? BuildingCode { get; set; }
        public int? BuildingNumber { get; set; }
        public AddressParameter BuildingNumberType { get; set; }
        public string BuildingPafta { get; set; }
        public string BuildingParcel { get; set; }
        public string BuildingSiteName { get; set; }
        public AddressParameter BuildingStructureType { get; set; }
        public string Cbsm { get; set; }
        public int? CbsmCode { get; set; }
        public string OutDoorNumber { get; set; }
        public AddressParameter Error { get; set; }
        public string InDoorNumber { get; set; }
        public string City { get; set; }
        public int? CityCode { get; set; }
        public string District { get; set; }
        public int? DistrictCode { get; set; }
        public string FloorNumber { get; set; }
        public string Village { get; set; }
        public long? VillageRegistryNumber { get; set; }
        public long? VillageCode { get; set; }
        public string Neighbourhood { get; set; }
        public int? NeighbourhoodCode { get; set; }
        public string IndependentDeedSecitonNumber { get; set; }
        public AddressParameter BuildingUsagePurpose { get; set; }
        public int? NumberOfFloorsBelowRoad { get; set; }
        public int? NumberOfFloorsAboveRoad { get; set; }
    }

    public class DefaultAddressModel
    {
        public AddressInformationResponse KpsResponse { get; set; }
    }
}
