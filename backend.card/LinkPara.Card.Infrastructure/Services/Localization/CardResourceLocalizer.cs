#nullable enable
using LinkPara.Card.Application.Commons.Interfaces.Localization;
using Microsoft.Extensions.Localization;

namespace LinkPara.Card.Infrastructure.Services.Localization;

internal sealed class CardResourceLocalizer : ICardResourceLocalizer
{
    private readonly IStringLocalizer _localizer;

    public CardResourceLocalizer(IStringLocalizerFactory factory)
    {
        _localizer = factory.Create("Exceptions", "LinkPara.Card.API");
    }

    public string Get(string key)
    {
        return _localizer[key].Value;
    }

    public string Get(string key, params object[] arguments)
    {
        return _localizer[key, arguments].Value;
    }
}
