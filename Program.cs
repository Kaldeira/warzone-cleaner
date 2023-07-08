using Microsoft.Win32;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Security.AccessControl;
using System.Diagnostics;


namespace warzoneHelper
{
    internal class Program
    {
        private static bool bCleanSteam = false;
        private static bool bCleanTrace = false;
        private static bool bCleanReg = false;
        private static bool bSpoofGuid = false;

        private static void Main(string[] args)
        {
            Task application = new Task(closeApplication);
            Task files = new Task(cleanFiles);
            Task registry = new Task(cleanRegistry);
            Task guid = new Task(spoofGuid);
            Task steam = new Task(cleanSteam);

            application.Start();
            application.Wait();

            Console.WriteLine("Warzone Shadow Ban Helper by Dukk1\n\n");

            Console.WriteLine("[1] Clean Traces files");
            Console.WriteLine("[2] Clean Steam files");
            Console.WriteLine("[3] Clean Registry");
            Console.WriteLine("[4] Spoof GUID");
            Console.WriteLine("[5] Do ALL");

        startMenu:
            Console.WriteLine("\r\nSelect an option: ");
            switch (Console.ReadLine())
            {
                case "1":
                    bCleanTrace = true;
                    break;
                case "2":
                    bCleanSteam = true;
                    break;
                case "3":
                    bCleanReg = true;
                    break;
                case "4":
                    bSpoofGuid = true;
                    break;
                case "5":
                    bCleanSteam = true;
                    bCleanTrace = true;
                    bCleanReg = true;
                    bSpoofGuid = true;
                    break;
                default:
                    goto startMenu;
            }

            steam.Start();
            steam.Wait();

            files.Start();
            files.Wait();

            registry.Start();
            registry.Wait();

            guid.Start();
            guid.Wait();

            Console.WriteLine("\nPlease restart your system, then repair your game.\n");
            Console.WriteLine("if shadow ban persists, reinstall battle.net or steam");
            Console.ReadKey();
        }

        private static void cleanSteam()
        {
            if (!bCleanSteam)
                return;

            Console.WriteLine("Cleaning Steam Files...\n");

            string[] directoryPaths = {
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + $"\\AppData\\Local\\Steam",
                $"C:\\Program Files (x86)\\Steam\\userdata",
                $"C:\\Program Files (x86)\\Steam\\dumps",
                $"C:\\Program Files (x86)\\Steam\\logs",
                $"C:\\Program Files (x86)\\Steam\\appcache",
                $"C:\\Program Files (x86)\\Steam\\config\\loginusers.vdf",
            };

            foreach (string directoryPath in directoryPaths)
            {
                DirectoryInfo directory = new DirectoryInfo(directoryPath);
                if (directory.Exists)
                {
                    Console.WriteLine("Deleted " + directory.ToString());
                    directory.Delete(true);
                }
            }

            string[] filePaths = {
                $"C:\\Program Files (x86)\\Steam\\config\\loginusers.vdf",
            };

            foreach (string filePath in filePaths)
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    Console.WriteLine("Deleted " + filePath.ToString());
                }
            }

            string steamMWReg = @"SOFTWARE\Valve\Steam\Apps";
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(steamMWReg, true))
            {
                if (key != null)
                {
                    foreach (string subkey in key.GetSubKeyNames())
                    {
                        if (subkey == "1938090") //codigo do game na steam
                        {
                            Console.WriteLine("[Subkey] Apps/1938090: Found and Deleted");
                            key.DeleteSubKeyTree(subkey);
                        }
                    }
                }
            }

            string steamFolder = @"SOFTWARE\Valve\Steam";
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(steamFolder, true))
            {
                if (key != null)
                {
                    foreach (string keyValues in key.GetValueNames())
                    {
                        if (keyValues == "AutoLoginUser")
                        {
                            Console.WriteLine("[Value] Steam -> AutoLoginUser: Found and Deleted");
                            key.DeleteValue(keyValues);
                        }
                    }

                    foreach (string subkey in key.GetSubKeyNames())
                    {
                        if (subkey == "Users")
                        {
                            Console.WriteLine("[Subkey] Steam/Users: Found and Deleted");
                            key.DeleteSubKeyTree(subkey);
                        }

                        if (subkey == "ActiveProcess")
                        {
                            Console.WriteLine("[Subkey] Steam/ActiveProcess: Found and Deleted");
                            key.DeleteSubKeyTree(subkey);
                        }
                    }
                }
            }

            string appsFolderLM = @"SOFTWARE\Valve\Steam\Apps";
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(appsFolderLM, true))
            {
                if (key != null)
                {
                    foreach (string subkey in key.GetSubKeyNames())
                    {
                        if (subkey == "1938090") //codigo do game na steam
                        {
                            Console.WriteLine("[LM Subkey] Apps/1938090: Found and Deleted");
                            key.DeleteSubKeyTree(subkey);
                        }
                    }
                }
            }

            Console.WriteLine("Cleaned Steam files.");
        }

        private static void cleanFiles()
        {
            if (!bCleanTrace)
                return;

            Console.WriteLine("Cleaning Trace Files...\n");

            string wz1dir = find_warzone1_directory();
            string wz2dir = find_warzone2_directory();

            if (wz1dir == null)
                Console.WriteLine("Warzone 1 dir not found.");
            else
                Console.WriteLine("Warzone 1 dir found : " + wz1dir.ToString());

            if (wz2dir == null)
                Console.WriteLine("Warzone 2 dir not found.");
            else
                Console.WriteLine("Warzone 2 dir found : " + wz2dir.ToString());


            string[] directoryPaths = {
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + $"\\AppData\\Local\\Battle.net",
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + $"\\AppData\\Local\\Blizzard Entertainment",
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + $"\\AppData\\Roaming\\Battle.net",
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + $"\\Documents\\Call of Duty Modern Warfare",
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + $"\\Documents\\Call of Duty",
                //Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + $"\\AppData\\Local\\Steam",
                //$"C:\\Program Files (x86)\\Steam\\userdata",
                //$"C:\\Program Files (x86)\\Steam\\dumps",
                //$"C:\\Program Files (x86)\\Steam\\logs",
                //$"C:\\Program Files (x86)\\Steam\\appcache",
                $"C:\\ProgramData\\Battle.net",
                $"C:\\ProgramData\\Blizzard Entertainment"
            };

            foreach (string directoryPath in directoryPaths)
            {
                DirectoryInfo directory = new DirectoryInfo(directoryPath);
                if (directory.Exists)
                {
                    Console.WriteLine("Deleted " + directory.ToString());
                    directory.Delete(true);
                }
            }

            string[] filePaths = {
                //$"C:\\Program Files (x86)\\Steam\\config\\loginusers.vdf",

                wz2dir + $"\\main\\data0.dcache",
                wz2dir + $"\\main\\data1.dcache",
                wz2dir + $"\\main\\toc0.dcache",
                wz2dir + $"\\main\\toc1.dcache",
                wz2dir + $"\\main\\fileSysCheck.cfg",

                wz1dir + $"\\main\\data0.dcache",
                wz1dir + $"\\main\\data1.dcache",
                wz1dir + $"\\main\\toc0.dcache",
                wz1dir + $"\\main\\toc1.dcache",
                wz1dir + $"\\Data\\data\\shmem",
                wz1dir + $"\\main\\recipes\\cmr_hist"
            };

            foreach (string filePath in filePaths)
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    Console.WriteLine("Deleted " + filePath.ToString());
                }
            }

            Console.WriteLine("Cleaned Trace files.");

        }

        private static void cleanRegistry()
        {
            if (!bCleanReg)
                return;

            Console.WriteLine("Cleaning Registry...\n");

            string Software = @"SOFTWARE";
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(Software, true))
            {
                if (key != null)
                {
                    foreach (string subkey in key.GetSubKeyNames())
                    {
                        if (subkey == "Hex-Rays")
                        {
                            Console.WriteLine("[LM Subkey] Software/Hex-Rays: Found and Deleted");
                            key.DeleteSubKeyTree(subkey);
                        }
                    }
                }
            }

            string WOW6432Node = @"SOFTWARE\WOW6432Node";
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(WOW6432Node, true))
            {
                if (key != null)
                {
                    foreach (string subkey in key.GetSubKeyNames())
                    {
                        if (subkey == "Blizzard Entertainment")
                        {
                            Console.WriteLine("[LM Subkey] WOW6432Node/Blizzard Entertainment: Found and Deleted");
                            key.DeleteSubKeyTree(subkey);
                        }

                        if (subkey == "Valve")
                        {
                            Console.WriteLine("[LM Subkey] WOW6432Node/Valve: Found and Deleted");
                            key.DeleteSubKeyTree(subkey);
                        }
                    }
                }
            }

            string Blizzard = @"Software\Blizzard Entertainment";
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(Blizzard, true))
            {
                if (key != null)
                {
                    foreach (string subkey in key.GetSubKeyNames())
                    {
                        if (subkey == "Battle.net")
                        {
                            Console.WriteLine("[Subkey] Software/Battle.net/: Found and Deleted");
                            key.DeleteSubKeyTree(subkey);
                        }
                    }
                }
            }

            Console.WriteLine("Cleaned registry.");
        }

        private static void spoofGuid()
        {
            if (!bSpoofGuid)
                return;

            Console.WriteLine("Spoofing GUID...\n");

            Guid guid = Guid.NewGuid();

            string GUID = @"SYSTEM\CurrentControlSet\Control\IDConfigDB\Hardware Profiles\0001";
            RegistryKey subkey = Registry.LocalMachine.OpenSubKey(GUID, true);
            if (subkey != null)
            {
                string user = Environment.UserDomainName + "\\" + Environment.UserName;
                RegistrySecurity rs = new RegistrySecurity();
                rs.AddAccessRule(new RegistryAccessRule(user,
                    RegistryRights.FullControl,
                    InheritanceFlags.None,
                    PropagationFlags.None,
                    AccessControlType.Allow));
                subkey.SetAccessControl(rs);
                subkey.SetValue("HwProfileGuid", "{" + guid + "}", RegistryValueKind.String);
                subkey.Close();
            }

            Console.WriteLine("GUID spoofed to " + guid);
        }

        private static string find_warzone1_directory()
        {
            RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.Users, RegistryView.Default);
            foreach (string subKey in baseKey.GetSubKeyNames())
            {
                if (subKey.Contains("S-1-5-21-") && !subKey.Contains("Classes"))
                {
                    RegistryKey games = baseKey.OpenSubKey(subKey + @"\System\GameConfigStore\Children");
                    foreach (string game in games.GetSubKeyNames())
                    {
                        RegistryKey temp = games.OpenSubKey(game);
                        if (Equals(temp.GetValue("TitleId"), "1787008472"))
                        {
                            string ExePath = temp.GetValue("MatchedExeFullPath").ToString();
                            return ExePath.Substring(0, ExePath.LastIndexOf("\\"));
                        }
                    }
                }
            }
            return null;
        }

        private static string find_warzone2_directory()
        {
            string services = @"SYSTEM\CurrentControlSet\Services";
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(services, true))
            {
                if (key != null)
                {
                    foreach (string subkey in key.GetSubKeyNames())
                    {

                        RegistryKey randgrid = key.OpenSubKey("atvi-randgrid_sr");

                        if (randgrid != null)
                        {
                            string gamePath = randgrid.GetValue("ImagePath").ToString();
                            string clean1 = gamePath.Substring(0, gamePath.LastIndexOf("\\")); ;
                            string clean2 = clean1.Substring(4);

                            return clean2;
                        }
                    }
                }
            }

            return null;
        }

        private static void closeApplication()
        {
            string[] processNames = { "Agent", "Battle.net", "Steam", "cod", "modernwarfare", "hl1", "hl2" };

            foreach (string processName in processNames)
            {
                Process[] processes = Process.GetProcessesByName(processName);
                foreach (Process process in processes) process.Kill();
            }
            Console.WriteLine("Closed applications.");
        }
    }
}
