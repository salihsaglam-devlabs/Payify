namespace LinkPara.SharedModels.Notification;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = true)]
public class LocalizedDisplayAttribute : Attribute
{
    public string Language { get; }
    public string Name { get; }

    public LocalizedDisplayAttribute(string name, string language)
    {
        Language = language;
        Name = name;
    }
}