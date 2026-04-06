using LinkPara.PF.Application.Commons.Models.Payments.Response;
using LinkPara.PF.Application.Features.Payments.Commands.PointInquiry;

namespace LinkPara.PF.Application.Commons.Interfaces;

public interface IPointInquiryService
{
    Task<PointInquiryResponse> PointInquiryAsync(PointInquiryCommand request);
}
