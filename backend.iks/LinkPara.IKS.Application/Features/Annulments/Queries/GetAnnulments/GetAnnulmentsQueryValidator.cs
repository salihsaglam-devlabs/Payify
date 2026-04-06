using FluentValidation;

namespace LinkPara.IKS.Application.Features.Annulments.Queries.GetAnnulments
{
    public class GetAnnulmentsQueryValidator : AbstractValidator<GetAnnulmetsQuery>
    {
        public GetAnnulmentsQueryValidator()
        {
            RuleFor(b => b.GlobalMerchantId).NotEmpty().NotNull().Length(8);
        }
    }
}