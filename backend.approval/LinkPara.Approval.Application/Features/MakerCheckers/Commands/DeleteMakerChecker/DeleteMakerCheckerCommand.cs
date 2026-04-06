using LinkPara.Approval.Application.Commons.Attributes;
using LinkPara.Approval.Domain.Entities;
using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Approval.Application.Features.MakerCheckers.Commands.DeleteMakerChecker;

public class DeleteMakerCheckerCommand : IRequest
{
    [Audit]
    public Guid Id { get; set; }
}

public class DeleteMakerCheckerCommandHandler : IRequestHandler<DeleteMakerCheckerCommand>
{
    private readonly IGenericRepository<MakerChecker> _makerCheckerRepository;

    public DeleteMakerCheckerCommandHandler(IGenericRepository<MakerChecker> makerCheckerRepository)
    {
        _makerCheckerRepository = makerCheckerRepository;
    }

    public async Task<Unit> Handle(DeleteMakerCheckerCommand request, CancellationToken cancellationToken)
    {
        var makerChecker = await _makerCheckerRepository.GetAll()
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken: cancellationToken);

        if (makerChecker is null)
        {
            throw new NotFoundException(nameof(MakerChecker), request.Id);
        }

        makerChecker.RecordStatus = RecordStatus.Passive;

        await _makerCheckerRepository.UpdateAsync(makerChecker);       

        return Unit.Value;
    }
}
