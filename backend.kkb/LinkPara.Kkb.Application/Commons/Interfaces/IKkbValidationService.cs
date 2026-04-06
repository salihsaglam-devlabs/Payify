using LinkPara.Kkb.Application.Features.Kkb;

namespace LinkPara.Kkb.Application.Commons.Interfaces
{
    public interface IKkbValidationService
    {
        Task<ValidateIbanResponse> IbanValidationAsync(string iban, string identityNo);
        Task<InquireIbanResponse> IbanInquireAsync(string iban);
    }
}