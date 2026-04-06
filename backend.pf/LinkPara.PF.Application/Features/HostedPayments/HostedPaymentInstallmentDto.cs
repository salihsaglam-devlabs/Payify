using LinkPara.PF.Application.Commons.Mappings;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;

namespace LinkPara.PF.Application.Features.HostedPayments;

public class HostedPaymentInstallmentDto : IMapFrom<HostedPaymentInstallment>
{
    public CardNetwork CardNetwork { get; set; }
    public int Installment { get; set; }
    public decimal? Amount { get; set; }
}