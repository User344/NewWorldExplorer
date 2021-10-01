using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace NewWorldExplorer.NewWorld
{
    public class Pak : IDisposable
    {
        public string Name { get; private set; }
        public List<PakFile> Files { get; private set; }

        private ZipArchive _zip;

        internal Pak(ZipArchive zip, string name, List<PakFile> files)
        {
            _zip = zip;
            Name = name;
            Files = files;
        }

        public void Dispose()
        {
            _zip.Dispose();
        }

        public static Task<Pak> Load(string path)
        {
            var zip = ZipFile.OpenRead(path);
            var files = new List<PakFile>();

            foreach (var entry in zip.Entries)
            {
                var file = new PakFile(zip, entry);
                files.Add(file);
            }

            var pak = new Pak(zip, Path.GetFileName(path), files);
            return Task.FromResult(pak);
        }
    }
}
