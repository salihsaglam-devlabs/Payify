using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.Calendar.API.Controllers
{
    [ApiController]
    [Route("v1/[controller]")]
    public class ApiControllerBase : ControllerBase
    {
        protected ISender Mediator =>
         HttpContext.RequestServices.GetRequiredService<ISender>();
    }
}
