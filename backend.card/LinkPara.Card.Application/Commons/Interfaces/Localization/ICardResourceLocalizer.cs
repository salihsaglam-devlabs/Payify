#nullable enable
namespace LinkPara.Card.Application.Commons.Interfaces.Localization;

public interface ICardResourceLocalizer
{
    string Get(string key);
    string Get(string key, params object[] arguments);
}
