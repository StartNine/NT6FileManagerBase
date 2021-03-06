﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsSharp.DiskItems;

namespace NT6FileManagerBase
{
    public static class Manager
    {
        /*public static bool ShowItemCheckboxes
        {
            get => (bool)GetValue(ShowItemCheckboxesProperty);
            set => SetValue(ShowItemCheckboxesProperty, value);
        }

        public static readonly DependencyProperty ShowItemCheckboxesProperty = DependencyProperty.Register("ShowItemCheckboxes", typeof(bool), typeof(Manager), new PropertyMetadata(false));*/
        public static bool ShowItemCheckboxes { get; set; } = false;

        static string _favoritesPath = Environment.ExpandEnvironmentVariables(@"%appdata%\Start9\TempData\" + Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly().Location) + "_Favorites.txt");

        public static ObservableCollection<DiskItem> Favorites
        {
            get
            {
                ObservableCollection<DiskItem> items = new ObservableCollection<DiskItem>();
                //string favoritesPath = Environment.ExpandEnvironmentVariables(@"%appdata%\Start9\TempData\RibbonFileManager_Favorites.txt");

                if (!File.Exists(_favoritesPath))
                {
                    string dir = Path.GetDirectoryName(_favoritesPath);

                    if (!Directory.Exists(dir))
                        Directory.CreateDirectory(dir);

                    File.WriteAllLines(_favoritesPath, new List<string>()
                    {
                        @"%userprofile%\Documents",
                        @"%userprofile%\Pictures",
                        @"%userprofile%\Downloads"
                    }.ToArray());
                }

                var lines = File.ReadAllLines(_favoritesPath);
                foreach (string l in lines)
                {
                    if (l.Contains('?'))
                    {
                        var parts = l.Split('?');
                        //foreach (string s in parts)
                        for (int i = 0; i < parts.Count(); i++)
                        {
                            string s = parts[i];
                            Debug.WriteLine("parts[" + i + "]: " + s);
                        }
                        items.Add(new DiskItem(parts[0])
                        {
                            ItemDisplayName = parts[1]
                        });
                    }
                    else
                        items.Add(new DiskItem(l));
                }
                return items;
            }
            set
            {
                List<string> paths = new List<string>();
                //string favoritesPath = Environment.ExpandEnvironmentVariables(@"%appdata%\Start9\TempData\RibbonFileManager_Favorites.txt");

                foreach (DiskItem d in value)
                {
                    paths.Add(d.ItemPath);
                }
                File.WriteAllLines(_favoritesPath, paths.ToArray());
            }
            /*get;
            set;*/
        }// = new ObservableCollection<DiskItem>();

        public static ObservableCollection<DiskItem> ComputerSubfolders
        {
            get
            {
                var collection = new ObservableCollection<DiskItem>();
                var _userFolders = new ObservableCollection<String>() { @"%userprofile%\Desktop", @"%userprofile%\Documents", @"%userprofile%\Downloads", @"%userprofile%\Music", @"%userprofile%\Pictures", @"%userprofile%\Videos" };
                foreach (var s in _userFolders)
                    collection.Add(new DiskItem(Environment.ExpandEnvironmentVariables(s)));

                foreach (var s in System.IO.DriveInfo.GetDrives())
                {
                    collection.Add(new DiskItem(Environment.ExpandEnvironmentVariables(s.Name)));
                }
                return collection;
            }
            set
            {
            }
        }

        public static ObservableCollection<DiskItem> ClipboardContents { get; set; } = new ObservableCollection<DiskItem>();
        public static Boolean Cut { get; set; } = false;

        public static ObservableCollection<DiskItem> CopyTo(String targetPath)
        {
            var success = new ObservableCollection<DiskItem>();
            foreach (var d in ClipboardContents)
            {
                try
                {
                    var baseFilename = targetPath + @"\" + Path.GetFileName(d.ItemPath);
                    var outFilename = baseFilename;

                    var interval = 1;

                    var ext = Path.GetExtension(baseFilename);
                    while ((File.Exists(outFilename)) | (Directory.Exists(outFilename)))
                    {
                        outFilename = baseFilename + " (" + interval.ToString() + ")";
                        if (!(String.IsNullOrEmpty(ext)))
                        {
                            if (!(outFilename.EndsWith(ext)))
                            {
                                if (outFilename.Contains(ext))
                                {
                                    outFilename = outFilename.Replace(ext, "");
                                    outFilename = outFilename + ext;
                                }
                            }
                        }
                        interval += 1;
                    }
                    Debug.WriteLine("outFilename: " + outFilename);

                    if (Cut)
                    {
                        if (File.Exists(d.ItemPath))
                        {
                            File.Move(d.ItemPath, outFilename);
                        }
                        else
                        {
                            Directory.Move(d.ItemPath, outFilename);
                        }
                    }
                    else
                    {
                        if (File.Exists(d.ItemPath))
                        {
                            File.Copy(d.ItemPath, outFilename);
                        }
                        else
                        {
                            CopyDirectory(d.ItemPath, outFilename, true);
                        }
                    }
                    success.Add(new DiskItem(outFilename));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }
            return success;
        }

        //https://docs.microsoft.com/en-us/dotnet/standard/io/how-to-copy-directories
        static void CopyDirectory(String sourceDirName, String destDirName, Boolean copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            var dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            var dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (var file in files)
            {
                var temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (var subdir in dirs)
                {
                    var temppath = Path.Combine(destDirName, subdir.Name);
                    CopyDirectory(subdir.FullName, temppath, copySubDirs);
                }
            }
        }
    }
}
