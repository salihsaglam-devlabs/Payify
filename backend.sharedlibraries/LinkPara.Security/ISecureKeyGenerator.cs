namespace LinkPara.Security;

public interface ISecureKeyGenerator
{
    string Generate(int length);
}