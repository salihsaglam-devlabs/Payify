using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.Emoney.API.Controllers;

[ApiController]
[Route("v1/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    protected ISender Mediator => 
        HttpContext.RequestServices.GetRequiredService<ISender>();
}