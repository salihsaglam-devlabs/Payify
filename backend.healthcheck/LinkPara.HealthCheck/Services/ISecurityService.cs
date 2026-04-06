using System.Net;

namespace LinkPara.HealthCheck.Services;

public interface ISecurityService
{
    bool IpIsAllowed(List<string> allowedIpAddresses);
}
