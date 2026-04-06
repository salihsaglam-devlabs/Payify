namespace LinkPara.HttpProviders.IKS.Models.Response
{
    public class IKSError
    {
        public string type { get; set; }
        public string path { get; set; }
        public int httpCode { get; set; }
        public string httpMessage { get; set; }
        public string moreInformation { get; set; }
        public Error[] errors { get; set; }
        public DateTime timestamp { get; set; }

    }
    public class Error
    {
        public string objectName { get; set; }
        public string field { get; set; }
        public string code { get; set; }
        public string description { get; set; }
    }
}
