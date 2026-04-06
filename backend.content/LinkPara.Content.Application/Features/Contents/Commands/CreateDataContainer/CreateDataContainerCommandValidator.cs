using FluentValidation;

namespace LinkPara.Content.Application.Features.Contents.Commands.CreateDataContainer;

public class CreateDataContainerCommandValidator : AbstractValidator<CreateDataContainerCommand>
{
   public CreateDataContainerCommandValidator()
   {
      RuleFor(x => x.Key).NotEmpty().MaximumLength(25).MinimumLength(3);
      RuleFor(x => x.Value).NotEmpty();
   }

}
