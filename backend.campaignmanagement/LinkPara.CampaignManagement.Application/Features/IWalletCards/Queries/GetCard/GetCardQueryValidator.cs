using FluentValidation;

namespace LinkPara.CampaignManagement.Application.Features.IWalletCards.Queries.GetCard;

public class GetCardQueryValidator : AbstractValidator<GetCardQuery>
{
    public GetCardQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotNull()
            .NotEmpty();
    }
}
