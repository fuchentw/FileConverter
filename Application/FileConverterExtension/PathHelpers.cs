// <copyright file="FileConverterExtension.cs" company="AAllard">License: http://www.gnu.org/licenses/gpl.html GPL version 3.</copyright>

namespace FileConverterExtension
{
    using System.IO;
    using System;
    using Microsoft.Win32;

    public static class PathHelpers
    {
        private static RegistryKey fileConverterRegistryKey;
        private static string fileConverterPath;

        public static string UserSettingsFilePath => Path.Combine(PathHelpers.GetUserDataFolderPath, "Settings.user.xml");

        public static string DefaultSettingsFilePath
        {
            get
            {
                // First check adjacent to the current executing application / domain directory.
                string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                if (!string.IsNullOrEmpty(baseDirectory))
                {
                    string localDefaultSettings = Path.Combine(baseDirectory, "Settings.default.xml");
                    if (File.Exists(localDefaultSettings))
                    {
                        return localDefaultSettings;
                    }
                }

                string pathToFileConverterExecutable = PathHelpers.FileConverterPath;
                if (!string.IsNullOrEmpty(pathToFileConverterExecutable))
                {
                    string directory = Path.GetDirectoryName(pathToFileConverterExecutable);
                    if (!string.IsNullOrEmpty(directory))
                    {
                        string registryDefaultSettings = Path.Combine(directory, "Settings.default.xml");
                        if (File.Exists(registryDefaultSettings))
                        {
                            return registryDefaultSettings;
                        }
                    }
                }

                return null;
            }
        }

        public static RegistryKey FileConverterRegistryKey
        {
            get
            {
                if (PathHelpers.fileConverterRegistryKey == null)
                {
                    try
                    {
                        PathHelpers.fileConverterRegistryKey = Registry.CurrentUser.OpenSubKey(@"Software\FileConverter");
                    }
                    catch
                    {
                        PathHelpers.fileConverterRegistryKey = null;
                    }
                }

                return PathHelpers.fileConverterRegistryKey;
            }
        }

        public static string FileConverterPath
        {
            get
            {
                if (string.IsNullOrEmpty(PathHelpers.fileConverterPath))
                {
                    try
                    {
                        PathHelpers.fileConverterPath = PathHelpers.FileConverterRegistryKey?.GetValue("Path") as string;
                    }
                    catch
                    {
                        PathHelpers.fileConverterPath = null;
                    }

                    if (string.IsNullOrEmpty(PathHelpers.fileConverterPath))
                    {
                        string fallbackExe = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FileConverter.exe");
                        if (File.Exists(fallbackExe))
                        {
                            PathHelpers.fileConverterPath = fallbackExe;
                        }
                    }
                }

                return PathHelpers.fileConverterPath;
            }
        }

        public static string GetUserDataFolderPath
        {
            get
            {
                string path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
                path = Path.Combine(path, "FileConverter");

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                return path;
            }
        }
    }
}
