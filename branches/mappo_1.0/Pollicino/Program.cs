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

using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;



namespace MapperTools.Pollicino
{

    static class Program
    {
        internal static string GetPath()
        {
            Uri furl = new Uri(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            System.Diagnostics.Debug.Assert(furl.IsFile, "CodeBase returns not file uri");
            return Path.GetDirectoryName(furl.LocalPath);
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [MTAThread]
        static void Main()
        {
   
#if TRACE
            string path = Program.GetPath();
            System.Diagnostics.TextWriterTraceListener outtw = new System.Diagnostics.TextWriterTraceListener(path + "\\pollicino_log.txt");
            System.Diagnostics.Trace.Listeners.Add(outtw);
            //System.Diagnostics.Debug.Listeners.Clear();
            //System.Diagnostics.Debug.Listeners.Add(outtw);
            //System.Diagnostics.Trace.WriteLine(DateTime.Now.ToString() + " - Starting");
#endif
            try
            {
                Application.Run(new Form_MapperToolMain());
            }
            catch (Exception e)
            {
#if TRACE
                System.Diagnostics.Trace.Write("------------------------" + DateTime.Now.ToString());
                System.Diagnostics.Trace.Write(e.ToString());
#endif
                throw e;
            }
            finally
            {
#if TRACE
                System.Diagnostics.Trace.Close();
                outtw.Dispose();
#endif
            }
        }
    }
}