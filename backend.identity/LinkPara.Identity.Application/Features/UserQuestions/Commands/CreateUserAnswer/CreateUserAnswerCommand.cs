using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Domain.Entities;
using LinkPara.SharedModels.Persistence;
using LinkPara.SharedModels.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using LinkPara.Audit.Models;
using LinkPara.Audit;

namespace LinkPara.Identity.Application.Features.UserQuestions.Commands.CreateUserAnswer;

public class CreateUserAnswerCommand : IRequest
{
    public Guid UserId { get; set; }
    public Guid SecurityQuestionId { get; set; }
    public string Answer { get; set; }
}

public class CreateUserAnswerCommandHandler : IRequestHandler<CreateUserAnswerCommand>
{
    private readonly UserManager<User> _userManager;
    private readonly IRepository<UserSecurityAnswer> _userSecurityAnswerRepository;
    private readonly IAuditLogService _auditLogService;

    public CreateUserAnswerCommandHandler(UserManager<User> userManager,
        IRepository<UserSecurityAnswer> userSecurityAnswerRepository,
        IAuditLogService auditLogService)
    {
        _userManager = userManager;
        _userSecurityAnswerRepository = userSecurityAnswerRepository;
        _auditLogService = auditLogService;
    }

    public async Task<Unit> Handle(CreateUserAnswerCommand command, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(command.UserId.ToString());

        if (user is null)
        {
            throw new NotFoundException(nameof(User));
        }

        var answer = await _userSecurityAnswerRepository.GetAll()
            .SingleOrDefaultAsync(q => q.UserId == command.UserId
                                    && q.RecordStatus == RecordStatus.Active, 
            cancellationToken);

        if (answer is not null)
        {

            await _auditLogService.AuditLogAsync(
            new AuditLog
            {
                IsSuccess = false,
                LogDate = DateTime.Now,
                Operation = "CreateUserAnswer",
                SourceApplication = "Identity",
                Resource = "UserSecurityAnswer",
                UserId = command.UserId,
                Details = new Dictionary<string, string>
                {
                    {"UserId", answer.UserId.ToString() },
                    {"ErrorMessage" , "DuplicateRecordException"}
                }
            });

            throw new DuplicateRecordException(nameof(UserSecurityAnswer));
        }

        var userSecurityAnswer = new UserSecurityAnswer
        {
            UserId = command.UserId,
            SecurityQuestionId = command.SecurityQuestionId,
            AnswerHash = _userManager.PasswordHasher.HashPassword(user, command.Answer)
        };

        await _userSecurityAnswerRepository.AddAsync(userSecurityAnswer);

        await _auditLogService.AuditLogAsync(
            new AuditLog
            {
                IsSuccess = true,
                LogDate = DateTime.Now,
                Operation = "CreateUserAnswer",
                SourceApplication = "Identity",
                Resource = "UserSecurityAnswer",
                UserId = command.UserId,
                Details = new Dictionary<string, string>
                {
                     {"UserId", command.UserId.ToString() },
                }
            });

        return Unit.Value;
    }
}