using System.Runtime.Serialization;

namespace LinkPara.SharedModels.Exceptions;

[Serializable]
public class ApiException : Exception, ISerializable
{
    public readonly string Code;

    public ApiException(string code, string message)
        : base(message)
    {
        Code = code;
    }
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        if (info == null)
        {
            throw new ArgumentNullException(nameof(info));
        }

        base.GetObjectData(info, context);

        info.AddValue("Code", Code, typeof(string));
    }

    protected ApiException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
        Code = (string)info.GetValue("Code", typeof(string));
    }
}