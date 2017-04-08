using System;
using System.IO;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using System.Management;

namespace HardwareChangeEventTrigger
{
	public partial class Service1 : ServiceBase
	{
		#region Fields

		private IntPtr deviceNotifyHandle;
		private IntPtr deviceEventHandle;
		private IntPtr directoryHandle;
		private Win32.ServiceControlHandlerEx myCallback;

		public delegate void HardwareChangedEventHandler();
		public event HardwareChangedEventHandler HardwareChangedEvent;

		#endregion

		private void OnHardwareChangedEvent()
		{
			if(HardwareChangedEvent != null)
			{
				HardwareChangedEvent();
			}
		}

		private int ServiceControlHandler(int control, int eventType, IntPtr eventData, IntPtr context)
		{
			if (control == Win32.SERVICE_CONTROL_STOP || control == Win32.SERVICE_CONTROL_SHUTDOWN)
			{
				Win32.UnregisterDeviceNotification(deviceEventHandle);

				base.Stop();
			}
			else if (control == Win32.SERVICE_CONTROL_DEVICEEVENT)
			{
				switch (eventType)
				{
					case Win32.DBT_DEVICEARRIVAL:
						Win32.DEV_BROADCAST_HDR hdr;
						hdr = (Win32.DEV_BROADCAST_HDR)
							Marshal.PtrToStructure(eventData, typeof(Win32.DEV_BROADCAST_HDR));
						

						if (hdr.dbcc_devicetype == Win32.DBT_DEVTYP_DEVICEINTERFACE)
						{
							//ManagementObjectSearcher searcher = new ManagementObjectSearcher("select * from " + "Win32_DesktopMonitor");

							//string searchGUID;
							//foreach (ManagementObject share in searcher.Get())
							//{
							//	if (share["DeviceID"] != null)
							//	{
							//		searchGUID = share["UniqueId"].ToString();
							//	}
							//	foreach (PropertyData PC in share.Properties)
							//	{
							//		int debug = 0;
							//		//some codes ...

							//	}
							//}

							OnHardwareChangedEvent();
							//Win32.DEV_BROADCAST_DEVICEINTERFACE deviceInterface;
							//deviceInterface = (Win32.DEV_BROADCAST_DEVICEINTERFACE)
							//	Marshal.PtrToStructure(eventData, typeof(Win32.DEV_BROADCAST_DEVICEINTERFACE));
							//string name = new string(deviceInterface.dbcc_name);
							//string guid = System.Text.Encoding.Default.GetString(deviceInterface.dbcc_classguid);
							//int type = deviceInterface.dbcc_devicetype;

							// name = name.Substring(0, name.IndexOf('\0')) + "\\";

							//StringBuilder stringBuilder = new StringBuilder();
							//Win32.GetVolumeNameForVolumeMountPoint(name, stringBuilder, 100);

							//uint stringReturnLength = 0;
							//string driveLetter = "";

							//Win32.GetVolumePathNamesForVolumeNameW(stringBuilder.ToString(), driveLetter, (uint) driveLetter.Length, ref stringReturnLength);
							//if (stringReturnLength == 0)
							//{
							//	// TODO handle error
							//}

							//driveLetter = new string(new char[stringReturnLength]);

							//if (!Win32.GetVolumePathNamesForVolumeNameW(stringBuilder.ToString(), driveLetter, stringReturnLength, ref stringReturnLength))
							//{
							//	// TODO handle error
							//}

							//RegisterForHandle(driveLetter[0]);

							//fileSystemWatcher.Path = driveLetter[0] + ":\\";
							//fileSystemWatcher.EnableRaisingEvents = true;
						}
						break;
					case Win32.DBT_DEVICEQUERYREMOVE:

						break;
				}
			}

			return 0;
		}


		private void RegisterDeviceNotification()
		{
			myCallback = new Win32.ServiceControlHandlerEx(ServiceControlHandler);
			Win32.RegisterServiceCtrlHandlerEx(this.ServiceName, myCallback, IntPtr.Zero);

			if (this.ServiceHandle == IntPtr.Zero)
			{
				// TODO handle error
			}

			Win32.DEV_BROADCAST_DEVICEINTERFACE deviceInterface = new Win32.DEV_BROADCAST_DEVICEINTERFACE();
			int size = Marshal.SizeOf(deviceInterface);
			deviceInterface.dbcc_size = size;
			deviceInterface.dbcc_devicetype = Win32.DBT_DEVTYP_DEVICEINTERFACE;
			IntPtr buffer = default(IntPtr);
			buffer = Marshal.AllocHGlobal(size);
			Marshal.StructureToPtr(deviceInterface, buffer, true);
			deviceEventHandle = Win32.RegisterDeviceNotification(this.ServiceHandle, buffer, Win32.DEVICE_NOTIFY_SERVICE_HANDLE | Win32.DEVICE_NOTIFY_ALL_INTERFACE_CLASSES);
			if (deviceEventHandle == IntPtr.Zero)
			{
				// TODO handle error
			}
		}

		
		public Service1()
		{
			//InitializeComponent();
		}



		#region ServiceBase Implementation

		protected override void OnStart(string[] args)
		{
			base.OnStart(args);

			RegisterDeviceNotification();
		}

		#endregion
	}

	public class Win32
	{
		public const int DEVICE_NOTIFY_SERVICE_HANDLE = 1;
		public const int DEVICE_NOTIFY_ALL_INTERFACE_CLASSES = 4;

		public const int SERVICE_CONTROL_STOP = 1;
		public const int SERVICE_CONTROL_DEVICEEVENT = 11;
		public const int SERVICE_CONTROL_SHUTDOWN = 5;

		public const uint GENERIC_READ = 0x80000000;
		public const uint OPEN_EXISTING = 3;
		public const uint FILE_SHARE_READ = 1;
		public const uint FILE_SHARE_WRITE = 2;
		public const uint FILE_SHARE_DELETE = 4;
		public const uint FILE_ATTRIBUTE_NORMAL = 128;
		public const uint FILE_FLAG_BACKUP_SEMANTICS = 0x02000000;
		public static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

		public const int DBT_DEVTYP_DEVICEINTERFACE = 5;
		public const int DBT_DEVTYP_HANDLE = 6;

		public const int DBT_DEVICEARRIVAL = 0x8000;
		public const int DBT_DEVICEQUERYREMOVE = 0x8001;
		public const int DBT_DEVICEREMOVECOMPLETE = 0x8004;

		public const int WM_DEVICECHANGE = 0x219;

		public delegate int ServiceControlHandlerEx(int control, int eventType, IntPtr eventData, IntPtr context);

		[DllImport("advapi32.dll", SetLastError = true)]
		public static extern IntPtr RegisterServiceCtrlHandlerEx(string lpServiceName, ServiceControlHandlerEx cbex, IntPtr context);

		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool GetVolumePathNamesForVolumeNameW(
				[MarshalAs(UnmanagedType.LPWStr)]
					string lpszVolumeName,
				[MarshalAs(UnmanagedType.LPWStr)]
					string lpszVolumePathNames,
				uint cchBuferLength,
				ref UInt32 lpcchReturnLength);

		[DllImport("kernel32.dll")]
		public static extern bool GetVolumeNameForVolumeMountPoint(string
		   lpszVolumeMountPoint, [Out] StringBuilder lpszVolumeName,
		   uint cchBufferLength);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern IntPtr RegisterDeviceNotification(IntPtr IntPtr, IntPtr NotificationFilter, Int32 Flags);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern uint UnregisterDeviceNotification(IntPtr hHandle);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern IntPtr CreateFile(
			  string FileName,                    // file name
			  uint DesiredAccess,                 // access mode
			  uint ShareMode,                     // share mode
			  uint SecurityAttributes,            // Security Attributes
			  uint CreationDisposition,           // how to create
			  uint FlagsAndAttributes,            // file attributes
			  int hTemplateFile                   // handle to template file
			  );

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool CloseHandle(IntPtr hObject);

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		public struct DEV_BROADCAST_DEVICEINTERFACE
		{
			public int dbcc_size;
			public int dbcc_devicetype;
			public int dbcc_reserved;
			[MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 16)]
			public byte[] dbcc_classguid;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
			public char[] dbcc_name;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct DEV_BROADCAST_HDR
		{
			public int dbcc_size;
			public int dbcc_devicetype;
			public int dbcc_reserved;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct DEV_BROADCAST_HANDLE
		{
			public int dbch_size;
			public int dbch_devicetype;
			public int dbch_reserved;
			public IntPtr dbch_handle;
			public IntPtr dbch_hdevnotify;
			public Guid dbch_eventguid;
			public long dbch_nameoffset;
			public byte dbch_data;
			public byte dbch_data1;
		}
	}
}
