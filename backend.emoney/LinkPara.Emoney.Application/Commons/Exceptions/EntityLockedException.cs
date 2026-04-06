using System.Runtime.Serialization;
using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Emoney.Application.Commons.Exceptions;

[Serializable]
public class EntityLockedException : Exception
{
    public EntityLockedException() : base("Entity locked!")
    {
        
    }
    public EntityLockedException(string message) : base(message)
    {
    }
    
    protected EntityLockedException(SerializationInfo info, StreamingContext context) 
        : base(info, context)
    {
    }
}