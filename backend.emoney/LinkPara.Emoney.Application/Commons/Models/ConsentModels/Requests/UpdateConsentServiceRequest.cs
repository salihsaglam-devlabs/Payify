using LinkPara.Emoney.Application.Commons.Mappings;
using LinkPara.Emoney.Application.Features.ConsentOperations.Commands.UpdateConsent;
using LinkPara.Emoney.Domain.Enums;

namespace LinkPara.Emoney.Application.Commons.Models.ConsentModels.Requests
{
    public class UpdateConsentServiceRequest : IMapFrom<UpdateConsentCommand>
    {
        public string ConsentId { get; set; }
        public string CustomerId { get; set; }
        public string ConsentType { get; set; }
        public string SelectedAccountResponse { get; set; }
        public string CustomerName { get; set; }
        public string IdentityType { get; set; }
        public string IdentityValue { get; set; }
        public string CorporateIdentityType { get; set; }
        public string CorporateIdentityValue { get; set; }

    }
}
