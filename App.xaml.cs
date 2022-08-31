using AsyncThreadsComparison.Services.AppService;
using System.Windows;

namespace AsyncThreadsComparison
{
    public partial class App : Application
    {
        public App()
        {
            new AppService().Run();
        }
    }
}