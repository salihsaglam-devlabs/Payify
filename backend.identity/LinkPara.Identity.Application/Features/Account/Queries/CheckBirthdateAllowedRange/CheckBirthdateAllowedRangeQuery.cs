using LinkPara.HttpProviders.BusinessParameter;
using LinkPara.Identity.Application.Common.Exceptions;
using LinkPara.SharedModels.Exceptions;
using MediatR;
using Microsoft.Extensions.Localization;

namespace LinkPara.Identity.Application.Features.Account.Queries.CheckBirthdateAllowedRange;
public class CheckBirthdateAllowedRangeQuery : IRequest<bool>
{
    public DateTime BirthDate { get; set; }
}
public class CheckBirthdateAllowedRangeQueryHandler : IRequestHandler<CheckBirthdateAllowedRangeQuery, bool>
{
    private readonly IParameterService _parameterService;
    private readonly IStringLocalizer _localizer;
    public CheckBirthdateAllowedRangeQueryHandler(IParameterService parameterService, IStringLocalizerFactory factory)
    {
        _parameterService = parameterService;
        _localizer = factory.Create("Exceptions", "LinkPara.Identity.API");
    }
    public async Task<bool> Handle(CheckBirthdateAllowedRangeQuery request, CancellationToken cancellationToken)
    {
        var customerAgeRequirements = await _parameterService.GetParametersAsync("CustomerAgeRequirements");
        
        _ = int.TryParse(
            customerAgeRequirements.FirstOrDefault(p => p.ParameterCode == "MinAge")?.ParameterValue, out var minAge);
        var rangeStart = DateTime.Now.AddYears(-1 * minAge);

        _ = int.TryParse(
            customerAgeRequirements.FirstOrDefault(p => p.ParameterCode == "MaxAge")?.ParameterValue, out var maxAge);
        var rangeEnd = DateTime.Now.AddYears(-1 * maxAge);

        if (!(request.BirthDate <= rangeStart && request.BirthDate >= rangeEnd))
        {
            var exceptionMessage = _localizer.GetString("BirthdateOutOfRange")
                .Value.Replace("@@minAge", 
                    customerAgeRequirements.FirstOrDefault(p => p.ParameterCode == "MinAge")?.ParameterValue);
            
            throw new BirthdateOutOfRangeException(exceptionMessage);
        }
        return true;
    }
}