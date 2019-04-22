using System.Diagnostics;
using System.Linq;

namespace AutoArc
{
    public class Program
    {
        public const string GameExeName = "Gw2-64.exe";

        private static void Main(string[] args)
        {
            new Remote
            {
                Name = "ArcDPS",
                RemotePath = "https://www.deltaconnected.com/arcdps/x64/d3d9.dll",
                ChecksumPath = "https://www.deltaconnected.com/arcdps/x64/d3d9.dll.md5sum",
            }.Update();

            new Remote
            {
                Name = "ArcDPS Build Templates",
                RemotePath = "https://www.deltaconnected.com/arcdps/x64/buildtemplates/d3d9_arcdps_buildtemplates.dll",
            }.Update();

            new Remote
            {
                Name = "ArcDPS Extras",
                RemotePath = "https://www.deltaconnected.com/arcdps/x64/extras/d3d9_arcdps_extras.dll",
            }.Update();

            new Remote
            {
                Name = "ArcDPS Mechanics",
                RemotePath = "http://martionlabs.com/wp-content/uploads/d3d9_arcdps_mechanics.dll",
                ChecksumPath = "http://martionlabs.com/wp-content/uploads/d3d9_arcdps_mechanics.dll.md5sum",
            }.Update();

            if (args.Any())
                Process.Start(GameExeName, args.Last());
            else
                Process.Start(GameExeName);
        }

    }
}