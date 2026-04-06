using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace LinkPara.SoftOtp.API.Controllers;

[ApiController]
[Route("v1/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    protected ISender Mediator =>
        HttpContext.RequestServices.GetRequiredService<ISender>();
}
