using AsyncAwaitBestPractices.MVVM;
using AsyncThreadsComparison.Dto;
using AsyncThreadsComparison.Services.NetworkService;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AsyncThreadsComparison
{
    public sealed class MainWindowViewModel : BaseViewModel
    {
        private readonly INetworkService _networkService = new NetworkService();

        public MainWindowViewModel()
        {
            ResetCommand = new CommandHandler(() => Text = "", () => true);
            SyncRunCommand = new CommandHandler(() => ReportResult("Starts synchronous scan of network.", "Synchronous scan ended with time elapsed", _networkService.GetDevicesIpStatusMap), () => true);
            AsyncRunCommand = new AsyncCommand(async () => await ReportResult("Starts asynchronous scan of network.", "Asynchronous scan ended with time elapsed", _networkService.GetDevicesIpStatusMapAsync));
            AsyncParallelRunCommand = new AsyncCommand(async () => await ReportResult("Starts asynchronous parallel scan of network.", "Asynchronous parallel scan ended with time elapsed", _networkService.GetDevicesIpStatusMapParallelAsync));
            ParallelForRunCommand = new CommandHandler(() => ReportResult("Starts lock with try finally scan of network.", "Lock with try finally scan ended with time elapsed", _networkService.GetDevicesIpStatusMapParallelFor), () => true);
            ThreadsRunCommand = new CommandHandler(() => ReportResult("Starts parallel threads scan of network.", "Threads parallel scan ended with time elapsed", _networkService.GetDevicesIpStatusMapByThreads), () => true);
            ThreadPoolRunCommand = new CommandHandler(() => ReportResult("Starts ThreadPool scan of network.",
                "ThreadPool scan ended with time elapsed",
                _networkService.GetDevicesIpStatusMapThreadPool), () => true);
        }

        private string _text;
        public string Text
        {
            get => _text;
            set => Set(ref _text, value);
        }


        #region commands

        public ICommand ResetCommand { get; }

        public ICommand SyncRunCommand { get; }

        public ICommand AsyncRunCommand { get; }

        public ICommand AsyncParallelRunCommand { get; }

        public ICommand ParallelForRunCommand { get; }

        public ICommand ThreadsRunCommand { get; }

        public ICommand ThreadPoolRunCommand { get; }

        #endregion commands


        private void ReportResult(string startsMsg, string endsMsg, Func<IEnumerable<ExecutionDetailsDto>> action)
        {
            Text += $"\r\n{DateTime.Now}  {startsMsg}\r\n";
            var stopwatch = Stopwatch.StartNew();

            IEnumerable<ExecutionDetailsDto> result = action();

            stopwatch.Stop();

            ReportResult(endsMsg, result, stopwatch.Elapsed);
        }

        private async Task ReportResult(string startsMsg, string endsMsg, Func<Task<IEnumerable<ExecutionDetailsDto>>> action)
        {
            Text += $"\r\n{DateTime.Now}  {startsMsg}\r\n";
            var stopwatch = Stopwatch.StartNew();

            IEnumerable<ExecutionDetailsDto> result = await action();

            stopwatch.Stop();

            ReportResult(endsMsg, result, stopwatch.Elapsed);
        }

        private void ReportResult(string endsMsg, IEnumerable<ExecutionDetailsDto> result, TimeSpan elapsed)
        {
            var sb = new StringBuilder();
            ExecutionDetailsDto[] sortList = result.ToArray();//.OrderBy(x => x.IPAddress.Address).ToArray();

            foreach (ExecutionDetailsDto item in sortList)
            {
                sb.Append($"\t ThreadId {item.ManagedThreadId}: {item.IPAddress} {item.IPStatus}\r\n");
            }

            sb.AppendLine($"{DateTime.Now}  {endsMsg} {elapsed}{Environment.NewLine}");

            Text += sb.ToString();
        }
    }
}