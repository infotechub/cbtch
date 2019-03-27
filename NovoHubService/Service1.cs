using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace NovoHubService
{
    public partial class Service1 : ServiceBase
    {

        public static bool Exit = false;
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {

            while(Exit)
            {
                
            }
        }

        protected override void OnStop()
        {

            Exit = true;
        }
    }
}
