using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Enums;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;

public class GetFilterMerchantPoolRequest : SearchQueryParams
{
    public DateTime? CreateDateStart { get; set; }
    public DateTime? CreateDateEnd { get; set; }
    public CompanyType? CompanyType { get; set; }
    public MerchantPoolStatus? MerchantPoolStatus { get; set; }
    public MerchantType? MerchantType { get; set; }
    public PosType? PosType { get; set; }
    public int? MoneyTransferStartHourStart { get; set; }
    public int? MoneyTransferStartHourFinish { get; set; }
    public int? MoneyTransferStartMinuteStart { get; set; }
    public int? MoneyTransferStartMinuteFinish { get; set; }
}
