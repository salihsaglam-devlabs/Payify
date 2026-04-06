using System.Net;

namespace LinkPara.HealthCheck.Services;

public class SecurityService : ISecurityService
{
    public bool IpIsAllowed(List<string> allowedList)
    {
        var hostName = Dns.GetHostName();
        var ipList = Dns.GetHostEntry(hostName, System.Net.Sockets.AddressFamily.InterNetwork).AddressList; ;

        var allowed = false;

        foreach (var allowedIp in allowedList)
        {
            if (allowedIp.Contains("/"))
            {
                foreach (var localIp in ipList)
                {
                    allowed = IsInRange(localIp.ToString(), allowedIp);

                    if (allowed)
                    {
                        return true;
                    }
                }
            }
            else
            {
                foreach (var localIp in ipList)
                {
                    allowed = allowedIp.Equals(localIp.ToString());

                    if (allowed)
                    {
                        return true;
                    }
                }
            }
        }

        return allowed;
    }

    // true if ipAddress falls inside the CIDR range, example
    // bool result = IsInRange("10.50.30.7", "10.0.0.0/8");
    private bool IsInRange(string ipAddress, string CIDRmask)
    {
        string[] parts = CIDRmask.Split('/');

        int IP_addr = BitConverter.ToInt32(IPAddress.Parse(ipAddress).GetAddressBytes(), 0);
        int CIDR_addr = BitConverter.ToInt32(IPAddress.Parse(parts[0]).GetAddressBytes(), 0);
        int CIDR_mask = IPAddress.HostToNetworkOrder(-1 << (32 - int.Parse(parts[1])));

        return ((IP_addr & CIDR_mask) == (CIDR_addr & CIDR_mask));
    }
}
