using Microsoft.Win32;
using System;
using System.Linq;

namespace DocumentTools.Services
{
    /// <summary>
    /// 注册表操作服务：封装注册表的读写操作
    /// </summary>
    public class RegistryService : IRegistryService
    {
        public object GetRegistryValue(RegistryHive registryHive, string registryPath, string registryName)
        {
            object tempValue = null;
            RegistryKey tempRootRegistryKey;
            
            if (Environment.Is64BitOperatingSystem)
            {
                tempRootRegistryKey = RegistryKey.OpenBaseKey(registryHive, RegistryView.Registry64);
            }
            else
            {
                tempRootRegistryKey = RegistryKey.OpenBaseKey(registryHive, RegistryView.Registry32);
            }
            
            using (tempRootRegistryKey)
            {
                RegistryKey tempSubKey = tempRootRegistryKey.OpenSubKey(registryPath);
                if (tempSubKey != null)
                {
                    using (tempSubKey)
                    {
                        tempValue = tempSubKey.GetValue(registryName);
                    }
                }
            }
            
            return tempValue;
        }

        public void WriteRegistryValue(
            RegistryHive registryHive,
            string registryPath,
            string registryName,
            object registryValue,
            RegistryValueKind valueKind)
        {
            RegistryKey tempRootRegistryKey;
            if (Environment.Is64BitOperatingSystem)
            {
                tempRootRegistryKey = RegistryKey.OpenBaseKey(registryHive, RegistryView.Registry64);
            }
            else
            {
                tempRootRegistryKey = RegistryKey.OpenBaseKey(registryHive, RegistryView.Registry32);
            }

            using (tempRootRegistryKey)
            {
                tempRootRegistryKey.CreateSubKey(registryPath, true);
                RegistryKey tempSubKey = tempRootRegistryKey.OpenSubKey(registryPath, true);

                if (tempSubKey != null)
                {
                    using (tempSubKey)
                    {
                        tempSubKey.SetValue(registryName, registryValue, valueKind);
                    }
                }
            }
        }

        public void DeleteRegistryKey(RegistryHive registryHive, string registryPath, string deleteName)
        {
            RegistryKey tempRootRegistryKey;
            if (Environment.Is64BitOperatingSystem)
            {
                tempRootRegistryKey = RegistryKey.OpenBaseKey(registryHive, RegistryView.Registry64);
            }
            else
            {
                tempRootRegistryKey = RegistryKey.OpenBaseKey(registryHive, RegistryView.Registry32);
            }

            using (tempRootRegistryKey)
            {
                RegistryKey tempSubKey = tempRootRegistryKey.OpenSubKey(registryPath, true);
                if (tempSubKey != null)
                {
                    using (tempSubKey)
                    {
                        if (tempSubKey.GetValueNames().Contains(deleteName))
                        {
                            tempSubKey.DeleteValue(deleteName);
                        }
                    }
                }
            }
        }
    }

    public interface IRegistryService
    {
        object GetRegistryValue(RegistryHive registryHive, string registryPath, string registryName);
        void WriteRegistryValue(RegistryHive registryHive, string registryPath, string registryName, object registryValue, RegistryValueKind valueKind);
        void DeleteRegistryKey(RegistryHive registryHive, string registryPath, string deleteName);
    }
}
