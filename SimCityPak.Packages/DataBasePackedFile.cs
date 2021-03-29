using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Gibbed.Spore.Helpers;
using System.Linq;
using SimCityPak.PackageReader;

namespace Gibbed.Spore.Package
{
    public class DatabasePackedFileException : Exception
    {
    }

    public class NotAPackageException : DatabasePackedFileException
    {
    }

    public class UnsupportedPackageVersionException : DatabasePackedFileException
    {
    }



    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DatabasePackedFileHeader
    {
        //public uint Magic;		// 00
        public int MajorVersion;	// 04
        public int MinorVersion;	// 08
        public uint Unknown0C;		// 0C
        public uint Unknown10;		// 10
        public uint Unknown14;		// 14 - always 0?
        public uint Unknown18;		// 18 - always 0?
        public uint Unknown1C;		// 1C - always 0?
        public uint Unknown20;		// 20
        public int IndexCount;		// 24 - Number of index entries in the package.
        public uint Unknown28;		// 28
        public int IndexSize;		// 2C - The total size in bytes of index entries.
        public uint Unknown30;		// 30
        public uint Unknown34;		// 34
        public uint Unknown38;		// 38
        public uint Always3;		// 3C - Always 3?
        public int IndexOffset;		// 40 - Absolute offset in package to the index header.
        public uint Unknown44;		// 44
        public uint Unknown48;		// 48
        public uint Unknown4C;		// 4C
        public uint Unknown50;		// 50
        public uint Unknown54;		// 54
        public uint Unknown58;		// 58
        public uint Unknown5C;		// 5C
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DatabaseBigPackageFileHeader
    {
        //public uint Magic;		// 00
        public int MajorVersion;	// 04
        public int MinorVersion;	// 08
        public uint Unknown0C;		// 0C //Major User version
        public uint Unknown10;		// 10 //Minor User Version
        public uint Unknown14;		// 14 - always 0? //Flags
        public uint Unknown18;		// 18 - always 0? //Created Date/Time Stamp
        public uint Unknown1C;		// 1C - always 0? //Modified Data/Time Stamp
        public uint Unknown20;		// 20 Index major version
        public int IndexCount;		// 24 - Number of index entries in the package.
        public Int64 IndexSize;		// 28 - The total size in bytes of index entries.
        public uint Unknown30;		// 30 - Index size in bytes?
        public uint Always3;		// 34 - Always 3?
        public Int64 IndexOffset;	// 38 - Absolute offset in package to the index header.
        public uint Unknown40;		// 40
        public uint Unknown44;		// 44
        public uint Unknown48;		// 48
        public uint Unknown4C;		// 4C
        public uint Unknown50;		// 50
        public uint Unknown54;		// 54
        public uint Unknown58;		// 58
        public uint Unknown5C;		// 5C
        public uint Unknown60;		// 60
        public uint Unknown64;		// 64
        public uint Unknown68;		// 68
        public uint Unknown6C;		// 6C
        public uint Unknown70;		// 70
        public uint Unknown74;		// 74
    }

    public class DatabasePackedFile
    {
        /// <summary>
        /// Contains a reference to the original file that was loaded.
        /// </summary>
        private FileInfo _packageFile;

        //private constructor
        private DatabasePackedFile()
        {
            Indices = new ObservableList<DatabaseIndex>();
        }

        public string FileName
        {
            get { return _packageFile.Name; } 
        }

        public FileInfo packageFileInfo
        {
            get { return _packageFile; }
        }

        public bool IsModified
        {
            get { return Indices.Exists(i => i.IsModified); }
        }
        /// <summary>
        /// Loads a DataBasePackedFile .package file from disk
        /// </summary>
        /// <param name="path">The full path to the file to be loaded</param>
        /// <returns>a DBPF instance</returns>
        public static DatabasePackedFile LoadFromFile(string path)
        {
            DatabasePackedFile file = new DatabasePackedFile();
            file._packageFile = new FileInfo(path);
            using (FileStream stream = file._packageFile.OpenRead())
            {
                file.Read(stream);
            }
            return file;
        }

        public static DatabasePackedFile Create()
        {
            DatabasePackedFile file = new DatabasePackedFile();
            return file;
        }

        public byte[] GetData(uint offset, int length)
        {
            byte[] data = new byte[length];

            using (FileStream stream = _packageFile.OpenRead())
            {
                stream.Seek(offset, SeekOrigin.Begin);
                stream.Read(data, 0, length);
            }

            return data;
        }
        public DatabaseIndex InsertSubFile(byte[] data, uint typeId = 0, uint GroupContainer = 0, uint instanceId = 0)
        {
            DatabaseIndex newIndex = new DatabaseIndex(this);

            newIndex.CompressedSize = (uint)data.Length;
            newIndex.Compressed = false;
            newIndex.CompressedFlags = 0;
            newIndex.DecompressedSize = (uint)data.Length;
            newIndex.GroupContainer = GroupContainer;
            newIndex.InstanceId = instanceId;
            newIndex.TypeId = typeId;
            newIndex.ModifiedData = new ModifiedGenericFile() { FileData = data };
            newIndex.IsModified = true;
            this.Indices.Add(newIndex);
            this.Indices.Changed();
            return newIndex;
        }
        public static void SaveAs(string fileName, List<DatabaseIndex> selectedIndices)
        {
            DatabasePackedFile newPackage = new DatabasePackedFile();
            newPackage.Version = new Version(3, 0);
            newPackage.Indices = new ObservableList<DatabaseIndex>();
            //elFarto, should this line really be like this?
            //shouldn't it just be AddRange(selectedIndices) ?
            //newPackage.Indices.AddRange(this.Indices.Where(si => selectedIndices.Contains(si)));
            newPackage.Indices.AddRange(selectedIndices);



            using (FileStream fileStream = File.Create(fileName))
            {
                // int totalSize = 0;
                // newPackage.Indices.ForEach(ind => totalSize += (int)ind.DecompressedSize);

                //   using (Stream str2 = File.OpenRead(@"J:\Games\SCPack2\SimCityData\turbine.png"))
                //   {
                //int start = 96; //start after the header
                // newPackage.WriteHeader(fileStream, start + totalSize, totalSize);
                newPackage.WriteHeader(fileStream, 0, 0);

                //  package.WriteIndex(fileStream);

                int totalSize = 0;
                foreach (DatabaseIndex subIndex in newPackage.Indices)
                {
                    byte[] data;

                    if (subIndex.IsModified)
                    {
                        data = subIndex.ModifiedData.GetData();

                        subIndex.Offset = fileStream.Position;
                        subIndex.CompressedSize = (uint)data.Length;
                        subIndex.Compressed = false;
                        subIndex.CompressedFlags = 0;
                        subIndex.DecompressedSize = (uint)data.Length;


                        subIndex.TypeId = subIndex.TypeId;
                        subIndex.GroupContainer = subIndex.GroupContainer;
                        subIndex.InstanceId = subIndex.InstanceId;

                        fileStream.Write(data, 0, data.Length);

                    }
                    else
                    {
                        data = subIndex.GetIndexData(false);
                        if (data.Length != subIndex.CompressedSize)
                        {
                            throw new Exception("error");
                        }

                        subIndex.Offset = fileStream.Position;

                        fileStream.Write(data, 0, data.Length);
                    }

                    totalSize += data.Length;
                }

                long positionBeforeIndex = fileStream.Position;

                newPackage.WriteIndex(fileStream);

                int indexOffset = 96 + totalSize;

                totalSize = (int)fileStream.Position;



                fileStream.Seek(0, SeekOrigin.Begin);
                newPackage.WriteHeader(fileStream, indexOffset, totalSize - (int)positionBeforeIndex);
            }
        }

        public void SaveAs(string fileName)
        {
            DatabasePackedFile newPackage = new DatabasePackedFile();
            newPackage.Version = new Version(3, 0);
            newPackage.Indices = this.Indices;

            using (FileStream fileStream = File.Create(fileName))
            {

                newPackage.WriteHeader(fileStream, 0, 0);

                int totalSize = 0;
                foreach (DatabaseIndex subIndex in newPackage.Indices)
                {
                    if (subIndex.Deleted == true)
                    {
                        continue;
                    }

                    byte[] data;

                    if (subIndex.IsModified)
                    {
                        data = subIndex.ModifiedData.GetData();

                        subIndex.Offset = fileStream.Position;
                        subIndex.CompressedSize = (uint)data.Length;
                        subIndex.Compressed = false;
                        subIndex.CompressedFlags = 0;
                        subIndex.DecompressedSize = (uint)data.Length;
          
                        fileStream.Write(data, 0, data.Length);

                    }
                    else
                    {
                        data = subIndex.GetIndexData(false);
                        if (data.Length != subIndex.CompressedSize)
                        {
                            throw new Exception("error");
                        }

                        subIndex.Offset = fileStream.Position;

                        fileStream.Write(data, 0, data.Length);
                    }

                    totalSize += data.Length;
                }

                long positionBeforeIndex = fileStream.Position;

                newPackage.WriteIndex(fileStream);

                int indexOffset = 96 + totalSize;

                totalSize = (int)fileStream.Position;



                fileStream.Seek(0, SeekOrigin.Begin);
                newPackage.WriteHeader(fileStream, indexOffset, totalSize - (int)positionBeforeIndex);
            }
        }

        public Version Version = new Version();

        public ObservableList<DatabaseIndex> Indices
        { get; set; }

        private void Read(Stream stream)
        {
            bool big = false;
            Int64 indexCount;
            Int64 indexSize;
            Int64 indexOffset;

            uint magic = stream.ReadU32();
            if (magic != 0x46504244 && magic != 0x46424244) // DBPF & DBBF
            {
                throw new NotAPackageException();
            }

            if (magic == 0x46424244) // DBBF
            {
                big = true;

                DatabaseBigPackageFileHeader header;
                byte[] data = new byte[Marshal.SizeOf(typeof(DatabaseBigPackageFileHeader))];

                if (data.Length != (120 - 4))
                {
                    throw new Exception("DatabaseBigPackageFileHeader is wrong Size (" + data.Length.ToString() + ")");
                }

                stream.Read(data, 0, data.Length);
                header = (DatabaseBigPackageFileHeader)data.BytesToStructure(typeof(DatabaseBigPackageFileHeader));

                if (header.Always3 != 3)
                {
                    throw new Exception("the value in the DBBF Header that is always 3 was not 3");
                }

                // Nab useful stuff
                this.Version = new Version(header.MajorVersion, header.MinorVersion);
                indexCount = header.IndexCount;
                indexOffset = header.IndexOffset;
                indexSize = header.IndexSize;
            }
            else
            {
                big = false;

                DatabasePackedFileHeader header;
                byte[] data = new byte[Marshal.SizeOf(typeof(DatabasePackedFileHeader))];

                if (data.Length != (96 - 4))
                {
                    throw new Exception("DatabasePackageFileHeader is wrong Size (" + data.Length.ToString() + ")");
                }

                stream.Read(data, 0, data.Length);
                header = (DatabasePackedFileHeader)data.BytesToStructure(typeof(DatabasePackedFileHeader));

                if (header.Always3 != 3)
                {
                    throw new Exception("the value in the DBPF Header that is always 3 was not 3");
                }

                // Nab useful stuff
                this.Version = new Version(header.MajorVersion, header.MinorVersion);
                indexCount = header.IndexCount;
                indexOffset = header.IndexOffset;
                indexSize = header.IndexSize;
            }

            this.Indices = new ObservableList<DatabaseIndex>();

            if (indexCount > 0)
            {
                // Read index
                stream.Seek(indexOffset, SeekOrigin.Begin);

                int indexHeaderValues = stream.ReadS32();
                if (indexHeaderValues < 4 || indexHeaderValues > 7)
                {
                    throw new InvalidDataException("don't know how to handle this index data");
                }

                uint indexTypeId = 0xCAFEBABE;
                // type id
                if ((indexHeaderValues & (1 << 0)) == 1 << 0)
                {
                    indexTypeId = stream.ReadU32();
                }

                uint indexGroupContainer = 0xCAFEBABE;
                // group id
                if ((indexHeaderValues & (1 << 1)) == 1 << 1)
                {
                    indexGroupContainer = stream.ReadU32();
                }

                uint indexUnknown = 0xCAFEBABE;
                // unknown value
                if ((indexHeaderValues & (1 << 2)) == 1 << 2)
                {
                    indexUnknown = stream.ReadU32();
                }

                for (int i = 0; i < indexCount; i++)
                {
                    DatabaseIndex index = new DatabaseIndex(this);
                    index.IsModified = false;

                    #region index.TypeId
                    if ((indexHeaderValues & (1 << 0)) == 1 << 0)
                    {
                        index.TypeId = indexTypeId;
                    }
                    else
                    {
                        index.TypeId = stream.ReadU32();
                    }
                    #endregion
                    #region index.GroupContainer
                    if ((indexHeaderValues & (1 << 1)) == 1 << 1)
                    {
                        index.GroupContainer = indexGroupContainer;
                    }
                    else
                    {
                        index.GroupContainer = stream.ReadU32();
                    }
                    #endregion
                    #region index.Unknown
                    if ((indexHeaderValues & (1 << 2)) == 1 << 2)
                    {
                        index.Unknown = indexUnknown;
                    }
                    else
                    {
                        index.Unknown = stream.ReadU32();
                    }
                    #endregion
                    index.InstanceId = stream.ReadU32();

                    if (big == true)
                    {
                        index.Offset = stream.ReadS64();
                    }
                    else
                    {
                        index.Offset = stream.ReadS32();
                    }


                    index.CompressedSize = stream.ReadU32() & ~0x80000000;
                    index.DecompressedSize = stream.ReadU32();
                    index.CompressedFlags = stream.ReadS16();
                    index.Flags = stream.ReadU16();
                    index.CheckCompressed();

                    this.Indices.Add(index);
                }
            }
        }

        private static byte[] StructureToBytes(object structure)
        {
            int size = Marshal.SizeOf(structure.GetType());
            byte[] data = new byte[size];
            GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            Marshal.StructureToPtr(structure, handle.AddrOfPinnedObject(), false);
            handle.Free();
            return data;
        }

        public void WriteHeader(Stream output, int indexOffset, int indexSize)
        {
            output.WriteASCII("DBPF");
            DatabasePackedFileHeader header = new DatabasePackedFileHeader();
            header.MajorVersion = this.Version.Major;
            header.MinorVersion = this.Version.Minor;
            header.Always3 = 3;
            header.IndexCount = this.Indices.Count(idx => idx.Deleted == false);
            header.IndexOffset = indexOffset;
            header.IndexSize = indexSize;

            byte[] data = StructureToBytes(header);
            output.Write(data, 0, data.Length);
        }

        public void WriteIndex(Stream output)
        {
            output.WriteU32(4); // index header values
            output.WriteU32(0); // unknown

            foreach (DatabaseIndex index in this.Indices)
            {
                if (index.Deleted == true)
                {
                    continue;
                }
                output.WriteU32(index.TypeId);
                output.WriteU32(index.GroupContainer);
                output.WriteU32(index.InstanceId);
                output.WriteS32((int)index.Offset);
                output.WriteU32(index.CompressedSize | 0x80000000);
                output.WriteU32(index.DecompressedSize);
                output.WriteS16(index.CompressedFlags);
                output.WriteU16(index.Flags);
            }
        }

        public static void WriteToPath(DatabaseIndex element, string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }
            using (FileStream fileStream = File.Create(path))
            {
                byte[] data = element.GetIndexData(true);
                fileStream.Write(data, 0, data.Length);
            }
        }
    }
}
