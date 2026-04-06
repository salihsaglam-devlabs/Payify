using LinkPara.Identity.Domain.Entities;
using MediatR;
using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Application.Features.Account.Queries;
using Microsoft.EntityFrameworkCore;
using LinkPara.ContextProvider;
using LinkPara.Audit.Models;
using LinkPara.Audit;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Identity.Application.Features.Account.Commands.SaveUserPicture;
public class SaveUserPictureCommand : IRequest
{
    public UserPictureDto UserPicture { get; set; }
}

public class SaveUserPictureCommandHandler : IRequestHandler<SaveUserPictureCommand>
{
    private readonly IRepository<UserPicture> _userPictureRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly IContextProvider _contextProvider;

    public SaveUserPictureCommandHandler(IRepository<UserPicture> userPictureRepository,
        IAuditLogService auditLogService,
        IContextProvider contextProvider
        )
    {
        _userPictureRepository = userPictureRepository;
        _auditLogService = auditLogService;
        _contextProvider = contextProvider;
    }

    public async Task<Unit> Handle(SaveUserPictureCommand command, CancellationToken cancellationToken)
    {
        var userPicture = await _userPictureRepository.GetAll()
            .FirstOrDefaultAsync(x => x.UserId == command.UserPicture.UserId);

        var userId = _contextProvider.CurrentContext.UserId;
        var parseUserId = userId != null ? Guid.Parse(userId) : Guid.Empty;

        if (userPicture is not null)
        {
            userPicture.Bytes = command.UserPicture.Bytes;
            userPicture.ContentType = command.UserPicture.ContentType;

            await _userPictureRepository.UpdateAsync(userPicture);

            await _auditLogService.AuditLogAsync(
                new AuditLog
                {
                      IsSuccess = true,
                      LogDate = DateTime.Now,
                      Operation = "UpdateUserPicture",
                      SourceApplication = "Identity",
                      Resource = "UserPicture",
                      UserId = parseUserId,
                      Details = new Dictionary<string, string>
                      {
                           {"UserId", command.UserPicture.UserId.ToString() },
                           {"ContentType", command.UserPicture.ContentType },
                      }
                 });
        }
        else
        {
            userPicture = new UserPicture()
            {
                UserId = command.UserPicture.UserId,
                Bytes = command.UserPicture.Bytes,
                ContentType = command.UserPicture.ContentType
            };

            await _userPictureRepository.AddAsync(userPicture);


            await _auditLogService.AuditLogAsync(
                new AuditLog
                {
                    IsSuccess = true,
                    LogDate = DateTime.Now,
                    Operation = "SaveUserPicture",
                    SourceApplication = "Identity",
                    Resource = "UserPicture",
                    UserId = parseUserId,
                    Details = new Dictionary<string, string>
                    {
                        {"UserId", command.UserPicture.UserId.ToString() },
                        {"ContentType", command.UserPicture.ContentType },
                    }
                });
        }



        return Unit.Value;
    }
}