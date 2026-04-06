using LinkPara.ApiGateway.BackOffice.Commons.Mappings;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Enums;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;

namespace LinkPara.ApiGateway.BackOffice.Commons.Models.ExcelExportModels;

public class MerchantExcelExportModel : IMapFrom<MerchantDto>
{
    public string Name { get; set; }
    public MerchantStatus MerchantStatus { get; set; }
    public ApplicationChannel ApplicationChannel { get; set; }
    public IntegrationMode IntegrationMode { get; set; }
    public string MccCode { get; set; }
    public string Language { get; set; }
    public string WebSiteUrl { get; set; }
    public string PhoneCode { get; set; }
    public DateTime AgreementDate { get; set; }
    public Guid? SalesPersonId { get; set; }
    public decimal MonthlyTurnover { get; set; }
    public int PaymentDueDay { get; set; }
    public bool Is3dRequired { get; set; }
    public bool InstallmentAllowed { get; set; }
    public bool InternationalCardAllowed { get; set; }
    public bool PreAuthorizationAllowed { get; set; }
    public bool FinancialTransactionAllowed { get; set; }
    public bool PaymentAllowed { get; set; }
    public string PricingProfileNumber { get; set; }
    public DateTime CreateDate { get; set; }
    public string RejectReason { get; set; }
    public string ParameterValue { get; set; }
}
