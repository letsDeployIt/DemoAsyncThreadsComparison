using System.Net;
using System.Net.Sockets;
using NetworkSearchApproachesComparison.Models;

namespace NetworkSearchApproachesComparison.Services.NetworkDeviceService;

public interface INetworkDeviceService
{
    public NetworkDevice NetworkDevice { get; }

    public void GetLocalhostIP();
}