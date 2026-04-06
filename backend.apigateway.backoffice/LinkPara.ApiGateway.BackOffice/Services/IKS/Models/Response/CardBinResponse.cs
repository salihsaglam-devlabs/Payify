namespace LinkPara.ApiGateway.BackOffice.Services.IKS.Models.Response;

public class CardBinResponse
{
    public Binlist[] binList { get; set; }
    public bool success { get; set; }
    public object[] validationResult { get; set; }
}

public class Binlist
{
    public string binRangeMin { get; set; }
    public string binRangeMax { get; set; }
    public bool main { get; set; }
    public int cardNoLength { get; set; }
    public bool validateForCheckDigit { get; set; }
    public string cardTypeNo { get; set; }
    public string cardType { get; set; }
    public string brandName { get; set; }
    public int memberNo { get; set; }
    public string memberName { get; set; }
    public bool activeForClearing { get; set; }
    public bool activeForFraud { get; set; }
}
