namespace LinkPara.Security;

public interface ISecureRandomGenerator
{
    long GenerateSecureRandomNumber(int maxLength);
    long GenerateSecureRandomNumber(long min, long max);
}
