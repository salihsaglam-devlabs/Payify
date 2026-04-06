using FluentValidation;

namespace LinkPara.PF.Application.Features.MerchantContents.Command.UploadMerchantLogo;

public class UploadMerchantLogoCommandValidator : AbstractValidator<UploadMerchantLogoCommand>
{
    public UploadMerchantLogoCommandValidator()
    {
        RuleFor(s => s.MerchantLogo.ContentType)
            .MaximumLength(100);
    }
}