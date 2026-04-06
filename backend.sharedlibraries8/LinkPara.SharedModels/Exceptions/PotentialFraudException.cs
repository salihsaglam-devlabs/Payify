namespace LinkPara.SharedModels.Exceptions;

public class PotentialFraudException : CustomApiException
{
    public PotentialFraudException()
        : base(ErrorCode.PotentialFraud, "The transaction is a potential fraud!")
    {
    }
    
    public PotentialFraudException(string message)
        : base(ErrorCode.PotentialFraud, message)
    {
    }
}
