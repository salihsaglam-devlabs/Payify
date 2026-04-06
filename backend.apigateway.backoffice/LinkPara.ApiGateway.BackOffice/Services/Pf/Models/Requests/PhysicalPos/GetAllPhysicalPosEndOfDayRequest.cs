using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Enums.PhysicalPos;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests.PhysicalPos;

public class GetAllPhysicalPosEndOfDayRequest : SearchQueryParams
{
    public Guid? MerchantId { get; set; }
    public string BatchId { get; set; }
    public string PosMerchantId { get; set; }
    public string PosTerminalId { get; set; }
    public DateTime? DateStart { get; set; }
    public DateTime? DateEnd { get; set; }
    public int? SaleCount { get; set; }
    public int? VoidCount { get; set; }
    public int? RefundCount { get; set; }
    public int? InstallmentSaleCount { get; set; }
    public int? FailedCount { get; set; }
    public decimal? SaleAmount { get; set; }
    public decimal? VoidAmount { get; set; }
    public decimal? RefundAmount { get; set; }
    public decimal? InstallmentSaleAmount { get; set; }
    public string Currency { get; set; }
    public int? InstitutionId { get; set; }
    public string Vendor { get; set; }
    public string SerialNumber { get; set; }
    public EndOfDayStatus? Status { get; set; }
}