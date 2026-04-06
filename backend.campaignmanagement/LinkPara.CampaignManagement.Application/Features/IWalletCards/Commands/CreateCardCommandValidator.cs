using FluentValidation;

namespace LinkPara.CampaignManagement.Application.Features.IWalletCards.Commands;

public class CreateCardCommandValidator : AbstractValidator<CreateCardCommand>
{
    public CreateCardCommandValidator()
    {
        RuleFor(x => x.CityId)
            .NotNull()
            .NotEmpty()
            .GreaterThan(0);

        RuleFor(x => x.TownId)
            .NotNull()
            .NotEmpty()
            .GreaterThan(0);

        RuleFor(x => x.UserId)
            .NotNull()
            .NotEmpty();

        RuleFor(x => x.WalletNumber)
            .NotNull()
            .NotEmpty();

        RuleFor(x => x.FullName)
            .NotNull()
            .NotEmpty();

        RuleFor(x => x.IdentityNumber)
            .NotNull()
            .NotEmpty();

        RuleFor(x => x.Email)
            .NotNull()
            .NotEmpty();

        RuleFor(x => x.PhoneNumber)
            .NotNull()
            .NotEmpty();

        RuleFor(x => x.AddressDetail)
            .NotNull()
            .NotEmpty();

        RuleFor(x => x.IndividualFrameworkAgreementVersion)
            .NotNull()
            .NotEmpty();

        RuleFor(x => x.PreliminaryInformationAgreementVersion)
            .NotNull()
            .NotEmpty();

        RuleFor(x => x.KvkkAgreementVersion)
            .NotNull()
            .NotEmpty();
    }
}
