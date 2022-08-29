using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Printing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using AsyncThreadsComparison;
using AsyncThreadsComparison.Dto;
using Microsoft.EntityFrameworkCore;
using NetworkSearchApproachesComparison.Models;
using NetworkSearchApproachesComparison.Services.NetworkDeviceService;

namespace NetworkSearchApproachesComparison.Services.NetworkService;

public class NetworkService : INetworkService
{
    private const byte MAX_NETWORK_DEVICES = 10;
    private readonly Network _network;
    private readonly INetworkDeviceService _networkDeviceService;
    private readonly object _lock = new object();
    private bool _lockTaken = false;
    private List<ExecutionDetailsDto> devicesStatusForLock = new List<ExecutionDetailsDto>();

    public NetworkService(Network network, INetworkDeviceService networkDeviceService)
    {
        _network = network;
        _networkDeviceService = networkDeviceService;
    }


    public byte[] GetNetworkBase(byte[] localhostIP)
    {
        return new byte[] { localhostIP[0], localhostIP[1], localhostIP[2], 0 };
    }

    public List<ExecutionDetailsDto> GetDevicesIpStatusMap()
    {
        var result = new List<ExecutionDetailsDto>();
        var networkBase = GetNetworkBase(_networkDeviceService.NetworkDevice.IpAddress.GetAddressBytes()); //GetLocalhostIP());

        for (byte i = 1; i <= MAX_NETWORK_DEVICES; i++)
        {
            networkBase[3] = i;
            IPAddress deviceIP = new IPAddress(networkBase);
            Ping p = new Ping();
            PingReply rep = p.Send(deviceIP);

            result.Add(new ExecutionDetailsDto { ProcessId = Environment.ProcessId, ManagedThreadId = Thread.CurrentThread.ManagedThreadId, IPAddress = deviceIP, IPStatus = rep.Status});
        }

        return result;
    }

    private Dictionary<IPAddress, IPStatus> PingDevice(MainWindow window, Dictionary<IPAddress, IPStatus> devicesStatus, byte[] networkBase,
        byte lastByte)
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

    private List<ExecutionDetailsDto> PingDeviceParalellTasks(List<ExecutionDetailsDto> devicesStatus, IPAddress deviceIP)
    {
        //byte[] nb = new byte[networkBase.Length];
        //networkBase.CopyTo(nb, 0);
        //nb[3] = lastByte;
        //IPAddress deviceIP = new IPAddress(nb);
        PingReply rep;

        using (var p = new Ping())
        {
            rep = p.Send(deviceIP);
        }

        devicesStatus.Add( new ExecutionDetailsDto { ProcessId = Environment.ProcessId, ManagedThreadId = Thread.CurrentThread.ManagedThreadId, IPAddress = deviceIP, IPStatus = rep.Status });

        //devicesStatus.TryAdd(deviceIP, rep.Status);

        return devicesStatus;
    }

    private List<ExecutionDetailsDto> PingDeviceParalellFor(List<ExecutionDetailsDto> result, IPAddress deviceIP)
    {
        PingReply rep;

        using (var p = new Ping())
        {
            rep = p.Send(deviceIP);
        }
        var item = new ExecutionDetailsDto { ProcessId = Environment.ProcessId, ManagedThreadId = Thread.CurrentThread.ManagedThreadId, IPAddress = deviceIP, IPStatus = rep.Status };

        result.Add(item);

        return result;
    }

    private ConcurrentDictionary<IPAddress, IPStatus> PingDevice(MainWindow window, ConcurrentDictionary<IPAddress, IPStatus> devicesStatus, byte[] networkBase,
    byte lastByte)
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

    private volatile int pingDeviceCounter;
    private volatile IPStatus[] devicesStatus;

    private void PingDevice(List<ExecutionDetailsDto> result, IPAddress deviceIP)
    {
        PingReply rep;

        using (var p = new Ping())
        {
            rep = p.Send(deviceIP);
        }

        //var deviceIpBytes = deviceIP.GetAddressBytes();
        //devicesStatus[deviceIpBytes[deviceIpBytes.Length-1] -1] = rep.Status;

        pingDeviceCounter = pingDeviceCounter - 1;

        result.Add(new ExecutionDetailsDto { ProcessId = Environment.ProcessId, ManagedThreadId = Thread.CurrentThread.ManagedThreadId, IPAddress = deviceIP, IPStatus = rep.Status });
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

        pingDeviceCounter = pingDeviceCounter - 1;

        return devicesStatus;
    }



    private async Task<List<ExecutionDetailsDto>> PingDeviceAsync(IPAddress deviceIP, List<ExecutionDetailsDto> devicesStatus)
    {
        PingReply rep;

        using (var p = new Ping())
        {
            rep = await p.SendPingAsync(deviceIP);
        }

        devicesStatus.Add(new ExecutionDetailsDto { ProcessId = Environment.ProcessId, ManagedThreadId = Thread.CurrentThread.ManagedThreadId, IPAddress = deviceIP, IPStatus = rep.Status });

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
            devicesStatusForLock.Add(new ExecutionDetailsDto { ProcessId = Environment.ProcessId, ManagedThreadId = Thread.CurrentThread.ManagedThreadId, IPAddress = deviceIP, IPStatus = rep.Status });
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
            devicesStatusForLock.Add(new ExecutionDetailsDto { ProcessId = Environment.ProcessId, ManagedThreadId = Thread.CurrentThread.ManagedThreadId, IPAddress = deviceIP, IPStatus = rep.Status });
        }
        finally
        {
            if (_lockTaken) System.Threading.Monitor.Exit(_lock);
        }

        return devicesStatusForLock;
    }

    private IPAddress CombineDeviceIP(byte[] networkBase, byte lastByte)
    {
        byte[] nb = new byte[networkBase.Length];
        networkBase.CopyTo(nb, 0);
        nb[3] = lastByte;
        return new IPAddress(nb);
    }

    public async Task<List<ExecutionDetailsDto>> GetDevicesIpStatusMapAsync()
    {

        var result = new List<ExecutionDetailsDto>();
        var tasks = new List<Task<List<ExecutionDetailsDto>>>();
        byte[] networkBase = GetNetworkBase(_networkDeviceService.NetworkDevice.IpAddress.GetAddressBytes());

        for (byte i = 1; i <= MAX_NETWORK_DEVICES; i++)
        {
            byte lastByte = i;
            await PingDeviceAsync(CombineDeviceIP(networkBase, lastByte), result);
        }

        return result;
    }

    public async Task<List<ExecutionDetailsDto>> GetDevicesIpStatusMapParallelAsync()
    {

        var result = new List<ExecutionDetailsDto>();
        var tasks = new List<Task<List<ExecutionDetailsDto>>>();
        byte[] networkBase = GetNetworkBase(_networkDeviceService.NetworkDevice.IpAddress.GetAddressBytes());

        for (byte i = 1; i <= MAX_NETWORK_DEVICES; i++)
        {
            byte lastByte = i;
            tasks.Add(Task.Run(() => PingDeviceParalellTasks(result, CombineDeviceIP(networkBase, (byte)lastByte))));
        }

        await Task.WhenAll(tasks);

        return result;
    }

    public List<ExecutionDetailsDto> GetDevicesIpStatusMapParallelFor()
    {

        var result = new List<ExecutionDetailsDto>();
        byte[] networkBase = GetNetworkBase(_networkDeviceService.NetworkDevice.IpAddress.GetAddressBytes());

        Parallel.For(1, MAX_NETWORK_DEVICES + 1, i => {
            //byte lastByte = i;
            PingDeviceParalellFor(result, CombineDeviceIP(networkBase, (byte)i));
        });

        return result;
    }

    public List<ExecutionDetailsDto> GetDevicesIpStatusMapByThreads()
    {
        var result = new List<ExecutionDetailsDto>();
        var threads = new List<Thread>();
        byte[] networkBase = GetNetworkBase(_networkDeviceService.NetworkDevice.IpAddress.GetAddressBytes());
        pingDeviceCounter = MAX_NETWORK_DEVICES;
        devicesStatus = new IPStatus[MAX_NETWORK_DEVICES];

        for (byte i = 1; i <= MAX_NETWORK_DEVICES; i++)
        {
            byte lastByte = i;
            var t = new Thread(() => PingDevice(result, CombineDeviceIP(networkBase, lastByte)));
            t.Start();
            threads.Add(t);
        }

        while (pingDeviceCounter > 1)
        {
            Thread.Sleep(100);
        }

        return result;
    }

    public List<ExecutionDetailsDto> GetDevicesIpStatusMapThreadPool()
    {
        var result = new List<ExecutionDetailsDto>();
        var tasks = new List<Task<List<ExecutionDetailsDto>>>();
        byte[] networkBase = GetNetworkBase(_networkDeviceService.NetworkDevice.IpAddress.GetAddressBytes());
        pingDeviceCounter = MAX_NETWORK_DEVICES;

        //byte i = 1;
        for (byte i = 1; i <= MAX_NETWORK_DEVICES; i++)
        {
            ThreadPool.QueueUserWorkItem((object state) => {
                byte lastByte = i;
                PingDevice(result, CombineDeviceIP(GetNetworkBase(_networkDeviceService.NetworkDevice.IpAddress.GetAddressBytes()), lastByte));
            });
        }
        while (pingDeviceCounter > 1)
        {
            Thread.Sleep(100);
        }

        return result;

        //void ThreadPoolCall(object state)
        //{
        //    byte  lastByte = i;
        //    PingDevice(result, CombineDeviceIP(GetNetworkBase(_networkDeviceService.NetworkDevice.IpAddress.GetAddressBytes()), lastByte));
        //}
    }

    public async Task<List<ExecutionDetailsDto>> GetDevicesIpStatusMapWithLockAsync()
    {
        var tasks = new List<Task>();
        byte[] networkBase = GetNetworkBase(_networkDeviceService.NetworkDevice.IpAddress.GetAddressBytes());

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
            byte[] networkBase = GetNetworkBase(_networkDeviceService.NetworkDevice.IpAddress.GetAddressBytes());

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

    public void PrintResult(MainWindow window, List<ExecutionDetailsDto> result) {
        //TODO: how correct sort list
        result.Sort();
        foreach (var item in result)
        {
            window.Output.Text += $"{Environment.NewLine}\t Process id {item.ProcessId}. Thread id {item.ManagedThreadId}: {item.IPAddress} {item.IPStatus}";
        }
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