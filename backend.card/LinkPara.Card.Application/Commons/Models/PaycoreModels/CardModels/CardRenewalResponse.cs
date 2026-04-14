namespace LinkPara.Card.Application.Commons.Models.PaycoreModels.CardModels
{
    public class CardRenewalResponse : PaycoreResponse
    {
        public string CardNo { get; set; }
        public int ExpiryDate { get; set; }
        public int PanSeqNumber { get; set; }
    }
}
