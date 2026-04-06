using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.Identity.Application.Common.Exceptions;
using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Identity.Application.Features.UserQuestions.Commands.UpdateUserAnswer;

public class UpdateUserAnswerCommand : IRequest
{
    public Guid UserId { get; set; }
    public Guid SecurityQuestionId { get; set; }
    public string Answer { get; set; }
    public string CurrentAnswer { get; set; }
}

public class UpdateUserAnswerCommandHandler : IRequestHandler<UpdateUserAnswerCommand>
{
    private readonly UserManager<User> _userManager;
    private readonly IRepository<UserSecurityAnswer> _userSecurityAnswerRepository;
    private readonly IAuditLogService _auditLogService;

    public UpdateUserAnswerCommandHandler(UserManager<User> userManager,
        IRepository<UserSecurityAnswer> userSecurityAnswerRepository,
        IAuditLogService auditLogService)
    {
        _userManager = userManager;
        _userSecurityAnswerRepository = userSecurityAnswerRepository;
        _auditLogService = auditLogService;
    }

    public async Task<Unit> Handle(UpdateUserAnswerCommand command, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(command.UserId.ToString());

        if (user is null)
        {
            throw new NotFoundException(nameof(User));
        }

        var answer = await _userSecurityAnswerRepository.GetAll()
            .SingleOrDefaultAsync(q => q.UserId == command.UserId && q.RecordStatus == RecordStatus.Active, cancellationToken);

        if (answer is null)
        {
            throw new NotFoundException(nameof(UserSecurityAnswer));
        }

        var result = _userManager.PasswordHasher.VerifyHashedPassword(user, answer.AnswerHash, command.CurrentAnswer);

        if (result == PasswordVerificationResult.Failed)
        {
            throw new UserSecurityQuestionAnswerMissmatchException();
        }


        answer.SecurityQuestionId = command.SecurityQuestionId;
        answer.AnswerHash = _userManager.PasswordHasher.HashPassword(user, command.Answer);

        await _userSecurityAnswerRepository.UpdateAsync(answer);

        await _auditLogService.AuditLogAsync(
              new AuditLog
              {
                  IsSuccess = true,
                  LogDate = DateTime.Now,
                  Operation = "UpdateUserAnswer",
                  SourceApplication = "Identity",
                  Resource = "UserSecurityAnswer",
                  UserId = command.UserId,
                  Details = new Dictionary<string, string>
                  {
                    {"UserId", answer.UserId.ToString() },
                  }
              });

        return Unit.Value;
    }
}