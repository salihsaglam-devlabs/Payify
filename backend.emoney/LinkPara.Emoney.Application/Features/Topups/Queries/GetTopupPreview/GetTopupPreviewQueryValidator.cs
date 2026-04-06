using FluentValidation;

namespace LinkPara.Emoney.Application.Features.Topups.Queries.GetTopupPreview;

public class GetTopupPreviewQueryValidator : AbstractValidator<GetTopupPreviewQuery>
{
    public GetTopupPreviewQueryValidator()
    {
        RuleFor(r => r.CardNumber)
            .NotNull()
            .NotEmpty();

        RuleFor(r => r.Amount)
            .NotNull()
            .NotEmpty();

        RuleFor(r => r.Currency)
            .NotNull()
            .NotEmpty();

        RuleFor(r => r.UserId)
            .NotNull()
            .NotEmpty();

        RuleFor(r => r.WalletNumber)
           .NotNull()
           .NotEmpty();
    }
}

