using System;
using System.Text;
using System.Runtime.InteropServices;

namespace MapperTools.Pollicino
{
    class PlatformSpecificCode
    {
#if PocketPC || Smartphone || WindowsCE
        [DllImport("coredll")]
        extern public static void SystemIdleTimerReset();
#else
        static public void SystemIdleTimerReset() 
        {}
#endif
        [DllImport("CoreDll.DLL", EntryPoint = "SetPowerRequirement", SetLastError = true)]
        extern public static IntPtr SetPowerRequirement(String pvDevice, int DeviceState, int DeviceFlags, IntPtr pvSystemState, int StateFlags);

        public static event EventHandler Hibernate
        {
            add
            {
                Microsoft.WindowsCE.Forms.MobileDevice.Hibernate += value;
            }
            remove
            {
                Microsoft.WindowsCE.Forms.MobileDevice.Hibernate += value;
            }
        }
    }
}
