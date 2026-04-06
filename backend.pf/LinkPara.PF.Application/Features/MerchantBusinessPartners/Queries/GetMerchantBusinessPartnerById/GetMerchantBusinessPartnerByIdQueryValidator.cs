using FluentValidation;

namespace LinkPara.PF.Application.Features.MerchantBusinessPartners.Queries.GetMerchantBusinessPartnerById
{
    public class GetMerchantBusinessPartnerByIdQueryValidator : AbstractValidator<GetMerchantBusinessPartnerByIdQuery>
    {
        public GetMerchantBusinessPartnerByIdQueryValidator()
        {
            RuleFor(x => x.Id)
           .NotNull().NotEmpty();
        }
    }
}