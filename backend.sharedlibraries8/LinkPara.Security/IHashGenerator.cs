namespace LinkPara.Security;

public interface IHashGenerator
{
    string Generate(string message, string key);
}