using LinkPara.Billing.Application.Commons.Mappings;
using LinkPara.Billing.Domain.Entities;
using LinkPara.Billing.Domain.Enums;

namespace LinkPara.Billing.Application.Features.Billing;

public class BillTransactionResponseDto : IMapFrom<Transaction>
{
    public Guid Id { get; set; }
    public Guid VendorId { get; set; }
    public string VendorName { get; set; }
    public Guid SectorId { get; set; }
    public string SectorName { get; set; }
    public Guid InstitutionId { get; set; }
    public string InstitutionName { get; set; }
    public decimal BillAmount { get; set; }
    public string Currency { get; set; }
    public string BillId { get; set; }
    public string BillNumber { get; set; }
    public string SubscriptionNumber1 { get; set; }
    public string SubscriptionNumber2 { get; set; }
    public string SubscriptionNumber3 { get; set; }
    public DateTime? BillDate { get; set; }
    public DateTime BillDueDate { get; set; }
    public DateTime PaymentDate { get; set; }
    public Guid UserId { get; set; }
    public string ProvisionReferenceId { get; set; }
    public string PayeeFullName { get; set; }
    public string SubscriberName { get; set; }
    public string PayeeEmail { get; set; }
    public string PayeeMobile { get; set; }
    public string ServiceRequestId { get; set; }
    public string ErrorCode { get; set; }
    public string ErrorMessage { get; set; }
    public TransactionStatus TransactionStatus { get; set; }
    
    public void Mapping(Profile profile)
    {
        profile.CreateMap<Transaction, BillTransactionResponseDto>()
            .ForMember(dest => dest.VendorName, opt => opt.MapFrom(t => t.Institution.ActiveVendor.Name))
            .ForMember(dest => dest.SectorId, opt => opt.MapFrom(t => t.Institution.SectorId))
            .ForMember(dest => dest.SectorName, opt => opt.MapFrom(t => t.Institution.Sector.Name))
            .ForMember(dest => dest.InstitutionName, opt => opt.MapFrom(t => t.Institution.Name))
            .ForMember(dest => dest.SubscriptionNumber1, opt => opt.MapFrom<SensitiveDataResolver<Transaction, BillTransactionResponseDto>, Dictionary<string,string>>(p => new Dictionary<string, string>{{"PropertyName","Default"},{"SourceData",p.SubscriptionNumber1}}))
            .ForMember(dest => dest.SubscriptionNumber2, opt => opt.MapFrom<SensitiveDataResolver<Transaction, BillTransactionResponseDto>, Dictionary<string,string>>(p => new Dictionary<string, string>{{"PropertyName","Default"},{"SourceData",p.SubscriptionNumber2}}))
            .ForMember(dest => dest.SubscriptionNumber2, opt => opt.MapFrom<SensitiveDataResolver<Transaction, BillTransactionResponseDto>, Dictionary<string,string>>(p => new Dictionary<string, string>{{"PropertyName","Default"},{"SourceData",p.SubscriptionNumber3}}))
            .ForMember(dest => dest.PayeeFullName, opt => opt.MapFrom<SensitiveDataResolver<Transaction, BillTransactionResponseDto>, Dictionary<string,string>>(p => new Dictionary<string, string>{{"PropertyName","PayeeFullName"},{"SourceData",p.PayeeFullName}}))
            .ForMember(dest => dest.PayeeEmail, opt => opt.MapFrom<SensitiveDataResolver<Transaction, BillTransactionResponseDto>, Dictionary<string,string>>(p => new Dictionary<string, string>{{"PropertyName","PayeeEmail"},{"SourceData",p.PayeeEmail}}))
            .ForMember(dest => dest.PayeeMobile, opt => opt.MapFrom<SensitiveDataResolver<Transaction, BillTransactionResponseDto>, Dictionary<string,string>>(p => new Dictionary<string, string>{{"PropertyName","PayeeMobile"},{"SourceData",p.PayeeMobile}}));
    }
}