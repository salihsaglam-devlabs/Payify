using LinkPara.PF.Application.Commons.Models.Payments.Response;
using LinkPara.PF.Application.Features.Payments.Commands.Inquire;

namespace LinkPara.PF.Application.Commons.Interfaces;

public interface IInquireService
{
    Task<InquireResponse> InquireAsync(InquireCommand request);
}