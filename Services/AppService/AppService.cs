using NetworkSearchApproachesComparison.Models;


namespace NetworkSearchApproachesComparison.Services.AppService;

public class AppService : IAppService
{
    private readonly NetworkService.NetworkService _networkService;
    public AppService()
    {
        _networkService = new NetworkService.NetworkService(new Network(), new NetworkDeviceService.NetworkDeviceService(new NetworkDevice()));
    }
    public void Run()
    {

    }
}