using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace MapperTools.Pollicino
{
    class PlatformSpecificCode
    {
#if PocketPC
        [DllImport("coredll")]
        extern public static void SystemIdleTimerReset();
#else
        static void SystemIdleTimerReset() 
        {}
#endif
    }
}
