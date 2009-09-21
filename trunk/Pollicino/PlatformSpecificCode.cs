using System;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.WindowsMobile.Status;

namespace MapperTools.Pollicino
{
    class PlatformSpecificCode
    {
#if PocketPC || Smartphone || WindowsCE
        [DllImport("coredll")]
        extern public static void SystemIdleTimerReset();

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

        public static bool IsNetworkAvailable
        {
            get
            {
                // TODO: la rete potrebbe essere disponibile anche senza copertura GPRS, ad esempio via WIFI o ActiveSync
                return !SystemState.PhoneRoaming && SystemState.PhoneGprsCoverage;
            }
        }

        //TODO: creare un evento per il cambiamento di stato della disponibilità della rete, potrebbe servire durante il download delle mappe
#else
        public static void SystemIdleTimerReset() 
        {}

        public static IntPtr SetPowerRequirement(String pvDevice, int DeviceState, int DeviceFlags, IntPtr pvSystemState, int StateFlags)
        {}

        public static event EventHandler Hibernate;
        
        public static bool IsNetworkAvailable()
        {
            get
            {
                return true;
            }
        }
#endif

    }
}
