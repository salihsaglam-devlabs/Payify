namespace LinkPara.SharedModels.BusModels.Commands.Notification
{
    public class SendPushNotificationWithMessage
    {
        public string HeaderMessage { get; set; }
        public string Message { get; set; }
        public Dictionary<string, string> TemplateParameters { get; set; }
        public Dictionary<string, string> Data { get; set; }
        public List<string> Tokens { get; set; }
        public List<NotificationUserInfo> UserList { get; set; }
    }
}
