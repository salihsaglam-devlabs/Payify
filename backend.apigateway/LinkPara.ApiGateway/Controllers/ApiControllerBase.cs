using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LinkPara.ApiGateway.Controllers;

[ApiController]
[Route("v1/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    protected string UserId => HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);
}
