using Microsoft.Extensions.Localization;
using System.Reflection;

namespace LinkPara.Approval.Helper;

public static class UpdatedFieldsHelper
{
    public static Dictionary<string, object> GetUpdatedFields<T, T2>(T oldObject, T2 newObject, IStringLocalizer _localizer)
    {
        PropertyInfo[] properties = typeof(T).GetProperties();
        var changes = new Dictionary<string, object>();

        foreach (PropertyInfo pi in properties)
        {
            var value1 = typeof(T).GetProperty(pi.Name).GetValue(oldObject, null);
            var oldObjectProperty = typeof(T2).GetProperty(pi.Name);

            if (oldObjectProperty is null)
            {
                continue;
            }

            var value2 = oldObjectProperty.GetValue(newObject, null);

            if (value1 != value2 && (value1 == null || !value1.Equals(value2)))
            {
                var updatedField = new Dictionary<string, object>
                    {
                        {"OldValue", _localizer.GetString(value1 is null ? string.Empty : value1.ToString()).Value },
                        {"NewValue", _localizer.GetString(value2 is null ? string.Empty : value2.ToString()).Value }
                    };
                changes.Add(_localizer.GetString(pi.Name).Value, updatedField);
            }
        }
        return changes;
    }
}
