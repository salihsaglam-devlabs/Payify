using System.ComponentModel;

namespace LinkPara.PF.Pos.ApiGateway.Commons.Helpers;

public static class CollectionHelper
{
    public static Dictionary<string, string> ToDictionary<T>(T dynamicObject)
    {
        var dictionary = new Dictionary<string, string>();
        foreach (PropertyDescriptor propertyDescriptor in TypeDescriptor.GetProperties(dynamicObject))
        {
            var value = propertyDescriptor.GetValue(dynamicObject);
            var responseValue = value is null ? string.Empty : value.ToString();
            dictionary.Add(propertyDescriptor.Name, responseValue);
        }
        return dictionary;
    }
}
