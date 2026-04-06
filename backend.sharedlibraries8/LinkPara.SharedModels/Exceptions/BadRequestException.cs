namespace LinkPara.SharedModels.Exceptions;

public class BadRequestException : GenericException
{
    public BadRequestException()
        : base(ErrorCode.NotFound)
    {
        
    }

}