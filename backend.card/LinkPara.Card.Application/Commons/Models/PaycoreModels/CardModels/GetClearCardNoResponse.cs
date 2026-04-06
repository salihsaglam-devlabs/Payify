namespace LinkPara.Card.Application.Commons.Models.PaycoreModels.CardModels
{
    public class GetClearCardNoResponse : PaycoreResponse
    {
        public string CardNo { get; set; }
        public string CardToken { get; set; }
        public int CardUniqId { get; set; }
        
    }

}
