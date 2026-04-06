using LinkPara.IKS.Domain.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.IKS.Domain.Entities;

public class IksTerminal : AuditEntity
{
    public Guid MerchantId { get; set; }
    public string ReferenceCode { get; set; }
    public string GlobalMerchantId { get; set; }
    public string PspMerchantId { get; set; }
    public string TerminalId { get; set; }
    public string StatusCode { get; set; }
    public string Type { get; set; }
    public string BrandCode { get; set; }
    public string Model { get; set; }
    public string SerialNo { get; set; }
    public int OwnerPspNo { get; set; }
    public string OwnerTerminalId { get; set; }
    public string BrandSharing { get; set; }
    public string PinPad { get; set; }
    public string Contactless { get; set; }
    public string ConnectionType { get; set; }
    public string VirtualPosUrl { get; set; }
    public string HostingTaxNo { get; set; }
    public string HostingTradeName { get; set; }
    public string HostingUrl { get; set; }
    public string PaymentGwTaxNo { get; set; }
    public string PaymentGwTradeName { get; set; }
    public string PaymentGwUrl { get; set; }
    public int ServiceProviderPspNo { get; set; }
    public string FiscalNo { get; set; }
    public int TechPos { get; set; }
    public string ServiceProviderPspMerchantId { get; set; }
    public string PfMainMerchantId { get; set; }
    public string ResponseCode { get; set; }
    public string ResponseCodeExplanation { get; set; }
    public Guid VposId { get; set; }
    public Guid PhysicalPosId { get; set; }
    public TerminalStatus TerminalStatus { get; set; }
    public string CreatedNameBy { get; set; }
}