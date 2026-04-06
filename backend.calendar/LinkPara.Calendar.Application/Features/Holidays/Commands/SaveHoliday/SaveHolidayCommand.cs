using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.Calendar.Application.Commons.Interfaces;
using LinkPara.Calendar.Domain.Entities;
using LinkPara.Calendar.Domain.Enums;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Calendar.Application.Features.Holidays.Commands.SaveHoliday;

public class SaveHolidayCommand : IRequest
{
    public string CountryCode { get; set; }
    public string Name { get; set; }
    public HolidayType HolidayType { get; set; }
}

public class SaveHolidayCommandHandler : IRequestHandler<SaveHolidayCommand, Unit>
{
    private readonly IGenericRepository<Holiday> _repository;
    private readonly IAuditLogService _auditLogService;

    public SaveHolidayCommandHandler(IGenericRepository<Holiday> repository,
        IAuditLogService auditLogService)
    {
        _repository = repository;
        _auditLogService = auditLogService;
    }

    public async Task<Unit> Handle(SaveHolidayCommand request, CancellationToken cancellationToken)
    {
        var activeHoliday = await _repository.GetAll()
            .FirstOrDefaultAsync(b => b.CountryCode == request.CountryCode.ToUpper()
            && b.HolidayType == request.HolidayType
            && b.Name.Equals(request.Name)
            && b.RecordStatus == RecordStatus.Active, cancellationToken);

        if (activeHoliday is not null)
        {
            throw new DuplicateRecordException(nameof(Holiday));
        }

        var holiday = new Holiday
        {
            CountryCode = request.CountryCode.ToUpper(),
            Name = request.Name,
            HolidayType = request.HolidayType,
        };

        await _repository.AddAsync(holiday);

        await _auditLogService.AuditLogAsync(
            new AuditLog
            {
                IsSuccess = true,
                LogDate = DateTime.Now,
                Operation = "SaveHoliday",
                SourceApplication = "Calender",
                Resource = "Holiday",
                Details = new Dictionary<string, string>
                {
                        {"Id", holiday.Id.ToString() },
                        {"CountryCode", request.CountryCode },
                        {"Name", request.Name },
                        {"HolidayType", request.HolidayType.ToString()},
                }
            });

        return Unit.Value;
    }
}
