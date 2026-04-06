using LinkPara.SharedModels.Notification.NotificationModels;

namespace LinkPara.SharedModels.Notification;

public static class NotificationRegistry
{
    private static readonly Type[] SupportedTypes = new[] { typeof(INotificationEvent), typeof(INotificationOrder) };

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

    public static Dictionary<string, string> GetAllNotificationNamesWithDisplayNames()
    {
        return GetAllNotificationTypes()
            .Select(t => Activator.CreateInstance(t) as INotificationModel)
            .Where(instance => instance != null && !string.IsNullOrWhiteSpace(instance.NotificationName))
            .GroupBy(instance => instance!.NotificationName)
            .Select(g => g.First())
            .ToDictionary(
                instance => instance!.NotificationName,
                instance => instance.DisplayName ?? instance.NotificationName
            );
    }

    public static Dictionary<string, string> GetNotificationParameters(string notificationName)
    {
        var type = GetAllNotificationTypes()
            .FirstOrDefault(t =>
            {
                var instance = (INotificationModel)Activator.CreateInstance(t)!;
                return instance.NotificationName == notificationName;
            });

        if (type == null) return new();

        var instance = (INotificationModel)Activator.CreateInstance(type)!;
        return instance.Parameters;
    }
}
