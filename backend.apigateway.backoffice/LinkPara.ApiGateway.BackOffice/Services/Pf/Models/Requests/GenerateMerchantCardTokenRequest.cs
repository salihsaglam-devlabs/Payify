using LinkPara.ApiGateway.BackOffice.Authentication;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;

public class GenerateCardTokenRequest : SignatureAuthentication
{
    public string CardNumber { get; set; }
    public string ExpireMonth { get; set; }
    public string ExpireYear { get; set; }
    public string Cvv { get; set; }
}

public class GenerateMerchantCardTokenRequest : GenerateCardTokenRequest
{
    public GenerateMerchantCardTokenRequest(GenerateCardTokenRequest request)
    {
        Cvv = request.Cvv;
        Nonce = request.Nonce;
        Signature = request.Signature;
        CardNumber = request.CardNumber;
        ExpireMonth = request.ExpireMonth;
        ExpireYear = request.ExpireYear;
        PublicKey = request.PublicKey;
        ConversationId = request.ConversationId;
    }

    public Guid MerchantId { get; set; }
}
