namespace LinkPara.SharedModels.Exceptions;

public class InvalidParameterException : GenericException
{
    public InvalidParameterException()
        : base(ErrorCode.InvalidParameters, "Invalid or missing parameter(s)!")
    {
    }

    public InvalidParameterException(string parameter)
        : base(ErrorCode.InvalidParameters,$"Parameter ({parameter}) is invalid or missing.")
    {
    }
}