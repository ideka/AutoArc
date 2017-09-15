using System;
using System.Net;
using System.Security.Cryptography;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace AutoArc
{
    public class Download
    {
        public static string OldSuffix = ".old";

        public string Source { get; private set; }
        public string Name { get; private set; }
        public string ChecksumName { get; private set; }

        public Download(string source, string name, string checksumName = null)
        {
            this.Source = source;
            this.Name = name;
            this.ChecksumName = checksumName;
        }

        public void Perform(string targetPath)
        {
            Console.WriteLine("Checking for {0} updates...", this.Name);

            {
                HttpWebResponse resp = (HttpWebResponse)WebRequest.Create(this.Source + this.Name).GetResponse();

                if (File.Exists(targetPath + this.Name) && File.GetLastWriteTimeUtc(targetPath + this.Name) >= resp.LastModified)
                {
                    Console.WriteLine("Up to date ({0}).", resp.LastModified);
                    return;
                }

                Console.WriteLine("Downloading update ({0})...", resp.LastModified);
                resp.Close();
            }

            if (File.Exists(targetPath + this.Name))
                File.Copy(targetPath + this.Name, targetPath + this.Name + OldSuffix, true);

            byte[] file;
            using (WebClient wc = new WebClient())
            {
                file = wc.DownloadData(this.Source + this.Name);

                if (this.ChecksumName != null)
                {
                    using (MD5 md5 = MD5.Create())
                    {
                        string checksum = wc.DownloadString(this.Source + this.ChecksumName).Split(' ')[0];
                        string downloadChecksum = BitConverter.ToString(md5.ComputeHash(file)).Replace("-", "").ToLower();
                        bool equal = checksum == downloadChecksum;
                        Console.WriteLine("{0} == {1} ({2})", checksum, downloadChecksum, equal);
                        if (!equal)
                        {
                            Console.ReadKey();
                            return;
                        }
                    }
                }
                else
                {
                    Console.WriteLine("No checksum verification.");
                }
            }

            File.WriteAllBytes(targetPath + this.Name, file);
        }
    }

    public class Program
    {
        public const string DllLocalLocation = "bin64/";
        public const string GameExeName = "Gw2-64.exe";

        public const string Source = "https://www.deltaconnected.com/arcdps/x64/";
        public const string DllName = "d3d9.dll";
        public const string ChecksumName = "d3d9.dll.md5sum";

        public const string DllOldName = "d3d9.dll.old";

        private static void Main(string[] args)
        {
            //UpdateArc();
            new Download(Source, "d3d9.dll", "d3d9.dll.md5sum").Perform(DllLocalLocation);
            new Download(Source + "buildtemplates/", "d3d9_arcdps_buildtemplates.dll").Perform(DllLocalLocation);

            if (args.Any())
                Process.Start(GameExeName, args.Last());
            else
                Process.Start(GameExeName);
        }

        private static void UpdateArc()
        {
            {
                HttpWebResponse resp = (HttpWebResponse)WebRequest.Create(Source + ChecksumName).GetResponse();

#if !DEBUG
                if (File.Exists(DllLocalLocation + DllName) && File.GetLastWriteTimeUtc(DllLocalLocation + DllName) >= resp.LastModified)
                {
                    Console.WriteLine("Up to date ({0}).", resp.LastModified);
                    return;
                }
#endif

                Console.WriteLine("Downloading update ({0})...", resp.LastModified);
                resp.Close();
            }

#if !DEBUG
            if (File.Exists(DllLocalLocation + DllName))
                File.Copy(DllLocalLocation + DllName, DllLocalLocation + DllOldName, true);
#endif

            byte[] dll;
            using (WebClient wc = new WebClient())
            using (MD5 md5 = MD5.Create())
            {
                string checksum = wc.DownloadString(Source + ChecksumName).Split(' ')[0];

                dll = wc.DownloadData(Source + DllName);
                string downloadChecksum = BitConverter.ToString(md5.ComputeHash(dll)).Replace("-", "").ToLower();

                bool equal = checksum == downloadChecksum;
                Console.WriteLine("{0} == {1} ({2})", checksum, downloadChecksum, equal);
                if (!equal)
                {
#if !DEBUG
                    Console.ReadKey();
#endif
                    return;
                }
            }

#if !DEBUG
            File.WriteAllBytes(DllLocalLocation + DllName, dll);
#endif
        }
    }
}