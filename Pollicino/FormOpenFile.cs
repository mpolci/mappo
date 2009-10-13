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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace MapperTools.Pollicino
{
    public partial class FormOpenFile : Form
    {
        private bool dirselection;
        private string selectedfile;
        public string openfile
        {
            get
            {
                return selectedfile;
            }
        }
        public string directoty
        {
            get
            {
                return tb_Dir.Text;
            }
            set
            {
                if (Directory.Exists(value))
                {
                    tb_Dir.Text = (new DirectoryInfo(value)).FullName;
                    Fill_DirContents();
                }
                else
                    throw new DirectoryNotFoundException("Directory \"" + value + "\" not found.");
            }
        }

        /// <param name="dir">Directory di partenza</param>
        /// <param name="selectdir">True per selezionare una directory, false per selezionare un file</param>
        public FormOpenFile(string dir, bool selectdir)
        {
            InitializeComponent();
            tb_Dir.Text = dir;
            this.dirselection = selectdir;
            if (!selectdir)
                mainMenu.MenuItems.Remove(menuItem_selectdir);
            Fill_DirContents();
        }

        private static int CompareDirectoryInfoByName(DirectoryInfo a, DirectoryInfo b)
        {
            return a.Name.CompareTo(b.Name);
        }

        private static int CompareFileInfoByName(FileInfo a, FileInfo b)
        {
            return a.Name.CompareTo(b.Name);
        }

        private void Fill_DirContents()
        {
            string currentdir = tb_Dir.Text;
            if (Directory.Exists(currentdir))
            {
                DirectoryInfo cdirinfo = new DirectoryInfo(currentdir);
                lw_dircontent.Clear();
                if (cdirinfo.Parent != null)
                {
                    ListViewItem item = new ListViewItem("");
                    item.ImageIndex = 2;
                    lw_dircontent.Items.Add(item);
                }
                DirectoryInfo[] subdirs = cdirinfo.GetDirectories();
                Array.Sort<DirectoryInfo>(subdirs, CompareDirectoryInfoByName);
                foreach (DirectoryInfo d in subdirs)
                {
                    ListViewItem item = new ListViewItem(d.Name);
                    item.ImageIndex = 0;
                    lw_dircontent.Items.Add(item);
                }
                if (!dirselection)
                {
                    FileInfo[] files = cdirinfo.GetFiles();
                    Array.Sort<FileInfo>(files, CompareFileInfoByName);
                    foreach (FileInfo f in files)
                    {
                        ListViewItem item = new ListViewItem(f.Name);
                        item.ImageIndex = 1;
                        lw_dircontent.Items.Add(item);
                    }
                }
            }


        }

        private void lw_dircontent_ItemActivate(object sender, EventArgs e)
        {
            ListViewItem item = lw_dircontent.Items[lw_dircontent.SelectedIndices[0]];
            string newpath = tb_Dir.Text;
            if (!newpath.EndsWith("\\")) newpath += '\\';
            switch (item.ImageIndex) {
                case 2: // parent dir
                    tb_Dir.Text = (new DirectoryInfo(tb_Dir.Text)).Parent.FullName;
                    Fill_DirContents();
                    break;
                case 0: // dir
                    tb_Dir.Text = newpath + item.Text; 
                    Fill_DirContents();
                    break;
                case 1: // file
                    this.selectedfile = newpath + item.Text;
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                    break;
            }
        }

        private void menuItem_cancel_Click(object sender, EventArgs e)
        {
            selectedfile = null;
            tb_Dir.Text = null;
            this.Close();
        }

        private void menuItem_selectdir_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            Close();
        }
        
    }
}