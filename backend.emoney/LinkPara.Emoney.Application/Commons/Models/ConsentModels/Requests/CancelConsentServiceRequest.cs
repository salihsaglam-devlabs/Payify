using LinkPara.Emoney.Application.Commons.Mappings;
using LinkPara.Emoney.Application.Features.ConsentOperations.Commands.CancelConsent;

namespace LinkPara.Emoney.Application.Commons.Models.ConsentModels.Requests
{
    public class CancelConsentServiceRequest : IMapFrom<CancelConsentCommand>
    {
        public string ConsentId { get; set; }
        public string Username { get; set; }
        public string ConsentType { get; set; }
        public string RevokeCode { get; set; }
        public bool IsDecoupledAuth { get; set; }
    }
}
