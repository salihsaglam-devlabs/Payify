using LinkPara.Content.Application.Commons.Interfaces;

namespace LinkPara.Content.Infrastructure.Services
{
   public class CurrentUserService : ICurrentUserService
   {
      public string UserId { get; }
   }
}