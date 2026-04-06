namespace LinkPara.Billing.Application.Commons.Models.EventBusConfiguration
{
    public class EventBusSettings
    {
        public string Host { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public int RetryCount { get; set; }
        public string ApplicationName { get; set; }
        public string Exchange { get; set; }
    }
}
