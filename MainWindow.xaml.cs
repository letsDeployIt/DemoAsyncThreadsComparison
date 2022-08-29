using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AsyncThreadsComparison.Dto;
using AsyncThreadsComparison.Services.WorkerService;
using Microsoft.Extensions.Logging;
using NetworkSearchApproachesComparison.Models;
using NetworkSearchApproachesComparison.Services.AppService;
using NetworkSearchApproachesComparison.Services.NetworkService;

namespace AsyncThreadsComparison
{
    /// <summary>
    /// Interaction logic for xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private NetworkService _networkService = new NetworkSearchApproachesComparison.Services.NetworkService.NetworkService(new Network(), new NetworkSearchApproachesComparison.Services.NetworkDeviceService.NetworkDeviceService(new NetworkDevice()));
        public MainWindow()
        {
            InitializeComponent();

            SyncRun.Command = new CommandHandler(() => {
                var stopwatch = Stopwatch.StartNew();
                ReportResult("Starts synchronous scan of network.",
                    "Synchronous scan ended with time elapsed",
                    _networkService.GetDevicesIpStatusMap(), stopwatch);
                //Output.Text = "Starts synchronous scan of network.";
                //var stopwatch = Stopwatch.StartNew();
                //var res = _networkService.GetDevicesIpStatusMap();
                //_networkService.PrintResult(this, res);
                //stopwatch.Stop();
                //Output.Text += $"{Environment.NewLine}Synchronous scan ended with time elapsed {stopwatch.Elapsed}";
                //Output.Text += $"{Environment.NewLine}";
            }, () => true);

            AsyncRun.Command = new CommandHandler(async () => {
                //TODO: does it work properly
                var stopwatch = Stopwatch.StartNew();
                ReportResult("Starts asynchronous scan of network.",
                    "Asynchronous scan ended with time elapsed",
                    await _networkService.GetDevicesIpStatusMapAsync(), stopwatch);
                //Output.Text += $"Starts asynchronous scan of network.";
                //var stopwatch = Stopwatch.StartNew();
                //var res = await _networkService.GetDevicesIpStatusMapAsync();
                //_networkService.PrintResult(this, res);
                //stopwatch.Stop();
                //Output.Text += $"{Environment.NewLine}Asynchronous scan ended with time elapsed {stopwatch.Elapsed}";
                //Output.Text += $"{Environment.NewLine}";
            }, () => true);

            AsyncParallelRun.Command = new CommandHandler(async () => {
                //TODO: does it work properly
                var stopwatch = Stopwatch.StartNew();
                ReportResult("Starts asynchronous parallel scan of network.",
                    "Asynchronous parallel scan ended with time elapsed",
                    await _networkService.GetDevicesIpStatusMapParallelAsync(), stopwatch);
                //Output.Text += $"Starts asynchronous parallel scan of network.";
                //var stopwatch = Stopwatch.StartNew();
                //var res = await _networkService.GetDevicesIpStatusMapParallelAsync();
                //_networkService.PrintResult(this, res);
                //stopwatch.Stop();
                //Output.Text += $"{Environment.NewLine}Asynchronous parallel scan ended with time elapsed {stopwatch.Elapsed}";
                //Output.Text += $"{Environment.NewLine}";
            }, () => true);

            ParallelForRun.Command = new CommandHandler(() => {
                var stopwatch = Stopwatch.StartNew();
                ReportResult("Starts lock with try finally scan of network.",
                    "Lock with try finally scan ended with time elapsed",
                    _networkService.GetDevicesIpStatusMapParallelFor(), stopwatch);
                //Output.Text += $"Starts lock with try finally scan of network.";
                //var stopwatch = Stopwatch.StartNew();
                //var res = _networkService.GetDevicesIpStatusMapParallelFor();
                //_networkService.PrintResult(this, res);
                //stopwatch.Stop();
                //Output.Text += $"{Environment.NewLine}Lock with try finally scan ended with time elapsed {stopwatch.Elapsed}";
                //Output.Text += $"{Environment.NewLine}";
            }, () => true);

            ThreadsRun.Command = new CommandHandler(() => {
                //TODO: is it correct. one thread on each thread and not successively
                var stopwatch = Stopwatch.StartNew();
                ReportResult("Starts parallel threads scan of network.",
                    "Threads parallel scan ended with time elapsed",
                    _networkService.GetDevicesIpStatusMapByThreads(), stopwatch);
                //Output.Text += $"Starts parallel threads scan of network.";
                //var stopwatch = Stopwatch.StartNew();
                //var res = _networkService.GetDevicesIpStatusMapByThreads();
                //_networkService.PrintResult(this, res);
                //stopwatch.Stop();
                //Output.Text += $"{Environment.NewLine}Threads parallel scan ended with time elapsed {stopwatch.Elapsed}";
                //Output.Text += $"{Environment.NewLine}";
            }, () => true);

            WorkersRun.Command = new CommandHandler(() => {
                //TODO: is it correct
                var worker = new Worker(this);
                worker.StartAsync(new CancellationToken());
            }, () => true);

            ThreadPoolRun.Command = new CommandHandler(() => {
                //TODO: is it correct
                var stopwatch = Stopwatch.StartNew();
                ReportResult("Starts ThreadPool scan of network.",
                    "ThreadPool scan ended with time elapsed",
                    _networkService.GetDevicesIpStatusMapThreadPool(), stopwatch);
                //Output.Text += $"Starts ThreadPool scan of network.";
                //var stopwatch = Stopwatch.StartNew();
                //var res = _networkService.GetDevicesIpStatusMapThreadPool();
                //_networkService.PrintResult(this, res);
                //stopwatch.Stop();
                //Output.Text += $"{Environment.NewLine}ThreadPool scan ended with time elapsed {stopwatch.Elapsed}";
                //Output.Text += $"{Environment.NewLine}";
            }, () => true);

            SameIpAddressWithLockRun.Command = new CommandHandler(async () => {
                //TODO: is it correct.
                var stopwatch = Stopwatch.StartNew();
                ReportResult("Starts lock scan of network.",
                    "Lock scan ended with time elapsed",
                    await _networkService.GetDevicesIpStatusMapWithLockAsync(), stopwatch);
                //Output.Text += $"Starts lock scan of network.";
                //var stopwatch = Stopwatch.StartNew();
                //var res = await _networkService.GetDevicesIpStatusMapWithLockAsync();
                //_networkService.PrintResult(this, res);
                //stopwatch.Stop();
                //Output.Text += $"{Environment.NewLine}Lock scan ended with time elapsed {stopwatch.Elapsed}";
                //Output.Text += $"{Environment.NewLine}";
            }, () => true);

            SameIpAddressWithTryFinallyLockRun.Command = new CommandHandler(async () => {
                //TODO: is it correct.
                var stopwatch = Stopwatch.StartNew();
                ReportResult("Starts lock with try finally scan of network.",
                    "Lock with try finally scan ended with time elapsed",
                    await _networkService.GetDevicesIpStatusMapWithLockTryFinallyAsync(), stopwatch);
                //Output.Text += $"Starts lock with try finally scan of network.";
                //var stopwatch = Stopwatch.StartNew();
                //var res = await _networkService.GetDevicesIpStatusMapWithLockTryFinallyAsync();
                //_networkService.PrintResult(this, res);
                //stopwatch.Stop();
                //Output.Text += $"{Environment.NewLine}Lock with try finally scan ended with time elapsed {stopwatch.Elapsed}";
                //Output.Text += $"{Environment.NewLine}";
            }, () => true);

            Reset.Command = new CommandHandler(() =>
            {
                try
                {
                    Output.Text = "";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }, ()=> true);
        }

        //view model description / definition, handlers low level command (catch pointer events what is not binding to controls)

        //TODO: how correct send funtion or delegate to get consistent output
        public void ReportResult(string startsMsg, string endsMsg, /*Func<, */List<ExecutionDetailsDto>/*>*/ result, Stopwatch stopwatch)
        {
            Output.Text += startsMsg;
            _networkService.PrintResult(this, result);
            stopwatch.Stop();
            Output.Text += $"{Environment.NewLine}{endsMsg} {stopwatch.Elapsed}";
            Output.Text += $"{Environment.NewLine}";
        }
    }


}