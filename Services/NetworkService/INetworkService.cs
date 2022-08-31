using AsyncThreadsComparison.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AsyncThreadsComparison.Services.NetworkService;

public interface INetworkService
{
    IEnumerable<ExecutionDetailsDto> GetDevicesIpStatusMap();

    Task<IEnumerable<ExecutionDetailsDto>> GetDevicesIpStatusMapAsync();

    Task<IEnumerable<ExecutionDetailsDto>> GetDevicesIpStatusMapParallelAsync();

    IEnumerable<ExecutionDetailsDto> GetDevicesIpStatusMapParallelFor();

    IEnumerable<ExecutionDetailsDto> GetDevicesIpStatusMapByThreads();

    IEnumerable<ExecutionDetailsDto> GetDevicesIpStatusMapThreadPool();
}