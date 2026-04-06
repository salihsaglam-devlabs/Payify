
namespace LinkPara.SharedModels.Exceptions;

public class AlreadyInUseException : ApiException
{
    public AlreadyInUseException(string name)
        : base(ErrorCode.AlreadyInUse, name)
    {
    }
}