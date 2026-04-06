using FluentValidation;

namespace LinkPara.Content.Application.Features.Contents.Commands.DeleteDataContainer
{
   public class DeleteDataContainerCommandValidator : AbstractValidator<DeleteDataContainerCommand>
   {
      public DeleteDataContainerCommandValidator()
      {
         RuleFor(x => x.Key).NotEmpty().MaximumLength(50).MinimumLength(3);
      }
   }
}
