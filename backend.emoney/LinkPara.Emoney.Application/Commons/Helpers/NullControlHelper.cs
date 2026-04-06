using LinkPara.SharedModels.Exceptions;
using Microsoft.Extensions.Logging;

namespace LinkPara.Emoney.Application.Commons.Helpers
{
    public static class NullControlHelper
    {
        public static void CheckAndThrowIfNull<T>(this T value, object searchValue, ILogger logger) where T : class
        {
            if (value is null)
            {
                logger.LogError("{value} is not found! SearchParameter:{searchValue}", value, searchValue);
                throw new NotFoundException(nameof(value), searchValue);
            }
        }
    }
}
