/*******************************************************************************
 *  Mappo! - A tool for gps mapping.
 *  Copyright (C) 2008  Marco Polci
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see http://www.gnu.org/licenses/gpl.html.
 * 
 ******************************************************************************/

using System;
using System.Text;
using System.Runtime.InteropServices;
#if PocketPC || Smartphone || WindowsCE
using Microsoft.WindowsMobile.Status;
#endif

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

        [DllImport("coredll.dll", SetLastError = true)]
        static extern int SetSystemPowerState(string psState, int StateFlags, int Options);
        /*
        //#define POWER_STATE_ON           (DWORD)(0x00010000)        // on state
        //#define POWER_STATE_OFF          (DWORD)(0x00020000)        // no power, full off
        //#define POWER_STATE_CRITICAL     (DWORD)(0x00040000)        // critical off
        //#define POWER_STATE_BOOT         (DWORD)(0x00080000)        // boot state
        //#define POWER_STATE_IDLE         (DWORD)(0x00100000)        // idle state
        //#define POWER_STATE_SUSPEND      (DWORD)(0x00200000)        // suspend state
        //#define POWER_STATE_UNATTENDED   (DWORD)(0x00400000)        // Unattended state.
        //#define POWER_STATE_RESET        (DWORD)(0x00800000)        // reset state
        //#define POWER_STATE_USERIDLE     (DWORD)(0x01000000)        // user idle state
        //#define POWER_STATE_PASSWORD     (DWORD)(0x10000000)        // This state is password protected.
         */
        const int POWER_STATE_ON = 0x00010000;
        const int POWER_STATE_OFF = 0x00020000;
        const int POWER_STATE_SUSPEND = 0x00200000;
        const int POWER_STATE_IDLE = 0x00100000;
        const int POWER_STATE_USERIDLE = 0x01000000;
        const int POWER_STATE_RESET = 0x00800000;
        const int POWER_FORCE = 4096;

        public static int PowerForceDisplayOn()
        {
            return SetSystemPowerState(null, POWER_STATE_ON, POWER_FORCE);
        }

        public static int PowerForceDisplayOff()
        {
            return SetSystemPowerState(null, POWER_STATE_IDLE, POWER_FORCE);
        }

#else
        public static void SystemIdleTimerReset() 
        {}

        public static IntPtr SetPowerRequirement(String pvDevice, int DeviceState, int DeviceFlags, IntPtr pvSystemState, int StateFlags)
        {
            return IntPtr.Zero;
        }

        public static event EventHandler Hibernate;
        
        public static bool IsNetworkAvailable
        {
            get
            {
                return true;
            }
        }
#endif

    }
}
