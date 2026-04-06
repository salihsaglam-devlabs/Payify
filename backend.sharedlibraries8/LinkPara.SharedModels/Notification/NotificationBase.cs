using System.Reflection;
using System.Text.RegularExpressions;

namespace LinkPara.SharedModels.Notification;

public class NotificationBase
{
    public string NotificationName => GenerateNotificationName();
    public string DisplayName => NotificationName.Split('.').Last();
    public Dictionary<string, string> Parameters => GetStringProperties();
    public List<Dictionary<string, string>> AttachmentList { get; set; }
    public Dictionary<string, string> ParametersWithFieldName => GetParametersWithFieldName();
    
    public Dictionary<string, string> GetParameters(string language)
    {
        var type = GetType();
        var result = new Dictionary<string, string>();

        var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(p => p.PropertyType == typeof(string) &&
                        p.GetMethod?.IsPublic == true &&
                        p.SetMethod?.IsPublic == true);

        foreach (var prop in properties)
        {
            var localizedKey = NotificationRegistry.GetLocalizedDisplayName(prop, language) ?? prop.Name;
            var value = prop.GetValue(this) as string ?? "";
            result[localizedKey] = value;
        }

        return result;
    }
    private Dictionary<string, string> GetParametersWithFieldName()
    {
        var type = GetType();
        var result = new Dictionary<string, string>();

        var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(p => p.PropertyType == typeof(string) &&
                        p.GetMethod?.IsPublic == true &&
                        p.SetMethod?.IsPublic == true);

        foreach (var prop in properties)
        {
            var localizedKey = NotificationRegistry.GetLocalizedDisplayName(prop, "tr") ?? prop.Name;
            var localizedValue = NotificationRegistry.GetLocalizedDisplayName(prop, "en") ?? prop.Name;
            result[localizedKey] = prop.Name;
            result[localizedValue] = prop.Name;
        }

        return result;
    }

    public Dictionary<string, string> GetPropertiesWithDisplayNames(string language)
    {
        var type = GetType();
        var result = new Dictionary<string, string>();

        var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(p => p.PropertyType == typeof(string) &&
                        p.GetMethod?.IsPublic == true &&
                        p.SetMethod?.IsPublic == true);

        foreach (var prop in properties)
        {
            var localizedKey = NotificationRegistry.GetLocalizedDisplayName(prop, language) ?? prop.Name;
            
            result[prop.Name] = localizedKey;
        }

        return result;
    }

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
    
    public void AddAttachment(List<Dictionary<string, string>> attachmentList)
    {
        AttachmentList ??= new List<Dictionary<string, string>>();
        AttachmentList.AddRange(attachmentList);
    }
    
    public string RenderTemplate(string template, string language)
    {
        var type = GetType();
        var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);

        var keyToProperty = properties
            .Where(p => p.PropertyType == typeof(string))
            .SelectMany(p => p.GetCustomAttributes<LocalizedDisplayAttribute>()
                .Where(attr => attr.Language.Equals(language, StringComparison.OrdinalIgnoreCase))
                .Select(attr => new { attr.Name, Property = p }))
            .ToDictionary(x => x.Name, x => x.Property);

        var result = Regex.Replace(template, "{{(.*?)}}", match =>
        {
            var key = match.Groups[1].Value.Trim();
            if (keyToProperty.TryGetValue(key, out var prop))
            {
                var value = prop.GetValue(this) as string;
                return value ?? "";
            }
            return match.Value;
        });

        return result;
    }

}