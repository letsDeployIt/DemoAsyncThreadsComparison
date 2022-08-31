using AsyncThreadsComparison.Dto;
using AsyncThreadsComparison.Services.NetworkService;
using AsyncThreadsComparison.Services.WorkerService;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Windows;

namespace AsyncThreadsComparison
{
    public sealed partial class MainWindow : Window
    {
        public MainWindowViewModel ViewModel => DataContext as MainWindowViewModel;

        private readonly NetworkService _networkService = new NetworkService();

        public MainWindow()
        {
            InitializeComponent();


            SameIpAddressWithLockRun.Command = new CommandHandler(async () =>
            {
                //TODO: is it correct.
                var stopwatch = Stopwatch.StartNew();
                ReportResult("Starts lock scan of network.",
                    "Lock scan ended with time elapsed",
                    await _networkService.GetDevicesIpStatusMapWithLockAsync(), stopwatch);
            }, () => true);

            SameIpAddressWithTryFinallyLockRun.Command = new CommandHandler(async () =>
            {
                //TODO: is it correct.
                var stopwatch = Stopwatch.StartNew();
                ReportResult("Starts lock with try finally scan of network.",
                    "Lock with try finally scan ended with time elapsed",
                    await _networkService.GetDevicesIpStatusMapWithLockTryFinallyAsync(), stopwatch);
            }, () => true);




            WorkersRun.Command = new CommandHandler(() =>
            {
                //TODO: is it correct
                var worker = new Worker(this);
                worker.StartAsync(new CancellationToken());
            }, () => true);

        }

        //view model description / definition, handlers low level command (catch pointer events what is not binding to controls)

        //TODO: how correct send funtion or delegate to get consistent output
        public void ReportResult(string startsMsg, string endsMsg, /*Func<, */List<ExecutionDetailsDto>/*>*/ result, Stopwatch stopwatch)
        {
            Output.Text += startsMsg;
            stopwatch.Stop();
            Output.Text += $"{Environment.NewLine}{endsMsg} {stopwatch.Elapsed}";
            Output.Text += $"{Environment.NewLine}";
        }
    }
}