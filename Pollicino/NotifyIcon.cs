/*******************************************************************************
 *  Pollicino - A tool for gps mapping.
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
using System.Drawing;
using System.Runtime.InteropServices;

namespace MapperTools.Pollicino
{
    class NotifyIcon : Microsoft.WindowsCE.Forms.MessageWindow, IDisposable
    {
        public event System.EventHandler Click;

        private Icon _icon;
        private uint _uid = 17324;

        public NotifyIcon(Icon ico)
        {
            _icon = ico;

            NOTIFYICONDATA notdata = new NOTIFYICONDATA();

            notdata.cbSize = 152;
            notdata.hIcon = _icon.Handle;
            notdata.hWnd = this.Hwnd;
            notdata.uCallbackMessage = WM_USER_NOTIFY_TRAY;
            notdata.uFlags = NIF_MESSAGE | NIF_ICON;
            notdata.uID = _uid;

            int res = Shell_NotifyIcon(NIM_ADD, ref notdata);
            if (res == 0)
                throw new Exception("Impossibile registrare l'icona nella tray area");
        }

        #region IDisposable Members

        public new void Dispose()
        {
            NOTIFYICONDATA notdata = new NOTIFYICONDATA();

            notdata.cbSize = 152;
            notdata.hIcon = IntPtr.Zero;
            notdata.hWnd = this.Hwnd;
            notdata.uCallbackMessage = WM_USER_NOTIFY_TRAY;
            notdata.uFlags = NIF_MESSAGE | NIF_ICON;
            notdata.uID = _uid;

            int res = Shell_NotifyIcon(NIM_DELETE, ref notdata);
            base.Dispose();
            if (res == 0)
                throw new Exception("Impossibile rimuovere l'icona dalla tray area");
        }

        #endregion

        protected override void WndProc(ref Microsoft.WindowsCE.Forms.Message m)
        {
            switch (m.Msg ) 
            {
                case WM_USER_NOTIFY_TRAY:
                    if ((int)m.LParam == WM_LBUTTONDOWN && (int)m.WParam == _uid)
                    {
                        if (Click != null)
                            Click(this, null);
                    }
                    break;
            }
            base.WndProc(ref m);
        }

        #region Native code

        internal const int WM_LBUTTONDOWN = 0x0201;
        //User defined message
        internal const int WM_USER_NOTIFY_TRAY = 0x0400 + 2001;

        internal const int NIM_ADD = 0x00000000;
        internal const int NIM_MODIFY = 0x00000001;
        internal const int NIM_DELETE = 0x00000002;

        const int NIF_MESSAGE = 0x00000001;
        const int NIF_ICON = 0x00000002;


        internal struct NOTIFYICONDATA
        {
            internal int cbSize;
            internal IntPtr hWnd;
            internal uint uID;
            internal uint uFlags;
            internal uint uCallbackMessage;
            internal IntPtr hIcon;
            //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
            //internal char[] szTip; 
        }

#if PocketPC

        [DllImport("coredll.dll")]
        internal static extern int Shell_NotifyIcon(int dwMessage, ref NOTIFYICONDATA pnid);

#else
        internal static int Shell_NotifyIcon(int dwMessage, ref NOTIFYICONDATA pnid);
        {}
#endif

        #endregion

    }
}
