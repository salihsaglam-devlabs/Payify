namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi;

public abstract class VposBase
{
    protected abstract string FormatAmount(decimal amount);

    protected abstract string FormatExpiryDate(string month, string year);
}
