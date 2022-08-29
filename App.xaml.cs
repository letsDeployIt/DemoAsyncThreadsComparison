using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using NetworkSearchApproachesComparison.Services.AppService;

namespace AsyncThreadsComparison
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            Init();
        }

        void Init()
        {
            new AppService().Run();
        }
    }
}