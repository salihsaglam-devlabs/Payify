using LinkPara.Audit.Models;
using LinkPara.Audit;
using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SystemUser;
using MediatR;
using LinkPara.SharedModels.Persistence;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Identity.Application.Features.SecurityQuestions.Commands.DeleteSecurityQuestion;
public class DeleteSecurityQuestionCommand : IRequest
{
    public Guid Id { get; set; }
}

public class DeleteSecurityQuestionCommandHandler : IRequestHandler<DeleteSecurityQuestionCommand>
{
    private readonly IRepository<SecurityQuestion> _repository;
    private readonly IRepository<UserSecurityAnswer> _userSecurityAnswerRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly IApplicationUserService _applicationUserService;

    public DeleteSecurityQuestionCommandHandler(IRepository<SecurityQuestion> repository,
        IAuditLogService auditLogService,
        IApplicationUserService applicationUserService,
        IRepository<UserSecurityAnswer> userSecurityAnswerRepository)
    {
        _repository = repository;
        _auditLogService = auditLogService;
        _applicationUserService = applicationUserService;
        _userSecurityAnswerRepository = userSecurityAnswerRepository;
    }

    public async Task<Unit> Handle(DeleteSecurityQuestionCommand command, CancellationToken cancellationToken)
    {
        var securityQuestion = await _repository.GetAll().SingleOrDefaultAsync(x => x.Id == command.Id
        && x.RecordStatus == RecordStatus.Active);

        if (securityQuestion is null)
        {
            throw new NotFoundException(nameof(SecurityQuestion));
        }

        var answerList = await _userSecurityAnswerRepository.GetAll()
        .Where(q => q.SecurityQuestionId == command.Id
                            && q.RecordStatus == RecordStatus.Active).ToListAsync();

        if (answerList.Any())
        {
            throw new AlreadyInUseException(nameof(SecurityQuestion));
        }

        securityQuestion.RecordStatus = RecordStatus.Passive;
        await _repository.UpdateAsync(securityQuestion);

        await _auditLogService.AuditLogAsync(
              new AuditLog
              {
                  IsSuccess = true,
                  LogDate = DateTime.Now,
                  Operation = "DeleteSecurityQuestion",
                  SourceApplication = "Identity",
                  Resource = "SecurityQuestion",
                  UserId = _applicationUserService.ApplicationUserId,
                  Details = new Dictionary<string, string>
                  {
                        {"Question", securityQuestion.Question.ToString() },
                  }
              });

        return Unit.Value;
    }
}
