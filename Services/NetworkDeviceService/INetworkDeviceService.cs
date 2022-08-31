using NetworkSearchApproachesComparison.Models;

namespace AsyncThreadsComparison.Services.NetworkDeviceService;

public interface INetworkDeviceService
{
    public NetworkDevice NetworkDevice { get; }

    public void GetLocalhostIP();
}