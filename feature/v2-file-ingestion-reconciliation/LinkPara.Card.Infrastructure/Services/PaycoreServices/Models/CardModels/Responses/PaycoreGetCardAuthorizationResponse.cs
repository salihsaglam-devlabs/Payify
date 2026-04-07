namespace LinkPara.Card.Infrastructure.Services.PaycoreServices.Models.CardModels.Responses;

public class PaycoreGetCardAuthorizationResponse
{
    public bool authDomEcom { get; set; }
    public bool authDomMoto { get; set; }
    public bool authDomNoCVV2 { get; set; }
    public bool authDomContactless { get; set; }
    public bool authDomCash { get; set; }
    public bool authDomCasino { get; set; }
    public bool authIntEcom { get; set; }
    public bool authIntMoto { get; set; }
    public bool authIntNoCVV2 { get; set; }
    public bool authIntContactless { get; set; }
    public bool authIntCash { get; set; }
    public bool authIntCasino { get; set; }
    public bool authIntPosSale { get; set; }
    public bool auth3dSecure { get; set; }
    public string auth3dSecureType { get; set; }
    public bool authCloseInstallment { get; set; }
    public bool visaAccountUpdaterOpt { get; set; }
    public string CardNo { get; set; }
}
