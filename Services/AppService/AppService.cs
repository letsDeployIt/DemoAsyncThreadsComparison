namespace AsyncThreadsComparison.Services.AppService;

public class AppService : IAppService
{
    private readonly NetworkService.NetworkService _networkService;

    public AppService()
    {
        _networkService = new NetworkService.NetworkService();
    }

    public void Run()
    {

    }
}