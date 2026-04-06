namespace LinkPara.SharedModels.Exceptions;

public class InternalErrorException : GenericException
{
    public InternalErrorException()
        : base(ErrorCode.InternalError)
    {
        
    }

}