using FluentValidation;

namespace LinkPara.PF.Application.Features.Merchants.Command.SaveAnnulment
{
    public class SaveAnnulmentCommandValidator : AbstractValidator<SaveAnnulmentCommand>
    {
        public SaveAnnulmentCommandValidator()
        {
            RuleFor(b => b.Id).NotEmpty().NotNull();
            RuleFor(b => b.AnnulmentCode).NotEmpty().NotNull();
            RuleFor(b => b.AnnulmentDescription).NotEmpty().NotNull();
        }
    }
}