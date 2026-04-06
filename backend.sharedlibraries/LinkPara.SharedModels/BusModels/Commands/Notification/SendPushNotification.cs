namespace LinkPara.SharedModels.BusModels.Commands.Notification
{
    public class SendPushNotification
    {
        public string TemplateName { get; set; }
        public string Topic { get; set; }
        public Dictionary<string, string> TemplateParameters { get; set; }
        public Dictionary<string, string> Data { get; set; }
        public List<string> Tokens { get; set; }
        public List<NotificationUserInfo> UserList { get; set; }
    }
}
