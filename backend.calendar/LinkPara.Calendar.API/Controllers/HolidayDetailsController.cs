using LinkPara.Calendar.Application.Features.HolidayDetails.Commands.DeleteHolidayDetail;
using LinkPara.Calendar.Application.Features.HolidayDetails.Commands.SaveHolidayDetail;
using LinkPara.Calendar.Application.Features.HolidayDetails.Queries;
using LinkPara.Calendar.Application.Features.HolidayDetails.Queries.GetHolidayDetails;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.Calendar.API.Controllers;

public class HolidayDetailsController : ApiControllerBase
{
    /// <summary>
    /// Returns all holiday details. 
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpGet("")]
    public async Task<List<HolidayDetailDto>> GetHolidayDetailsAsync([FromQuery] GetHolidayDetailsQuery request)
    {
        return await Mediator.Send(request);
    }

    /// <summary>
    /// Creates holiday details  
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost("")]
    public async Task SaveHolidayDetailAsync(SaveHolidayDetailCommand request)
    {
        await Mediator.Send(request);
    }

    /// <summary>
    /// Deletes holiday detail. 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpDelete("")]
    public async Task DeleteHolidayDetailAsync(Guid id)
    {
        await Mediator.Send(new DeleteHolidayDetailCommand { Id = id });
    }
}
