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

namespace HardwareChangeEventTrigger
{
	static class Program
	{

		static String AppToRun = string.Empty;
		static DateTime lastSetTime;

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		static void Main(string[] args)
		{
			Log("Starting program");
			
			if(args.Length < 1)
			{
				Log("args = 0");
				return;
			}

			Log("Logging file path: ");
			Log(args[0]);

			if(File.Exists(Path.Combine(GetAppFolder(), args[0]) ))
			{
				Log("First file");
				AppToRun = Path.Combine(GetAppFolder(), args[0]);
			}
			else if(File.Exists(args[0]))
			{
				Log("Second file");
				AppToRun = args[0];
			}
			else
			{
				Log(args[0]);
				Log("No file");
				Log("Test");
				return;
			}
			lastSetTime = DateTime.MinValue;

			Service1 myService;
			ServiceBase[] ServicesToRun;
			ServicesToRun = new ServiceBase[] 
            { 
                myService = new Service1() 
            };

			myService.HardwareChangedEvent += myService_HardwareChangedEvent;
			ServiceBase.Run(ServicesToRun);
		}

		/// <summary>
		/// Get the path to where the program is located
		/// </summary>
		/// <returns></returns>
		static string GetAppFolder()
		{
			String exePath = System.Reflection.Assembly.GetEntryAssembly().Location;
			String exeFolder = Path.GetDirectoryName(exePath);
			return exeFolder;
		}

		/// <summary>
		/// Called when the myService fires event for hardware has changed. 
		/// </summary>
		static void myService_HardwareChangedEvent()
		{
			// WORKAROUND: The event is called a number of times, but we only want to set the wallpaper once, 
			// so here we check that it at least 3 seconds since the wallpaper was set the last time. 
			if (DateTime.Now > lastSetTime.AddSeconds(3))
			{
				lastSetTime = DateTime.Now;

				String processName = Path.GetFileNameWithoutExtension(AppToRun);
				Process[] processList = Process.GetProcessesByName(processName);
				if (processList.Length == 0)
				{
					ProcessExtensions.StartProcessAsCurrentUser(AppToRun);
				}
			}
		}

		public static void Log(string logMessage)
		{
			StreamWriter w = new StreamWriter(Path.Combine(GetAppFolder(), "Log.txt"), true);
			w.Write("\r\nLog Entry : ");
			w.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(),
				 DateTime.Now.ToLongDateString());
			w.WriteLine("  :");
			w.WriteLine("  :{0}", logMessage);
			w.WriteLine("-------------------------------");
			w.Close();
		}
	}
}
