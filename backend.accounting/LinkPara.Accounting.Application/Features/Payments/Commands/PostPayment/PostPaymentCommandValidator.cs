using FluentValidation;

namespace LinkPara.Accounting.Application.Features.Payments.Commands.PostPayment;

public class PostPaymentCommandValidator : AbstractValidator<PostPaymentCommand>
{
    public PostPaymentCommandValidator()
    {
        
    }
}