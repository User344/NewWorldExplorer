using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using Oodle.NET;

namespace NewWorldExplorer.NewWorld
{
    public class PakFile
    {
        private ZipArchive _zip;
        private ZipArchiveEntry _entry;

        public string Name => _entry.Name;
        public string FullName => _entry.FullName;

        internal PakFile(ZipArchive zip, ZipArchiveEntry entry)
        {
            _zip = zip;
            _entry = entry;
        }

        public Task<byte[]> Export()
        {
            var buffer = new byte[_entry.Length];

            var compressionField = _entry.GetType().GetField("_storedCompressionMethod", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var compressionMethod = Convert.ToInt32(compressionField.GetValue(_entry));

            if (compressionMethod == 0)
            {
                // No compression

                _entry.Open().Read(buffer, 0, (int)_entry.Length);
            }
            else if (compressionMethod == 15)
            {
                // Oodle compression
                using var oodle = new OodleCompressor(@"oo2core_8_win64.dll");

                // Force "No compression" so we can easily read uncompressed data
                compressionField.SetValue(_entry, Enum.ToObject(compressionField.FieldType, 0));

                var compressed = new byte[_entry.CompressedLength];
                _entry.Open().Read(compressed, 0, compressed.Length);

                var result = oodle.DecompressBuffer(
                    compressed, compressed.Length, buffer, buffer.Length,
                    OodleLZ_FuzzSafe.No, OodleLZ_CheckCRC.No, OodleLZ_Verbosity.None,
                    0L, 0L, 0L, 0L, 0L, 0L, OodleLZ_Decode_ThreadPhase.Unthreaded
                );
            }
            else
            {
                throw new NotImplementedException();
            }

            return Task.FromResult(buffer);
        }

        public async Task Save(string path)
        {
            var data = await Export();
            File.WriteAllBytes(path, data);
        }
    }
}
