using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace LinkPara.Emoney.Application.Commons.Strategies;

public class NoRetryExecutionStrategy : ExecutionStrategy
{
    public NoRetryExecutionStrategy(DbContext context)
        : base(context, maxRetryCount: 0, maxRetryDelay: TimeSpan.Zero) { }

    protected override bool ShouldRetryOn(Exception exception)
    {
        return false;
    }
}
