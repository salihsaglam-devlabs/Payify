using LinkPara.Card.Application.Commons.Interfaces;

namespace LinkPara.Card.Infrastructure.Services;

internal sealed class SystemTimeProvider : ITimeProvider
{
    public DateTime Now => DateTime.Now;
}

