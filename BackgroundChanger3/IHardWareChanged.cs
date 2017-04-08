using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackgroundChanger3
{
	public delegate void HardwareChangedEventHandler();

	public interface IHardWareChanged
	{
		event HardwareChangedEventHandler HardwareChangedEvent;
	}
}
