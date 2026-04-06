using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.Calendar.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;

namespace LinkPara.Calendar.Application.Features.Holidays.Commands.DeleteHoliday;

public class DeleteHolidayCommand : IRequest
{
    public Guid Id { get; set; }
}

public class DeleteHolidayCommandHandler : IRequestHandler<DeleteHolidayCommand>
{
    private readonly IGenericRepository<Holiday> _repository;
    private readonly IAuditLogService _auditLogService;

    public DeleteHolidayCommandHandler(IGenericRepository<Holiday> repository,
        IAuditLogService auditLogService)
    {
        _repository = repository;
        _auditLogService = auditLogService; 
    }

    public async Task<Unit> Handle(DeleteHolidayCommand request, CancellationToken cancellationToken)
    {
        var holiday = await _repository.GetByIdAsync(request.Id);

        if (holiday is null)
        {
            throw new NotFoundException(nameof(Holiday), request.Id);
        }

        await _repository.DeleteAsync(holiday);

        await _auditLogService.AuditLogAsync(
            new AuditLog
            {
                IsSuccess = true,
                LogDate = DateTime.Now,
                Operation = "DeleteHoliday",
                SourceApplication = "Calender",
                Resource = "Holiday",
                Details = new Dictionary<string, string>
                {
                        {"Id", holiday.Id.ToString() },
                        {"CountryCode", holiday.CountryCode },
                        {"Name", holiday.Name },
                        {"HolidayType", holiday.HolidayType.ToString()},
                }
            });

        return Unit.Value;
    }
}
