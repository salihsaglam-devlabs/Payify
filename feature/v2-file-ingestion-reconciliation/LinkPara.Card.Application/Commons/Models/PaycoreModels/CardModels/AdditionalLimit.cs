namespace LinkPara.Card.Application.Commons.Models.PaycoreModels.CardModels
{
    public class AdditionalLimit
    {
        public int CurrencyCode { get; set; }
        public int CurrentLimit { get; set; }
        public int LimitRatio { get; set; }
        public string EffectType { get; set; }
        public string UsageType { get; set; }
    }
}
