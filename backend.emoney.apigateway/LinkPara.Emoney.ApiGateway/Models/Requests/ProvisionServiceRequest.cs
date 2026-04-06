
using LinkPara.Emoney.ApiGateway.Models.Enums;

namespace LinkPara.Emoney.ApiGateway.Models.Requests;

public class ProvisionServiceRequest : ProvisionRequest
{
    public ProvisionServiceRequest(ProvisionRequest request, Guid partnerId)
    {
        WalletNumber = request.WalletNumber;
        Amount = request.Amount;
        CurrencyCode = request.CurrencyCode;
        Description = request.Description;
        ConversationId = request.ConversationId;
        ClientIpAddress = request.ClientIpAddress;

        ProvisionSource = ProvisionSource.Partner;
        PartnerId = partnerId;
    }

    public ProvisionSource ProvisionSource { get; set; }
    public Guid PartnerId { get; set; }
}
