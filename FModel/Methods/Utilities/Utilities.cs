﻿using FModel.Properties;
using System;
using System.Linq;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Windows.Forms;
using System.Collections.Generic;

namespace FModel
{
    static class Utilities
    {
        /// <summary>
        /// OpenWithDefaultProgramAndNoFocus is used to automatically open sound file once they are converted
        /// </summary>
        /// <param name="path"> path is the path to the converted sound file (in "Sounds" subfolder) </param>
        public static void OpenWithDefaultProgramAndNoFocus(string path)
        {
            Process fileopener = new Process();
            fileopener.StartInfo.FileName = "explorer";
            fileopener.StartInfo.Arguments = "\"" + path + "\"";
            fileopener.Start();
        }

        /// <summary>
        /// just create the default folders FModel needs to work
        /// </summary>
        public static void CreateDefaultFolders()
        {
            Directory.CreateDirectory(App.DefaultOutputPath + "\\Backup\\");
            Directory.CreateDirectory(App.DefaultOutputPath + "\\Extracted\\");
            Directory.CreateDirectory(App.DefaultOutputPath + "\\Icons\\");
            Directory.CreateDirectory(App.DefaultOutputPath + "\\Sounds\\");
            Directory.CreateDirectory(App.DefaultOutputPath + "\\Saved_JSON\\");
        }

        /// <summary>
        /// actually idk if it's useful, i added this for Admin windows account, when FModel can't even use CreateDefaultFolders 
        /// </summary>
        /// <param name="folderPath"></param>
        public static void SetFolderPermission(string folderPath)
        {
            var directoryInfo = new DirectoryInfo(folderPath);
            var directorySecurity = directoryInfo.GetAccessControl();
            var currentUserIdentity = WindowsIdentity.GetCurrent();
            var fileSystemRule = new FileSystemAccessRule(currentUserIdentity.Name,
                                                          FileSystemRights.Read,
                                                          InheritanceFlags.ObjectInherit |
                                                          InheritanceFlags.ContainerInherit,
                                                          PropagationFlags.None,
                                                          AccessControlType.Allow);

            directorySecurity.AddAccessRule(fileSystemRule);
            directoryInfo.SetAccessControl(directorySecurity);
        }

        /// <summary>
        /// By default the output folder is the Documents folder, however if user wanna change, once FModel restart SetOutputFolder is called
        /// </summary>
        public static void SetOutputFolder()
        {
            App.DefaultOutputPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\FModel"; //DOCUMENTS FOLDER BY DEFAULT

            if (string.IsNullOrEmpty(Settings.Default.ExtractOutput))
            {
                Settings.Default.ExtractOutput = App.DefaultOutputPath;
                Settings.Default.Save();
            }
            else
            {
                App.DefaultOutputPath = Settings.Default.ExtractOutput;
            }

            Directory.CreateDirectory(App.DefaultOutputPath);
        }

        /// <summary>
        /// 
        /// </summary>
        public static void JohnWickCheck()
        {
            if (File.Exists(App.DefaultOutputPath + "\\john-wick-parse-modded.exe")) //2.0-
            {
                File.Delete(App.DefaultOutputPath + "\\john-wick-parse-modded.exe");
            }
            if (File.Exists(App.DefaultOutputPath + "\\john-wick-parse_custom.exe")) //2.0+
            {
                File.Delete(App.DefaultOutputPath + "\\john-wick-parse_custom.exe");
            }
            //this will stay for 1 update
            if (Directory.Exists(App.DefaultOutputPath + "\\LocRes\\"))
            {
                Directory.Delete(App.DefaultOutputPath + "\\LocRes\\");
            }
            if (Directory.Exists(App.DefaultOutputPath + "\\Saved JSON\\"))
            {
                Directory.Delete(App.DefaultOutputPath + "\\Saved JSON\\");
            }
        }

        /// <summary>
        /// this should tell me if i can read the file, to avoid crash when a pak is being written by the launcher
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;
            try
            {
                stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
        }

        /// <summary>
        /// filter text with case insensitive support
        /// </summary>
        /// <param name="text"></param>
        /// <param name="value"></param>
        /// <param name="stringComparison"></param>
        /// <returns></returns>
        public static bool CaseInsensitiveContains(string text, string value, StringComparison stringComparison = StringComparison.CurrentCultureIgnoreCase)
        {
            return text.IndexOf(value, stringComparison) >= 0;
        }

        /// <summary>
        /// in the ToolStripMenuItem, color the paks that have a key (working or not working)
        /// result: improve readability when there's a lot of paks
        /// </summary>
        /// <param name="myToolStrip"></param>
        public static void colorMyPaks(ToolStripMenuItem myToolStrip)
        {
            foreach (ToolStripItem toolstripmenuitem in myToolStrip.DropDownItems)
            {
                toolstripmenuitem.BackColor = Color.FromArgb(255, 255, 255, 255);

                if (!string.IsNullOrEmpty(Settings.Default.AESKey))
                {
                    string pak = ThePak.mainPaksList.Where(i => i.thePak == toolstripmenuitem.Text).Select(i => i.thePak).FirstOrDefault();

                    if (pak == toolstripmenuitem.Text)
                    {
                        toolstripmenuitem.BackColor = Color.FromArgb(50, 50, 92, 219);
                    }
                }
                if (DynamicKeysManager.AESEntries != null)
                {
                    string pak = DynamicKeysManager.AESEntries.Where(i => i.thePak == toolstripmenuitem.Text).Select(i => i.thePak).FirstOrDefault();
                    string key = DynamicKeysManager.AESEntries.Where(i => i.thePak == toolstripmenuitem.Text).Select(i => i.theKey).FirstOrDefault();

                    if (!string.IsNullOrEmpty(key) && pak == toolstripmenuitem.Text)
                    {
                        toolstripmenuitem.BackColor = Color.FromArgb(50, 50, 92, 219);
                    }
                }
            }
        }
    }
}
