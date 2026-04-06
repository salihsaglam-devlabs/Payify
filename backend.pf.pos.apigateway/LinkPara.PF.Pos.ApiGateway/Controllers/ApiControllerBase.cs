using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.PF.Pos.ApiGateway.Controllers;

[ApiController]
[Route("v1/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    protected string UserId => HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);
}
