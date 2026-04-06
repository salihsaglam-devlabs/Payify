using FluentValidation;

namespace LinkPara.PF.Application.Features.MerchantContents.Command.CreateMerchantContent;

public class CreateMerchantContentCommandValidator : AbstractValidator<CreateMerchantContentCommand>
{
    public CreateMerchantContentCommandValidator()
    {
        RuleFor(x => x.MerchantId)
            .NotNull()
            .NotEmpty();
        
        RuleFor(x => x.Name)
            .NotNull()
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.ContentSource)
            .NotNull().IsInEnum();

        RuleFor(x => x.Contents)
            .NotNull()
            .NotEmpty();

        RuleForEach(x => x.Contents)
            .SetValidator(new CreateMerchantContentValidator());
    }

    public class CreateMerchantContentValidator : AbstractValidator<MerchantContentVersionDto>
    {
        public CreateMerchantContentValidator()
        {
            RuleFor(x => x.Title)
                .NotNull().NotEmpty()
                .MaximumLength(150);
            
            RuleFor(x => x.LanguageCode)
                .NotNull().NotEmpty()
                .MaximumLength(10);
        }
    }
}