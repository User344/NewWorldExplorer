using System;
using System.IO;
using System.Threading.Tasks;
using NewWorld = NewWorldExplorer.NewWorld;

namespace NewWorldExplorerCLI
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine($"Usage: NewWorldExplorerCLI.exe <game dir>");
                return;
            }

            var dir = args[0];
            if (!Directory.Exists(dir))
            {
                Console.WriteLine("Game directory doesn't exist!");
                return;
            }

            var paksList = ReadPaksList(dir);
            if (paksList.Length == 0)
            {
                Console.WriteLine("Couldn't read PaksList.lst!");
                return;
            }
            Console.WriteLine($"Found {paksList.Length} paks!");

            foreach (var pakPath in paksList)
            {
                if (String.IsNullOrEmpty(pakPath))
                    continue;

                var path = Path.Combine(dir, pakPath);
                if (!File.Exists(path)) continue;

                Console.WriteLine($"Processing {pakPath}... ");
                using var progress = new ProgressBar();

                var pak = await NewWorld.Pak.Load(path);
                var i = 0;
                foreach (var file in pak.Files)
                {
                    progress.Report((double)i++ / (double)pak.Files.Count);

                    var filePath = Path.Combine("output", file.FullName);
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                    await file.Save(filePath);
                }
            }

            Console.WriteLine("Done!");
        }

        public static string[] ReadPaksList(string dir)
        {
            var path = Path.Combine(dir, "PaksList.lst");
            if (!File.Exists(path)) return new string[] { };

            return File.ReadAllLines(path);
        }
    }
}
