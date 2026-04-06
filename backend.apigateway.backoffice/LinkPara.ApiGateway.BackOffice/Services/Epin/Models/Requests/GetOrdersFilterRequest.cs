using LinkPara.ApiGateway.BackOffice.Services.Epin.Models.Enums;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.Epin.Models.Requests;

public class GetOrdersFilterRequest : SearchQueryParams
{
    public DateTime? CreateDateStart { get; set; }
    public DateTime? CreateDateEnd { get; set; }
    public string Email { get; set; }
    public OrderStatus? OrderStatus { get; set; }
    public Guid? PublisherId { get; set; }
    public Guid? BrandId { get; set; }
    public int? ProductId { get; set; }
}
