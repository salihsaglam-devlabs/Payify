using System.Runtime.Serialization;

namespace LinkPara.SharedModels.Exceptions;

[Serializable]
public class GenericException : Exception, ISerializable
{
    public readonly string Code;

    public GenericException(string code)
    {
        Code = code;
    }
    
    public GenericException(string code, string message)
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

    protected GenericException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
        Code = (string)info.GetValue("Code", typeof(string));
    }
}
