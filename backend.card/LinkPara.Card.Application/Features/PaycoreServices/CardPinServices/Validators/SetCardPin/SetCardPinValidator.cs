using FluentValidation;
using LinkPara.Card.Application.Features.PaycoreServices.CardPinServices.Commands.SetCardPin;

namespace LinkPara.Card.Application.Features.PaycoreServices.CardPinServices.Validators.SetCardPin;

    public class SetCardPinValidator: AbstractValidator<SetCardPinCommand>
    {
        public SetCardPinValidator()
        {
            RuleFor(x => x.TokenPan)
                .NotEmpty().WithMessage("TokenPan zorunludur.");
            RuleFor(x => x.ClearPin)
                .NotEmpty().WithMessage("ClearPin zorunludur.")
                .Length(4).WithMessage("ClearPin 4 haneli olmalıdır.")
                .Must(value => value.All(char.IsDigit)).WithMessage("ClearPin sadece rakamlardan oluşmalıdır."); 
               
    }
}

