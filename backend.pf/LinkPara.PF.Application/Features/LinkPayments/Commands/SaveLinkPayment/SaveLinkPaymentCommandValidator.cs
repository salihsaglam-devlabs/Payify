using FluentValidation;
using LinkPara.PF.Application.Features.Links.Command.SaveLink;

namespace LinkPara.PF.Application.Features.LinkPayments.Commands.SaveLinkPayment;

public class SaveLinkPaymentCommandValidator : AbstractValidator<SaveLinkPaymentCommand>
{
    public SaveLinkPaymentCommandValidator()
    {

    }
}