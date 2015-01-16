#region

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Audiotica.Core.Utilities;

#endregion

namespace Audiotica
{
    public static class AutcpFormatHelper
    {
        public const int FormatVersion = 1;

        private const int FileHeaderSize = 37;


        public static async Task UnpackBackup(StorageFolder folder, Stream backupStream)
        {
            using (var zip = new ZipArchive(backupStream))
            {
                foreach (var entry in zip.Entries)
                {
                    try
                    {
                        var file =
                            await
                                StorageHelper.CreateFileAsync(entry.FullName,
                                    option: CreationCollisionOption.ReplaceExisting);
                        using (var stream = await file.OpenStreamForWriteAsync())
                        {
                            var original = entry.Open();
                            await original.CopyToAsync(stream);
                        }
                    }
                    catch { }
                }
            }
        }

        public static async Task<byte[]> CreateBackup(StorageFolder folder)
        {
            using (var autcpStream = new MemoryStream())
            {
                //add header now
                AddHeader(autcpStream);

                #region Compress everything

                using (var zipArchive = new ZipArchive(autcpStream, ZipArchiveMode.Create))
                {
                    var filesToCompress = await folder.GetItemsAsync();

                    foreach (var item in filesToCompress)
                    {
                        var files = new List<StorageFile>();

                        if (item.IsOfType(StorageItemTypes.File))
                        {
                            var file = (item as StorageFile);
                            if (file.FileType == ".autcp"
                                || file.FileType == ".sqldb"
                                || file.Name.StartsWith("_"))
                                continue;
                            files.Add(file);
                        }
                        else if (item.IsOfType(StorageItemTypes.Folder))
                        {
                            var name = (item as StorageFolder).Name;
                            if (name == "SOMA" || name == "Logs" || name == "AdMediator"
                                || name == "artists")
                                continue;

                            files.AddRange(await (item as StorageFolder).GetFilesAsync());
                        }

                        foreach (var file in files)
                        {
                            using (var stream = (await file.OpenStreamForReadAsync()))
                            {
                                var path = file.Path.Replace(folder.Path + "\\", "").Replace("\\", "/");

                                var entry = zipArchive.CreateEntry(path, CompressionLevel.Optimal);
                                using (var entryStream = entry.Open())
                                {
                                    await stream.CopyToAsync(entryStream);
                                }
                            }
                        }
                    }
                }

                #endregion

                return autcpStream.ToArray();
            }
        }

        public static bool AddHeader(Stream stream)
        {
            var fileHeader = new AutcpFileHeader
            {
                signature = Encoding.UTF8.GetBytes("AUTCP"),
                version = 1,
                compatability = 1
            };

            return WriteFileHeader(stream, fileHeader);
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
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 37)]
    public struct AutcpFileHeader
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)] public byte[] signature;
        public UInt16 version;
        public UInt16 compatability;
    }
}