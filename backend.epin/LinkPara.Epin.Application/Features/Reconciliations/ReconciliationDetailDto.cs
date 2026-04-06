using LinkPara.Epin.Application.Commons.Mappings;
using LinkPara.Epin.Domain.Entities;
using LinkPara.Epin.Domain.Enums;

namespace LinkPara.Epin.Application.Features.Reconciliations;

public class ReconciliationDetailDto : IMapFrom<ReconciliationDetail>
{
    public Guid Id { get; set; }
    public string UserFullName { get; set; }
    public string Email { get; set; }
    public string TransactionDate { get; set; }
    public decimal Amount { get; set; }
    public string PublisherName { get; set; }
    public string BrandName { get; set; }
    public string ProductName { get; set; }
    public DateTime ReconciliationDate { get; set; }
    public ReconciliationDetailStatus ReconciliationDetailStatus { get; set; }
    public int ExternalOrderId { get; set; }
    public bool HasInternalOrders { get; set; }
    public bool HasExternalOrders { get; set; }
    public Guid? OrderId { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<ReconciliationDetail, ReconciliationDetailDto>()
            .ForMember(d => d.UserFullName, opt => opt.MapFrom(s => s.Order != null ? s.Order.UserFullName : string.Empty))
            .ForMember(d => d.Email, opt => opt.MapFrom(s => s.Order != null ? s.Order.Email : string.Empty))
            .ForMember(d => d.Amount, opt => opt.MapFrom(s => s.Order != null ? s.Order.Price : default(decimal)))
            .ForMember(d => d.PublisherName, opt => opt.MapFrom(s => (s.Order != null && s.Order.Publisher != null) ? s.Order.Publisher.Name : string.Empty))
            .ForMember(d => d.BrandName, opt => opt.MapFrom(s => (s.Order != null && s.Order.Brand != null) ? s.Order.Brand.Name : string.Empty))
            .ForMember(d => d.ProductName, opt => opt.MapFrom(s => !string.IsNullOrEmpty(s.ProductName) ? s.ProductName : (( s.Order != null ) ? s.Order.Equivalent : string.Empty)))
            .ForMember(d => d.ExternalOrderId, opt => opt.MapFrom(s => (s.OrderHistory != null) ? s.OrderHistory.ExternalId : default(int)));
    }
}
