namespace LinkPara.SharedModels.Exceptions;

public class ForbiddenAccessException : SystemException
{
    public ForbiddenAccessException()
        : base("Unauthorized user access request!")
    {
    }
}