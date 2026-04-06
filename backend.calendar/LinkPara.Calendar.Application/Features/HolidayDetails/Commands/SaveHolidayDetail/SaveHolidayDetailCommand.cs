using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.Calendar.Application.Commons.Interfaces;
using LinkPara.Calendar.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Calendar.Application.Features.HolidayDetails.Commands.SaveHolidayDetail;

public class SaveHolidayDetailCommand : IRequest
{
    public int DurationInDays { get; set; }
    public DateTime DateOfHoliday { get; set; }
    public DateTime BeginningTime { get; set; }
    public DateTime EndingTime { get; set; }
    public Guid HolidayId { get; set; }
    public int Recurrence { get; set; }
}

public class SaveHolidayDetailCommandHandler : IRequestHandler<SaveHolidayDetailCommand, Unit>
{
    private readonly IGenericRepository<Holiday> _holidayRepository;
    private readonly IGenericRepository<HolidayDetail> _detailRepository;
    private readonly IAuditLogService _auditLogService;

    public SaveHolidayDetailCommandHandler(
        IGenericRepository<HolidayDetail> detailRepository,
        IGenericRepository<Holiday> holidayRepository,
        IAuditLogService auditLogService)
    {
        _detailRepository = detailRepository;
        _holidayRepository = holidayRepository;
        _auditLogService = auditLogService;
    }

    public async Task<Unit> Handle(SaveHolidayDetailCommand request, CancellationToken cancellationToken)
    {
        var holiday = await _holidayRepository.GetByIdAsync(request.HolidayId);

        if (holiday is null)
        {
            throw new NotFoundException(nameof(Holiday), request.HolidayId);
        }

        var activeHoliday = await _detailRepository.GetAll()
            .FirstOrDefaultAsync(b => b.HolidayId == request.HolidayId
            && b.BeginningTime == request.BeginningTime
            && b.EndingTime == request.EndingTime
            && b.RecordStatus == RecordStatus.Active, cancellationToken);

        if (activeHoliday is not null)
        {
            throw new DuplicateRecordException(nameof(HolidayDetail));
        }

        if (request.Recurrence > 1)
        {
            for (var i = 0; i < request.Recurrence; i++)
            {
                var beginningTime = request.BeginningTime.AddYears(i);
                var endingTime = request.EndingTime.AddYears(i);
                var dateOfHolidays = request.DateOfHoliday.Date.AddYears(i);

                await _detailRepository.AddAsync(new HolidayDetail
                {
                    BeginningTime = beginningTime,
                    EndingTime = endingTime,
                    DateOfHoliday = dateOfHolidays,
                    DurationInDays = request.DurationInDays,
                    HolidayId = request.HolidayId,
                });

                await _auditLogService.AuditLogAsync(
                new AuditLog
                {
                    IsSuccess = true,
                    LogDate = DateTime.Now,
                    Operation = "SaveHolidayDetail",
                    SourceApplication = "Calender",
                    Resource = "HolidayDetail",
                    Details = new Dictionary<string, string>
                    {
                     {"HolidayId", request.HolidayId.ToString() },
                     {"BeginningTime", request.BeginningTime.ToString() },
                     {"DurationInDays", request.DurationInDays.ToString() }
                    }
                });
            }
        }
        else
        {
            await _detailRepository.AddAsync(new HolidayDetail
            {
                BeginningTime = request.BeginningTime,
                EndingTime = request.EndingTime,
                DateOfHoliday = request.DateOfHoliday.Date,
                DurationInDays = request.DurationInDays,
                HolidayId = request.HolidayId,
            });

            await _auditLogService.AuditLogAsync(
            new AuditLog
            {
                IsSuccess = true,
                LogDate = DateTime.Now,
                Operation = "SaveHolidayDetail",
                SourceApplication = "Calender",
                Resource = "HolidayDetail",
                Details = new Dictionary<string, string>
                {
                     {"HolidayId", request.HolidayId.ToString() },
                     {"BeginningTime", request.BeginningTime.ToString() },
                     {"DurationInDays", request.DurationInDays.ToString() }
                }
            });
        }

        return Unit.Value;
    }
}