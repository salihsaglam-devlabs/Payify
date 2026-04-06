using FluentValidation;


namespace LinkPara.PF.Application.Features.MerchantLimits.Queries.GetFilterMerchantLimits
{
    public class GetMerchantLimitsQueryValidator : AbstractValidator<GetFilterMerchantLimitsQuery>
    {
        public GetMerchantLimitsQueryValidator()
        {
            RuleFor(s => s.MerchantId).NotNull().NotEmpty();
        }
    }
}
