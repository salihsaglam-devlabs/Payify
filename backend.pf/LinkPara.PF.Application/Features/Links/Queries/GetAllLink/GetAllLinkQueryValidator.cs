using FluentValidation;

namespace LinkPara.PF.Application.Features.Links.Queries.GetAllLink;

public class GetAllLinkQueryValidator : AbstractValidator<GetAllLinkQuery>
{
    public GetAllLinkQueryValidator()
    {
        RuleFor(x => x.MerchantId)
            .NotNull().NotEmpty();

        RuleFor(s => s.LinkSearchType)
            .IsInEnum();
    }
}
