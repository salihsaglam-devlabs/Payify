using LinkPara.ApiGateway.Commons.Helpers;

namespace LinkPara.ApiGateway.Services.CampaignManagement.Models.Requests;

public class IWalletCreateCardRequest
{
    public bool IsApprovedIndividualFrameworkAgreement { get; set; }
    public string IndividualFrameworkAgreementVersion { get; set; }
    public bool IsApprovedPreliminaryInformationAgreement { get; set; }
    public string PreliminaryInformationAgreementVersion { get; set; }
    public bool IsApprovedKvkkAgreement { get; set; }
    public string KvkkAgreementVersion { get; set; }
    public bool IsApprovedCommercialElectronicCommunicationAggrement { get; set; }
    public string CommercialElectronicCommunicationAggrementVersion { get; set; }

}

public class IWalletCreateCardServiceRequest : IWalletCreateCardRequest, IHasUserId
{
    public Guid UserId { get; set; }
    public string WalletNumber { get; set; }
    public string FullName { get; set; }
    public string IdentityNumber { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public int CityId { get; set; }
    public int TownId { get; set; }
    public string AddressDetail { get; set; }

}
