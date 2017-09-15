using System;
using System.Net;
using System.Security.Cryptography;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace AutoArc
{
    public class Program
    {
        public const string OldSuffix = ".old";
        public const string ChecksumSuffix = ".md5sum";

        public const string RemotePath = "https://www.deltaconnected.com/arcdps/x64/";
        public const string LocalPath = "bin64/";

        public const string GameExeName = "Gw2-64.exe";

        private static void Main(string[] args)
        {
            Download(RemotePath, LocalPath, "d3d9.dll", true);
            Download(RemotePath + "buildtemplates/", LocalPath, "d3d9_arcdps_buildtemplates.dll", false);

            if (args.Any())
                Process.Start(GameExeName, args.Last());
            else
                Process.Start(GameExeName);
        }

        private static void Download(string remotePath, string localPath, string name, bool hasChecksum)
        {
            Console.WriteLine("Checking for {0} updates...", name);

            {
                HttpWebResponse resp = (HttpWebResponse)WebRequest.Create(remotePath + name).GetResponse();

                if (File.Exists(localPath + name) && File.GetLastWriteTimeUtc(localPath + name) >= resp.LastModified)
                {
                    Console.WriteLine("Up to date ({0}).", resp.LastModified);
                    return;
                }

                Console.WriteLine("Downloading update ({0})...", resp.LastModified);
                resp.Close();
            }

            if (File.Exists(localPath + name))
                File.Copy(localPath + name, localPath + name + OldSuffix, true);

            byte[] file;
            using (WebClient wc = new WebClient())
            {
                file = wc.DownloadData(remotePath + name);

                if (hasChecksum)
                {
                    using (MD5 md5 = MD5.Create())
                    {
                        string checksum = wc.DownloadString(remotePath + name + ChecksumSuffix).Split(' ')[0];
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

            File.WriteAllBytes(localPath + name, file);
        }
    }
}