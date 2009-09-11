/*******************************************************************************
 *  MyTracks - A tool for gps mapping.
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

namespace MapsLibrary
{
    class Tools
    {
        public static TEXTMETRICS GetTextMetrics(System.Drawing.Font font)
        {
            IntPtr hDC = GetDC(IntPtr.Zero); //Screen DC
            IntPtr hFont = font.ToHfont();
            IntPtr hObjOld = SelectObject(hDC, hFont);
            TEXTMETRICS tm = new TEXTMETRICS();
            GetTextMetrics(hDC, ref tm);
            SelectObject(hDC, hObjOld);
            DeleteObject(hFont);
            ReleaseDC(IntPtr.Zero, hDC);
            return tm;
        }

        [DllImport("coredll")]
        private extern static IntPtr GetDC(IntPtr hWnd);

        [DllImport("coredll")]
        private extern static IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("coredll")]
        private extern static IntPtr SelectObject(IntPtr hDC, IntPtr hObj);

        [DllImport("coredll")]
        private extern static IntPtr DeleteObject(IntPtr hObject);

        [DllImport("coredll", CharSet = CharSet.Unicode)]
        private extern static int GetTextMetrics(IntPtr hDC, ref TEXTMETRICS tm);
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct TEXTMETRICS
    {
        public int tmHeight;
        public int tmAscent;
        public int tmDescent;
        public int tmInternalLeading;
        public int tmExternalLeading;
        public int tmAveCharWidth;
        public int tmMaxCharWidth;
        public int tmWeight;
        public int tmOverhang;
        public int tmDigitizedAspectX;
        public int tmDigitizedAspectY;
        public byte tmFirstChar;
        public byte tmLastChar;
        public byte tmDefaultChar;
        public byte tmBreakChar;
        public byte tmItalic;
        public byte tmUnderlined;
        public byte tmStruckOut;
        public byte tmPitchAndFamily;
        public byte tmCharSet;
    } 
}
