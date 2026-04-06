using System.Reflection;

namespace LinkPara.SharedModels.Notification;

public class NotificationBase
{
    public string NotificationName => GenerateNotificationName();
    public string DisplayName => NotificationName.Split('.').Last();
    public Dictionary<string, string> Parameters => GetStringProperties();
    public List<Dictionary<string, string>> AttachmentList { get; set; }

    private string GenerateNotificationName()
    {
        var @namespace = GetType().Namespace ?? string.Empty;
        var microserviceName = @namespace.Split('.').Last();
        return $"{microserviceName}.{GetType().Name}";
    }
    private Dictionary<string, string> GetStringProperties()
    {
        return GetType()
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(p =>
                p.PropertyType == typeof(string) &&
                p.GetMethod?.IsPublic == true &&
                p.SetMethod?.IsPublic == true)
            .ToDictionary(p => p.Name, p => (string?)p.GetValue(this) ?? "");
    }
    public void AddAttachment(string fileSizeBytes, string mimeType, string fileName, string fileExtension, string base64FileBytes)
    {
        AttachmentList ??= new List<Dictionary<string, string>>();
        AttachmentList.Add(new Dictionary<string, string>
        {
            { "file_size_bytes", fileSizeBytes },
            { "mime_type", mimeType },
            { "file_name", fileName },
            { "extension", fileExtension },
            { "body", base64FileBytes }
        });
    }
}