using murrayju.ProcessExtensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using WallpaperChanger;

namespace BackgroundChanger3
{
	static class Program
	{
		
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		static void Main()
		{
			Service1 myService;
			ServiceBase[] ServicesToRun;
			ServicesToRun = new ServiceBase[] 
            { 
                myService = new Service1() 
            };

			myService.HardwareChangedEvent += myService_HardwareChangedEvent;
			ServiceBase.Run(ServicesToRun);
		}

		static void myService_HardwareChangedEvent()
		{
			Process[] processList = Process.GetProcessesByName("WallpaperChanger.exe");
			if (processList.Length == 0)
			{
				String exePath = System.Reflection.Assembly.GetEntryAssembly().Location;
				String exeFolder = Path.GetDirectoryName(exePath);
				String appPath = Path.Combine(exeFolder, "WallpaperChanger.exe");
				ProcessExtensions.StartProcessAsCurrentUser(appPath);
			}			
		}
	}
}
