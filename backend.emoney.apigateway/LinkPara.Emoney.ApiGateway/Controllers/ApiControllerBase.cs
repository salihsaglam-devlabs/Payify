using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LinkPara.Emoney.ApiGateway.Controllers;


[ApiController]
[Route("v1/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    protected string UserId => HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);
}

