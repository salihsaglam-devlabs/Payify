using LinkPara.Calendar.Application.Features.Days.Queries.IsHoliday;
using LinkPara.Calendar.Application.Features.Days.Queries.NextWorkDay;
using LinkPara.Calendar.Application.Features.Days.Queries.PreviousWorkDay;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.Calendar.API.Controllers
{
    public class DaysController : ApiControllerBase
    {
        /// <summary>
        /// Returns the holiday check of the entered date.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("isholiday")]
        public async Task<bool> IsHolidayAsync([FromQuery] IsHolidayQuery request)
        {
            return await Mediator.Send(request);
        }

        /// <summary>
        /// Returns the working day before the entered date.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("previous-work-day")]
        public async Task<DateTime> PreviousWorkDayAsync([FromQuery] PreviousWorkDayQuery request)
        {
            return await Mediator.Send(request);
        }

        /// <summary>
        /// Returns the working day after the entered date. 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("next-work-day")]
        public async Task<DateTime> NextWorkDayAsync([FromQuery] NextWorkDayQuery request)
        {
            return await Mediator.Send(request);
        }
    }
}
