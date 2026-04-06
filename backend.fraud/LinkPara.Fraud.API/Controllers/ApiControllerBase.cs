using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.Fraud.API.Controllers
{
    [ApiController]
    [Route("v1/[controller]")]
    [Authorize]
    public abstract class ApiControllerBase : ControllerBase
    {
        protected ISender Mediator =>
            HttpContext.RequestServices.GetRequiredService<ISender>();
    }
}