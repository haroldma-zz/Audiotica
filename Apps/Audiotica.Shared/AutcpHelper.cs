#region

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;

using Audiotica.Core.Utils;

using PCLStorage;

using Windows.Storage;

using CreationCollisionOption = PCLStorage.CreationCollisionOption;

#endregion

namespace Audiotica
{
    public static class AutcpFormatHelper
    {
        public const int FormatVersion = 2;

        public const int FormatCompatabilityVersion = 2;

        private const int FileHeaderSize = 37;

        public static bool AddHeader(Stream stream)
        {
            var fileHeader = new AutcpFileHeader
            {
                signature = Encoding.UTF8.GetBytes("AUTCP"), 
                version = FormatVersion, 
                compatability = FormatCompatabilityVersion
            };

            return WriteFileHeader(stream, fileHeader);
        }

        public static async Task<byte[]> CreateBackup(StorageFolder folder)
        {
            using (var autcpStream = new MemoryStream())
            {
                // add header now
                AddHeader(autcpStream);

                using (var zipArchive = new ZipArchive(autcpStream, ZipArchiveMode.Create))
                {
                    var filesToCompress = await folder.GetItemsAsync();

                    foreach (var item in filesToCompress)
                    {
                        var files = new List<StorageFile>();

                        if (item is StorageFile)
                        {
                            var file = item as StorageFile;
                            if (file.FileType == ".autcp" || file.Name.ToLower().StartsWith("xam")
                                || file.Name.StartsWith("_"))
                            {
                                continue;
                            }

                            files.Add(file);
                        }
                        else if (item is StorageFolder)
                        {
                            var name = (item as StorageFolder).Name;
                            if (name == "SOMA" || name == "Logs" || name == "AdMediator" || name == "artists")
                            {
                                continue;
                            }

                            files.AddRange(await (item as StorageFolder).GetFilesAsync());
                        }

                        foreach (var file in files)
                        {
                            var buffer = await FileIO.ReadBufferAsync(file);

                            if (buffer.Length == 0)
                            {
                                continue;
                            }

                            var bytes = buffer.ToArray();

                            var path = file.Path.Replace(folder.Path + "\\", string.Empty).Replace("\\", "/");

                            var entry = zipArchive.CreateEntry(path, CompressionLevel.Optimal);
                            using (var entryStream = entry.Open())
                            {
                                await entryStream.WriteAsync(bytes, 0, bytes.Length);
                                await entryStream.FlushAsync();
                            }
                        }
                    }
                }

                

                return autcpStream.ToArray();
            }
        }

        public static bool ReadFileHeader(Stream stream, out AutcpFileHeader fileHeader)
        {
            try
            {
                var buffer = new byte[FileHeaderSize];

                stream.Seek(0, SeekOrigin.Begin);
                stream.Read(buffer, 0, FileHeaderSize);

                var gch = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                fileHeader = Marshal.PtrToStructure<AutcpFileHeader>(gch.AddrOfPinnedObject());

                gch.Free();

                // test for valid data
                var sig = Encoding.UTF8.GetString(fileHeader.signature, 0, fileHeader.signature.Length);
                var isSuccessful = sig == "AUTCP";

                return isSuccessful;
            }
            catch (Exception ex)
            {
                fileHeader = new AutcpFileHeader();
                return false;
            }
        }

        public static async Task UnpackBackup(StorageFolder folder, Stream backupStream)
        {
            using (var zip = new ZipArchive(backupStream))
            {
                foreach (var entry in zip.Entries)
                {
                    try
                    {
                        var name = entry.FullName;

                        // compatability with Autcp v1
                        if (name.Contains(".bksqldb"))
                        {
                            name = name.Replace(".bksqldb", ".sqldb");
                        }

                        var file =
                            await StorageHelper.CreateFileAsync(name, option: CreationCollisionOption.ReplaceExisting);
                        using (var stream = await file.OpenAsync(FileAccess.ReadAndWrite))
                        {
                            using (var original = entry.Open())
                            {
                                await original.CopyToAsync(stream);
                            }
                        }
                    }
                    catch
                    {
                    }
                }
            }
        }

        public static bool ValidateHeader(Stream stream)
        {
            AutcpFileHeader header;
            var valid = ReadFileHeader(stream, out header);
            if (valid)
            {
                valid = FormatVersion >= header.compatability;
            }

            return valid;
        }

        public static bool WriteFileHeader(Stream stream, AutcpFileHeader fileHeader)
        {
            try
            {
                var buffer = new byte[FileHeaderSize];

                var gch = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                Marshal.StructureToPtr(fileHeader, gch.AddrOfPinnedObject(), false);
                gch.Free();

                stream.Seek(0, SeekOrigin.Begin);
                stream.Write(buffer, 0, FileHeaderSize);

                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 37)]
    public struct AutcpFileHeader
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
        public byte[] signature;

        public ushort version;

        public ushort compatability;
    }
}