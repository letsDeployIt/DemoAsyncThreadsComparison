using NetworkSearchApproachesComparison.Models;
using System.Net;
using System.Net.Sockets;

namespace AsyncThreadsComparison.Services.NetworkDeviceService;

public class NetworkDeviceService : INetworkDeviceService
{
    public NetworkDevice NetworkDevice { get; }

    public NetworkDeviceService(NetworkDevice networkDevice)
    {
        NetworkDevice = networkDevice;
        GetLocalhostIP();
    }

    public void GetLocalhostIP()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());

        foreach (IPAddress ipAddress in host.AddressList)
        {
            if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
            {
                NetworkDevice.IpAddress = ipAddress;
            }
        }
    }
}