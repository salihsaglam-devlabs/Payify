namespace LinkPara.Card.Application.Commons.Models.PaycoreModels.CardModels
{
    public class GetCardEmbossStatusResponse
    {
        public int EmbossStat { get; set; }
        public string Description { get; set; }
        public DateTimeOffset EmbossRequestDate { get; set; }

    }
}
