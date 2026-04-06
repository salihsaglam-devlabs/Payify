using FluentValidation;

namespace LinkPara.Content.Application.Features.Contents.Commands.UpdateDataContainer
{
   public class UpdateDataContainerCommandValidator : AbstractValidator<UpdateDataContainerCommand>
   {
      public UpdateDataContainerCommandValidator()
      {
         RuleFor(x => x.Key).NotEmpty().MaximumLength(50).MinimumLength(3);
         RuleFor(x => x.Value).NotEmpty();
      }
   }
}
