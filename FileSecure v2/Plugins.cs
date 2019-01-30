using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FileSecure_v2
{
    class Registery
    {
        public static void LoadMainDB()
        {
            using (RegistryKey LocalDB = Registry.CurrentUser.OpenSubKey(@"Software\FileSecure"))
            {
                if(LocalDB == null)
                    Registry.CurrentUser.CreateSubKey(@"Software\FileSecure");
            }
        }
        public static string Read(string KeyName)
        {
            using (RegistryKey LocalDB = Registry.CurrentUser.OpenSubKey(@"Software\FileSecure"))
            {
                if(LocalDB != null)
                {
                    try
                    {
                        return (string)LocalDB.GetValue(KeyName);
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                } else
                {
                    return null;
                }
            }
        }
        public static bool Write(string KeyName, object KeyData)
        {
            using (RegistryKey LocalDB = Registry.CurrentUser.OpenSubKey(@"Software\FileSecure",true))
                if (LocalDB != null)
                {
                    try
                    {
                        LocalDB.SetValue(KeyName, KeyData);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        return false;
                    }
                } else
                    return false;
        }
    }
    class Generator
    {
        public static string GetIP()
        {
            using (WebClient client = new WebClient())
            {
                try
                {
                    return client.DownloadString("https://api.ipify.org/");
                } catch(Exception)
                {
                    return "131.241.123.123";
                }

            }
        }
        public static int CheckIP(string IP)
        {
            using (WebClient client = new WebClient())
            {
                string result = null;
                try
                {
                   result = client.DownloadString("http://check.getipintel.net/check.php?ip=" + IP + "&contact=hideyouremailbot@gmail.com");
                } catch(Exception)
                {
                    result = "0";
                }
                if (result == null || result == "")
                    return 0xFFFFF;
                else if (double.Parse(result) < 0)
                    return 0x1;
                else if (double.Parse(result) >= 0.8)
                    return 0x2;
                else
                    return 0x0;
            }
        }
        public static int TimeStamp()
        {
            return (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }
        public static bool CheckInternet()
        {

            try
            {
                using (WebClient client = new WebClient())
                using (client.OpenRead("http://clients3.google.com/generate_204"))
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
        public static string sha512gen(byte[] bytes)
        {
            SHA512Managed hashstring = new SHA512Managed();
            byte[] hash = hashstring.ComputeHash(bytes);
            string hashString = string.Empty;
            foreach (byte x in hash)
            {
                hashString += String.Format("{0:X2}", x);
            }
            return hashString;
        }
        public static string GetHWID()
        {
            var mbs = new ManagementObjectSearcher("Select ProcessorId From Win32_processor");
            ManagementObjectCollection mbsList = mbs.Get();
            string cpu = "";
            string hd = "";
            string mb = "";
            foreach (ManagementObject mo in mbsList)
            {
                cpu = mo["ProcessorId"].ToString();
                break;
            }
            ManagementObject dsk = new ManagementObject(@"win32_logicaldisk.deviceid=""c:""");
            dsk.Get();
            hd = dsk["VolumeSerialNumber"].ToString();
            ManagementObjectSearcher mos = new ManagementObjectSearcher("SELECT * FROM Win32_BaseBoard");
            ManagementObjectCollection moc = mos.Get();
            foreach (ManagementObject mo in moc)
            {
                mb = (string)mo["SerialNumber"];
            }
            return sha512gen(Encoding.UTF8.GetBytes("CPU {" + cpu + "}, HARDDRIVE {" + hd + "}, MOTHERBOARD {" + mb + "}"));
        }
    }
}
