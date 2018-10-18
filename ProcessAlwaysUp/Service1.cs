using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Configuration;

namespace ProcessAlwaysUp
{
    public partial class Service1 : ServiceBase
    {
        public List<string> process_name_list_before = new List<string>();
        public List<string> process_name_list_after = new List<string>();
        public List<string> app_process_name = new List<string>();
        public Configuration config;
        public string appName;
        public string appPath;
        public string appProcess;
        public string RestartTime;

        public Service1()
        {
            InitializeComponent();
             config = System.Configuration.ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
             appName = config.AppSettings.Settings["appName"].Value;
             appPath = config.AppSettings.Settings["appPath"].Value;
             appProcess = config.AppSettings.Settings["appProcess"].Value;
             RestartTime = config.AppSettings.Settings["RestartTime"].Value;
        }

        protected override void OnStart(string[] args)
        {
            GetProcessNameList(appProcess);
            Thread threadB = new Thread(ManageProcess);
            threadB.Name = "ManageProcess";
            threadB.Start();            
        }

        /// <summary>
        /// 配置守护进程列表
        /// </summary>
        /// <param name="appprocess"></param>
        private void GetProcessNameList(string appprocess)
        {
            if (appprocess == "")
            {
                foreach (Process ps in Process.GetProcesses())
                {
                    process_name_list_before.Add(ps.ProcessName);
                }



                //Interop.CreateProcess(appName , @appPath);
                Process.Start(appPath + appName);

                foreach (Process ps in Process.GetProcesses())
                {
                    if (!process_name_list_before.Contains(ps.ProcessName))
                    {
                        app_process_name.Add(ps.ProcessName);
                    }
                }
                foreach (string str in app_process_name)
                {
                    appprocess += str + ";";
                }

                config.AppSettings.Settings["appProcess"].Value = appprocess;
                config.Save();
            }
            else
            {
                string[] appnamelist = appprocess.Split(';');
                foreach (string str in appnamelist)
                {
                    if (str != "")
                    {
                        app_process_name.Add(str);
                    }
                }
            }
        }

        private void ManageProcess()
        {
            while (true)
            {
                bool IsStarted = false;
                process_name_list_after.Clear();
                foreach (Process ps in Process.GetProcesses())
                {                    
                    process_name_list_after.Add(ps.ProcessName);
                }
                foreach (string str in app_process_name)
                {
                    if (!process_name_list_after.Contains(str) && !IsStarted)
                    {

                        //Interop.CreateProcess(appName, @appPath); 
                        Process.Start(appPath+appName);
                        IsStarted = true;
                    }
                }
                Thread.Sleep(Convert.ToInt32(RestartTime));
            }
        }

        protected override void OnStop()
        {
            //config.AppSettings.Settings["appProcess"].Value = "";
            //config.Save();
            //System.Environment.Exit(0); 
        }
    }
}
