using FluentValidation;
using LinkPara.ApiGateway.Services.Emoney.Models.Enums;

namespace LinkPara.Emoney.Application.Features.OpenBankingOperations.Commands.CreatePaymentConsent;

public class CreatePaymentConsentCommandValidator : AbstractValidator<CreatePaymentConsentCommand>
{
    public CreatePaymentConsentCommandValidator()
    {
}
}
