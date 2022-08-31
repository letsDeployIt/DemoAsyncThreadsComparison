using AsyncThreadsComparison.Dto;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncThreadsComparison.Services.NetworkService;

public class NetworkService : INetworkService
{
    private const byte MAX_NETWORK_DEVICES = byte.MaxValue;
    private const int TIMEOUT = 10000;

    private readonly object _lock = new object();
    private readonly List<ExecutionDetailsDto> devicesStatusForLock = new List<ExecutionDetailsDto>();

    private bool _lockTaken;

    private static byte[] GetNetworkBase()
    {
        //var host = Dns.GetHostEntry(Dns.GetHostName());
        //foreach (var ip in host.AddressList)
        //{
        //    if (ip.AddressFamily == AddressFamily.InterNetwork)
        //    {
        //        var aaaa = ip.ToString();
        //    }
        //}

        return new byte[] { 192, 168, 3, 0 };//{ localhostIP[0], localhostIP[1], localhostIP[2], 0 };
    }

    private static IPAddress CombineDeviceIP(byte[] networkBase, byte lastByte)
    {
        byte[] nb = new byte[networkBase.Length];
        networkBase.CopyTo(nb, 0);
        nb[3] = lastByte;

        return new IPAddress(nb);
    }


    #region INetworkService

    public IEnumerable<ExecutionDetailsDto> GetDevicesIpStatusMap()
    {
        var result = new List<ExecutionDetailsDto>();
        byte[] networkBase = GetNetworkBase();
        using var p = new Ping();

        for (byte i = 1; i < MAX_NETWORK_DEVICES; i++)
        {
            IPAddress ip = CombineDeviceIP(networkBase, i);

            PingReply rep = p.Send(ip, TIMEOUT);

            var executionDetailsDto = new ExecutionDetailsDto
            {
                ManagedThreadId = Thread.CurrentThread.ManagedThreadId,
                IPAddress = ip,
                IPStatus = rep.Status
            };

            result.Add(executionDetailsDto);
        }

        return result;
    }

    public async Task<IEnumerable<ExecutionDetailsDto>> GetDevicesIpStatusMapAsync()
    {
        var result = new List<ExecutionDetailsDto>();
        byte[] networkBase = GetNetworkBase();

        using var p = new Ping();

        for (byte i = 0; i < MAX_NETWORK_DEVICES; i++)
        {
            IPAddress ip = CombineDeviceIP(networkBase, i);

            PingReply rep = await p.SendPingAsync(ip, TIMEOUT);

            var dto = new ExecutionDetailsDto
            {
                ManagedThreadId = Thread.CurrentThread.ManagedThreadId,
                IPAddress = ip,
                IPStatus = rep.Status
            };

            result.Add(dto);
        }

        return result;
    }


    public async Task<IEnumerable<ExecutionDetailsDto>> GetDevicesIpStatusMapParallelAsync()
    {
        var result = new List<ExecutionDetailsDto>();
        var tasks = new List<Task<List<ExecutionDetailsDto>>>();
        byte[] networkBase = GetNetworkBase();

        for (byte i = 1; i < MAX_NETWORK_DEVICES; i++)
        {
            IPAddress ip = CombineDeviceIP(networkBase, i);

            Task<List<ExecutionDetailsDto>> item = Task.Run(() => PingDeviceParallelTasks(result, ip));

            tasks.Add(item);
        }

        await Task.WhenAll(tasks);

        return result;
    }

    private List<ExecutionDetailsDto> PingDeviceParallelTasks(List<ExecutionDetailsDto> result, IPAddress ip)
    {
        PingReply rep;

        using (var p = new Ping())
        {
            rep = p.Send(ip, TIMEOUT);
        }

        var dto = new ExecutionDetailsDto
        {
            ManagedThreadId = Thread.CurrentThread.ManagedThreadId,
            IPAddress = ip,
            IPStatus = rep.Status
        };

        result.Add(dto);

        return result;
    }


    public IEnumerable<ExecutionDetailsDto> GetDevicesIpStatusMapParallelFor()
    {
        var result = new ConcurrentBag<ExecutionDetailsDto>();
        byte[] networkBase = GetNetworkBase();

        Parallel.For(1, MAX_NETWORK_DEVICES, i => PingDeviceParallelFor(result, networkBase, i));

        return result;
    }

    private void PingDeviceParallelFor(ConcurrentBag<ExecutionDetailsDto> result, byte[] networkBase, int i)
    {
        IPAddress ip = CombineDeviceIP(networkBase, (byte)i);
        PingReply rep;

        using (var p = new Ping())
        {
            rep = p.Send(ip, TIMEOUT);
        }

        var item = new ExecutionDetailsDto
        {
            ManagedThreadId = Thread.CurrentThread.ManagedThreadId,
            IPAddress = ip,
            IPStatus = rep.Status
        };

        result.Add(item);
    }

    private volatile int _t_pingDeviceCounter;
    private ExecutionDetailsDto[] _t_result;
    private readonly byte[] _t_networkBase = GetNetworkBase();

    public IEnumerable<ExecutionDetailsDto> GetDevicesIpStatusMapByThreads()
    {
        _t_pingDeviceCounter = 0;
        _t_result = new ExecutionDetailsDto[MAX_NETWORK_DEVICES + 1];

        for (byte i = 1; i < MAX_NETWORK_DEVICES; i++)
        {
            var t = new Thread(PingDevice)
            {
                IsBackground = true,
                Priority = ThreadPriority.Lowest
            };
            t.Start(i);
        }

        const byte maxNetworkDevices = MAX_NETWORK_DEVICES - 1;

        while (_t_pingDeviceCounter < maxNetworkDevices)
        {
            Thread.Sleep(100);
        }

        return _t_result.Where(x => x != null);
    }

    public IEnumerable<ExecutionDetailsDto> GetDevicesIpStatusMapThreadPool()
    {
        _t_pingDeviceCounter = 0;
        _t_result = new ExecutionDetailsDto[MAX_NETWORK_DEVICES + 1];

        for (byte i = 1; i < MAX_NETWORK_DEVICES; i++)
        {
            ThreadPool.QueueUserWorkItem(PingDevice, i);
        }

        while (ThreadPool.PendingWorkItemCount > 0)
        {
            Thread.Sleep(100);
        }

        return _t_result.Where(x => x != null);
    }

    private void PingDevice(object parameter)
    {
        var i = (byte)parameter;
        IPAddress ip = CombineDeviceIP(_t_networkBase, i);
        PingReply rep = null;

        try
        {
            using (var p = new Ping())
            {
                rep = p.Send(ip, TIMEOUT);
            }
        }
        catch (Exception exception)
        {
        }

        var dto = new ExecutionDetailsDto
        {
            ManagedThreadId = Thread.CurrentThread.ManagedThreadId,
            IPAddress = ip,
            IPStatus = rep?.Status ?? IPStatus.IcmpError
        };

        _t_result[i] = dto;

        lock (_lock)
        {
            _t_pingDeviceCounter = _t_pingDeviceCounter + 1;
        }
    }


    #endregion INetworkService



    private Dictionary<IPAddress, IPStatus> PingDevice(MainWindow window, Dictionary<IPAddress, IPStatus> devicesStatus, byte[] networkBase, byte lastByte)
    {
        networkBase[3] = lastByte;
        IPAddress deviceIP = new IPAddress(networkBase);
        PingReply rep;

        using (var p = new Ping())
        {
            rep = p.Send(deviceIP);
        }

        window.Dispatcher.Invoke(() =>
        {
            window.Output.Text += $"{Environment.NewLine}\t Process id {Environment.ProcessId}. Thread id {Thread.CurrentThread.ManagedThreadId}: {deviceIP} {rep.Status}";
        });


        devicesStatus.Add(deviceIP, rep.Status);

        return devicesStatus;
    }

    private ConcurrentDictionary<IPAddress, IPStatus> PingDevice(MainWindow window, ConcurrentDictionary<IPAddress, IPStatus> devicesStatus, byte[] networkBase, byte lastByte)
    {
        networkBase[3] = lastByte;
        IPAddress deviceIP = new IPAddress(networkBase);
        PingReply rep;

        using (var p = new Ping())
        {
            rep = p.Send(deviceIP);
        }

        window.Dispatcher.Invoke(() =>
        {
            window.Output.Text += $"{Environment.NewLine}\t Process id {Environment.ProcessId}. Thread id {Thread.CurrentThread.ManagedThreadId}: {deviceIP} {rep.Status}";
        });


        devicesStatus.TryAdd(deviceIP, rep.Status);

        return devicesStatus;
    }

    private ConcurrentDictionary<IPAddress, IPStatus> PingDevice(MainWindow window, IPAddress deviceIP, ConcurrentDictionary<IPAddress, IPStatus> devicesStatus)
    {
        PingReply rep;

        using (var p = new Ping())
        {
            rep = p.Send(deviceIP);
        }
        //this.disp
        //Thread.CurrentThread.Dispatcher
        //window.Dispatcher.InvokeAsync(() =>
        //{
        //    window.Output.Text += $"{Environment.NewLine}\t Process id {Environment.ProcessId}. Thread id {Thread.CurrentThread.ManagedThreadId}: {deviceIP} {rep.Status}";
        //});
        devicesStatus.TryAdd(deviceIP, rep.Status);

        _t_pingDeviceCounter = _t_pingDeviceCounter - 1;

        return devicesStatus;
    }

    private List<ExecutionDetailsDto> PingDeviceWithLock(IPAddress deviceIP)
    {
        PingReply rep;

        using (var p = new Ping())
        {
            rep = p.Send(deviceIP);
        }

        lock (_lock)
        {
            devicesStatusForLock.Add(new ExecutionDetailsDto { ManagedThreadId = Thread.CurrentThread.ManagedThreadId, IPAddress = deviceIP, IPStatus = rep.Status });
        }

        return devicesStatusForLock;
    }

    private List<ExecutionDetailsDto> PingDeviceWithLockTryFinally(MainWindow window, IPAddress deviceIP)
    {
        PingReply rep;

        using (var p = new Ping())
        {
            rep = p.Send(deviceIP);
        }

        try
        {
            System.Threading.Monitor.Enter(_lock, ref _lockTaken);
            devicesStatusForLock.Add(new ExecutionDetailsDto { ManagedThreadId = Thread.CurrentThread.ManagedThreadId, IPAddress = deviceIP, IPStatus = rep.Status });
        }
        finally
        {
            if (_lockTaken) System.Threading.Monitor.Exit(_lock);
        }

        return devicesStatusForLock;
    }

    public async Task<List<ExecutionDetailsDto>> GetDevicesIpStatusMapWithLockAsync()
    {
        var tasks = new List<Task>();
        byte[] networkBase = GetNetworkBase();

        for (byte i = 1; i <= MAX_NETWORK_DEVICES; i++)
        {
            byte lastByte = i;
            var t = Task.Run(() => PingDeviceWithLock(CombineDeviceIP(networkBase, lastByte)));
            tasks.Add(t);
        }

        await Task.WhenAll(tasks);

        return devicesStatusForLock;
    }

    public async Task<List<ExecutionDetailsDto>> GetDevicesIpStatusMapWithLockTryFinallyAsync()
    {
        try
        {
            System.Threading.Monitor.Enter(_lock, ref _lockTaken);
            var tasks = new List<Task>();
            byte[] networkBase = GetNetworkBase();

            for (byte i = 1; i <= MAX_NETWORK_DEVICES; i++)
            {
                byte lastByte = i;
                var t = Task.Run(() => PingDeviceWithLock(CombineDeviceIP(networkBase, lastByte)));
                tasks.Add(t);
            }

            await Task.WhenAll(tasks);
        }
        finally
        {
            if (_lockTaken) System.Threading.Monitor.Exit(_lock);
        }

        //Thread.CurrentThread.back
        return devicesStatusForLock;
    }

    //threads
    //threadpool
    //workers
    //lock
    //try finally monitore 
    //array
    //privete static volotile thredadCount-- TODO: volotile - purpose is not clear for me
    //thread main sleep 100 miliseconds till while count > 0
    //counter 10 
    //ever thred has access to array
    //TODO: foreground back ground threads how to detect, and how to set isBackground?
    //TODO: Monitor, Semaphore, Mutex, Dispatcher, Process, Thread
    //TODO: is Mutex kinda service (Worker)?
    //TODO: how to access to code of dll
}