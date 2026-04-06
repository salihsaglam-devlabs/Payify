using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.Calendar.Application.Commons.Interfaces;
using LinkPara.Calendar.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;

namespace LinkPara.Calendar.Application.Features.HolidayDetails.Commands.DeleteHolidayDetail;

public class DeleteHolidayDetailCommand : IRequest
{
    public Guid Id { get; set; }
}

public class DeleteHolidayDetailCommandHandler : IRequestHandler<DeleteHolidayDetailCommand>
{
    private readonly IGenericRepository<HolidayDetail> _repository;
    private readonly IAuditLogService _auditLogService;

    public DeleteHolidayDetailCommandHandler(IGenericRepository<HolidayDetail> repository,
        IAuditLogService auditLogService)
    {
        _repository = repository;
        _auditLogService = auditLogService; 
    }

    public async Task<Unit> Handle(DeleteHolidayDetailCommand request, CancellationToken cancellationToken)
    {
        var holidayDetail = await _repository.GetByIdAsync(request.Id);

        if (holidayDetail is null)
        {
            throw new NotFoundException(nameof(HolidayDetail), request.Id);
        }

        await _repository.DeleteAsync(holidayDetail);

        await _auditLogService.AuditLogAsync(
            new AuditLog
            {
                IsSuccess = true,
                LogDate = DateTime.Now,
                Operation = "DeleteHolidayDetail",
                SourceApplication = "Calender",
                Resource = "Holiday",
                Details = new Dictionary<string, string>
                {
                         {"Id", holidayDetail.Id.ToString() },
                         {"DurationInDays", holidayDetail.DurationInDays.ToString() },
                         {"HolidayId", holidayDetail.HolidayId.ToString() }
                }
            });

        return Unit.Value;
    }
}
