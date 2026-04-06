using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LinkPara.ApiGateway.Services.Identity.Models.Requests
{
    public class CheckPasswordRequest
    {
        public string Password { get; set; } = string.Empty;
    }
}