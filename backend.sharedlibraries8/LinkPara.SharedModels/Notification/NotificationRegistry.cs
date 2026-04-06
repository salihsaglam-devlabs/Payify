using System.Reflection;
using LinkPara.SharedModels.Notification.NotificationModels;

namespace LinkPara.SharedModels.Notification;

public static class NotificationRegistry
{
    private static readonly Type[] SupportedTypes = new[] { typeof(INotificationEvent), typeof(INotificationOrder), typeof(INotificationCustom) };

    public static IEnumerable<Type> GetAllNotificationTypes()
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(x => x.GetTypes())
            .Where(t =>
                !t.IsAbstract &&
                t.GetConstructor(Type.EmptyTypes) != null &&
                SupportedTypes.Any(i => i.IsAssignableFrom(t))
            );
    }

    public static Dictionary<string, string> GetAllNotificationNamesWithDisplayNames(EventNotificationField eventNotificationField, string language)
    {
        return GetAllNotificationTypes()
            .Select(t => new
            {
                Instance = Activator.CreateInstance(t) as INotificationModel,
                DisplayName = GetLocalizedDisplayName(t, language) ?? t.Name
            })
            .Where(x => x.Instance != null 
                        && !string.IsNullOrWhiteSpace(x.Instance.NotificationName) 
                        && (eventNotificationField == EventNotificationField.All 
                            || (x.Instance.EventNotificationField & eventNotificationField) != 0))
            .GroupBy(x => x.Instance!.NotificationName)
            .Select(g => g.First())
            .ToDictionary(
                x => x.Instance!.NotificationName,
                x => x.DisplayName
            );
    }

    public static NotificationType GetNotificationTypeByEventName(string notificationName)
    {
        var type = GetAllNotificationTypes()
            .FirstOrDefault(t =>
            {
                var instance = Activator.CreateInstance(t) as INotificationModel;
                return instance != null && instance.NotificationName == notificationName;
            });

        if (type == null)
            throw new ArgumentException($"Notification with name '{notificationName}' not found.", nameof(notificationName));
        
        if (typeof(INotificationEvent).IsAssignableFrom(type))
            return NotificationType.Event;
        if (typeof(INotificationOrder).IsAssignableFrom(type))
            return NotificationType.Order;

        return NotificationType.Custom;
    }
    
    public static List<string> GetAllNotificationNames()
    {
        return GetAllNotificationTypes()
            .Select(t => Activator.CreateInstance(t) as INotificationModel)
            .Where(instance => instance != null && !string.IsNullOrWhiteSpace(instance.NotificationName))
            .Select(instance => instance!.NotificationName)
            .Distinct()
            .ToList();
    }

    public static string? GetLocalizedDisplayName(MemberInfo memberInfo, string language)
    {
        return memberInfo
            .GetCustomAttributes<LocalizedDisplayAttribute>()
            .FirstOrDefault(attr => attr.Language.Equals(language, StringComparison.OrdinalIgnoreCase))
            ?.Name;
    }

    public static Dictionary<string, string> GetNotificationParameters(string notificationName, string language)
    {
        var type = GetAllNotificationTypes()
            .FirstOrDefault(t =>
            {
                var instance = (INotificationModel)Activator.CreateInstance(t)!;
                return instance.NotificationName == notificationName;
            });

        if (type == null) return new();

        var instance = (INotificationModel)Activator.CreateInstance(type)!;

        return (instance as NotificationBase)?.GetParameters(language) ?? new();
    }
}
