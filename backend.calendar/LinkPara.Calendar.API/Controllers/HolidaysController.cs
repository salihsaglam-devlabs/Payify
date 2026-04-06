using LinkPara.Calendar.Application.Features.Holidays;
using LinkPara.Calendar.Application.Features.Holidays.Commands.DeleteHoliday;
using LinkPara.Calendar.Application.Features.Holidays.Commands.SaveHoliday;
using LinkPara.Calendar.Application.Features.Holidays.Commands.UpdateHoliday;
using LinkPara.Calendar.Application.Features.Holidays.Queries.GetHolidays;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.Calendar.API.Controllers;

public class HolidaysController : ApiControllerBase
{
    /// <summary>
    /// Returns all holidays. 
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpGet("")]
    public async Task<List<HolidayDto>> GetHolidaysAsync([FromQuery] GetHolidaysQuery request)
    {
        return await Mediator.Send(request);
    }

    /// <summary>
    /// Creates a holiday. 
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost("")]
    public async Task SaveHolidayAsync(SaveHolidayCommand request)
    {
        await Mediator.Send(request);
    }

    /// <summary>
    /// Updates a holiday. 
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPut("")]
    public async Task UpdateHolidayAsync(UpdateHolidayCommand request)
    {
        await Mediator.Send(request);
    }

    /// <summary>
    /// Deletes a holiday. 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpDelete("")]
    public async Task DeleteHolidayAsync(Guid id)
    {
        await Mediator.Send(new DeleteHolidayCommand { Id = id });
    }
}
