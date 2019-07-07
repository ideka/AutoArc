using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;

namespace AutoArc
{
    public class Remote
    {
        public const string LocalPath = "bin64/";
        public const string LocalBakPath = "bin64/bak/";

        public string Name { get; set; }
        public bool BackupOld { get; set; } = true;

        public string RemotePath { get; set; }
        public string ChecksumPath { get; set; }

        public string LocalName
        {
            get => this.localName ?? this.RemotePath.Split('/').Last();
            set => this.localName = value;
        }

        private string localName = null;

        public void Update()
        {
            Console.WriteLine("Checking for {0} updates...", this.Name);

            string fullLocalPath = LocalPath + this.LocalName;

            using (var resp = (HttpWebResponse)WebRequest.Create(this.RemotePath).GetResponse())
            {
                if (File.Exists(fullLocalPath) && File.GetLastWriteTimeUtc(fullLocalPath) >= resp.LastModified)
                {
                    Console.WriteLine("Up to date ({0}).\n", resp.LastModified);
                    return;
                }

                Console.WriteLine("Downloading update ({0})...", resp.LastModified);
            }

            if (File.Exists(fullLocalPath))
            {
                Directory.CreateDirectory(LocalBakPath);
                File.Copy(fullLocalPath, LocalBakPath + this.LocalName, true);
            }

            byte[] file;
            using (var wc = new WebClient())
            {
                file = wc.DownloadData(this.RemotePath);

                if (this.ChecksumPath != null)
                {
                    using (var md5 = MD5.Create())
                    {
                        string checksum = wc.DownloadString(this.ChecksumPath).Split(' ')[0].Trim();
                        string downloadChecksum = BitConverter.ToString(md5.ComputeHash(file)).Replace("-", "").ToLower();
                        bool equal = checksum == downloadChecksum;
                        Console.WriteLine("{0} == {1} ({2})", checksum, downloadChecksum, equal);
                        if (!equal)
                        {
                            Console.WriteLine();
                            Console.ReadKey();
                            return;
                        }
                    }
                }
                else
                {
                    Console.WriteLine("No checksum verification for {0}.", this.Name);
                }
            }

            Console.WriteLine();
            File.WriteAllBytes(fullLocalPath, file);
        }
    }
}