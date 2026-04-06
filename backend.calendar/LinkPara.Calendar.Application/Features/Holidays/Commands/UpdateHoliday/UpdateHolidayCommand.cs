using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.Calendar.Application.Commons.Interfaces;
using LinkPara.Calendar.Domain.Entities;
using LinkPara.Calendar.Domain.Enums;
using LinkPara.SharedModels.Exceptions;
using MediatR;
using LinkPara.Calendar;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Calendar.Application.Features.Holidays.Commands.UpdateHoliday;

public class UpdateHolidayCommand : IRequest
{
    public Guid Id { get; set; }
    public string CountryCode { get; set; }
    public string Name { get; set; }
    public HolidayType HolidayType { get; set; }
}

public class UpdateHolidayCommandHandler : IRequestHandler<UpdateHolidayCommand, Unit>
{
    private readonly IGenericRepository<Holiday> _repository;
    private readonly IAuditLogService _auditLogService;

    public UpdateHolidayCommandHandler(IGenericRepository<Holiday> repository,
        IAuditLogService auditLogService)
    {
        _repository = repository;
        _auditLogService = auditLogService;
    }

    public async Task<Unit> Handle(UpdateHolidayCommand request, CancellationToken cancellationToken)
    {
        var holiday = await _repository.GetByIdAsync(request.Id);

        if (holiday is null)
        {
            throw new NotFoundException(nameof(Holiday), request.Id);
        }

        holiday.Name = request.Name;
        holiday.CountryCode = request.CountryCode.ToUpper();
        holiday.HolidayType = request.HolidayType;

        await _repository.UpdateAsync(holiday);

        await _auditLogService.AuditLogAsync(
            new AuditLog
            {
                IsSuccess = true,
                LogDate = DateTime.Now,
                Operation = "UpdateHoliday",
                SourceApplication = "Calender",
                Resource = "Holiday",
                Details = new Dictionary<string, string>
                {
                      {"Id", request.Id.ToString() },
                      {"CountryCode", request.CountryCode },
                      {"Name", request.Name },
                      {"HolidayType", request.HolidayType.ToString()},               
                }
            });

        return Unit.Value;
    }
}

