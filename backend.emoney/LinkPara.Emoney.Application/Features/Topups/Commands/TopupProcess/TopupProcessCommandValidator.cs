using FluentValidation;
using LinkPara.Emoney.Application.Commons.Models.PaymentProviderModels.PayifyPf.Requests;

namespace LinkPara.Emoney.Application.Features.Topups.Commands.TopupProcess;

public class TopupProcessCommandValidator : AbstractValidator<TopupProcessCommand>
{
    public TopupProcessCommandValidator()
    {
        RuleFor(r => r.BaseRequest.WalletNumber)
        .NotNull()
        .NotEmpty();

        RuleFor(r => r.BaseRequest.Amount)
        .NotNull()
        .NotEmpty();

        RuleFor(r => r.BaseRequest.CardTopupRequestId)
        .NotNull()
        .NotEmpty();

        RuleFor(r => r.BaseRequest.Currency)
        .NotNull()
        .NotEmpty();

        RuleFor(r => r.BaseRequest.UserId)
        .NotNull()
        .NotEmpty();

        RuleFor(r => r.BaseRequest)
            .NotNull()
            .ChildRules(baseRequest =>
            {
                When(x => x.BaseRequest is TopupProcessRequest payifyPfRequest, () =>
                {
                    RuleFor(r => ((TopupProcessRequest)r.BaseRequest).CardHolderName)
                        .NotNull()
                        .NotEmpty();
                });
            });
    }
}
