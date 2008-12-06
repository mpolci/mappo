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
    }
}
