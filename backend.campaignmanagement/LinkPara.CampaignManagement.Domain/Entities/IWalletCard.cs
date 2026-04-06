using LinkPara.CampaignManagement.Domain.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.CampaignManagement.Domain.Entities;

public class IWalletCard : AuditEntity
{
    public Guid UserId { get; set; }
    public string WalletNumber { get; set; }
    public CardApplicationStatus CardApplicationStatus { get; set; }
    public string FullName { get; set; }
    public string IdentityNumber { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string AddressDetail { get; set; }
    public int CityId { get; set; }
    public int TownId { get; set; }
    public UserType UserType { get; set; }
    public bool IsApprovedIndividualFrameworkAgreement { get; set; }
    public string IndividualFrameworkAgreementVersion { get; set; }
    public bool IsApprovedPreliminaryInformationAgreement { get; set; }
    public string PreliminaryInformationAgreementVersion { get; set; }
    public bool IsApprovedKvkkAgreement { get; set; }
    public string KvkkAgreementVersion { get; set; }
    public bool IsApprovedCommercialElectronicCommunicationAggrement { get; set; }
    public string CommercialElectronicCommunicationAggrementVersion { get; set; }
    public int CardId { get; set; }
    public string CardNumber { get; set; }
    public int CustomerBranchId { get; set; }
    public int CustomerId { get; set; }
    public string ErrorMessage { get; set; }
}
