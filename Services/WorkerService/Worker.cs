﻿using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NetworkSearchApproachesComparison.Models;
using NetworkSearchApproachesComparison.Services.NetworkService;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncThreadsComparison.Services.WorkerService
{
    internal class Worker : BackgroundService
    {
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly ILogger<Worker> _logger;
        private NetworkService _networkService = new NetworkSearchApproachesComparison.Services.NetworkService.NetworkService(new Network(), new NetworkSearchApproachesComparison.Services.NetworkDeviceService.NetworkDeviceService(new NetworkDevice()));
        private readonly MainWindow _mainWindow;

        public Worker(MainWindow mainWindow) {
            //TODO: what is host application and how to use it
            //_hostApplicationLifetime = _mainWindow;
            _mainWindow = mainWindow;
        }
        public Worker(
            IHostApplicationLifetime hostApplicationLifetime, ILogger<Worker> logger) =>
            (_hostApplicationLifetime, _logger) = (hostApplicationLifetime, logger);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //TODO: is it correct using of method

            _mainWindow.Output.Text += $"Starts worker async scan of network.";
            var stopwatch = Stopwatch.StartNew();
            var execRes = await _networkService.GetDevicesIpStatusMapAsync();
            execRes.Sort();
            foreach (var item in execRes)
            {
                _mainWindow.Output.Text += $"{Environment.NewLine}\t Process id {item.ProcessId}. Thread id {item.ManagedThreadId}: {item.IPAddress} {item.IPStatus}";
            }
            stopwatch.Stop();
            _mainWindow.Output.Text += $"{Environment.NewLine}Worker async scan ended with time elapsed {stopwatch.Elapsed}";
            _mainWindow.Output.Text += $"{Environment.NewLine}";
            //_hostApplicationLifetime.StopApplication();
        }
    }
}
