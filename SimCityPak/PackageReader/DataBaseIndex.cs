using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimCityPak;
using System.IO;
using Gibbed.Spore.Helpers;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SimCityPak.PackageReader;

namespace Gibbed.Spore.Package
{
    public class DatabaseIndex : IComparable<DatabaseIndex>, INotifyPropertyChanged
    {
        #region DataBaseIndex
        public DatabaseIndex(DatabasePackedFile owner)
        {
            this.Owner = owner;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName = "")
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public DatabasePackedFile Owner { get; private set; }
        public bool Compressed { get; set; }

        private uint _typeId;
        public uint TypeId { get { return _typeId; } set { if (value != _typeId) { _typeId = value; NotifyPropertyChanged("TypeId"); NotifyPropertyChanged("TypeName"); } } }

        private uint _groupContainer; // GroupContainer can be divided in several parts:
        public uint GroupContainer { get { return ModifiedGroupId.HasValue ? ModifiedGroupId.Value : _groupContainer; ; } set { if (value != _groupContainer) { _groupContainer = value; NotifyPropertyChanged("GroupContainer"); NotifyPropertyChanged("GroupName"); } } }
        public uint GroupId { get { return ((_groupContainer >> (3 * 8)) & 0xff); } } // mask XX000000
        public uint GroupType { get { return ((_groupContainer >> (2 * 8)) & 0xff); } } // mask 00XX0000
        public uint InstanceType { get { return (_groupContainer & 0xffff); } } // mask 0000XXXX

        private uint _instanceId;
        public uint InstanceId
        {
            get
            {
                return ModifiedInstanceId.HasValue ? ModifiedInstanceId.Value : _instanceId;
            }
            set
            {
                if (value != _instanceId)
                {
                    _instanceId = value; NotifyPropertyChanged("InstanceId");
                    this.InstanceName = TGIRegistry.Instance.Instances.GetName(this.InstanceId);
                }
            }
        }

        private bool _deleted;
        public bool Deleted { get { return _deleted; } set { if (value != _deleted) { _deleted = value; NotifyPropertyChanged("Deleted"); } } }

        private uint? _modifiedInstanceId;
        public uint? ModifiedInstanceId
        {
            get { return _modifiedInstanceId; }
            set
            {
                this._modifiedInstanceId = value;
                this.InstanceName = TGIRegistry.Instance.Instances.GetName(value.Value);
            }

        }

        private uint? _modifiedGroupId;
        public uint? ModifiedGroupId
        {
            get { return _modifiedGroupId; }
            set
            {
                this._modifiedGroupId = value;
            }
        }


        public uint? ModifiedTypeId { get; set; }

        /// <summary>
        /// Returns a display string for the Type
        /// </summary>
        public string TypeName
        {
            get
            {
                if ((TypeIds)TypeId == TypeIds.PropertyFile)
                {
                    return string.Format("{0} ({1})", TGIRegistry.Instance.FileTypes.GetName(TypeId), ((PropertyFileTypeIds)InstanceType).ToString());
                }
                else
                {
                    return string.Format("{0}", TGIRegistry.Instance.FileTypes.GetName(TypeId), TypeId.ToHex());
                }
            }
        }


        public uint IconSort
        {
            get
            {
                return TypeId;
            }

        }

        /// <summary>
        /// Returns a display string for the Group
        /// </summary>
        public string GroupName
        {
            get
            {
                return string.Format("{0} ({1})", TGIRegistry.Instance.Groups.GetName(GroupId), GroupContainer.ToHex());
            }
        }

        /// <summary>
        /// Returns a display string for the Instance
        /// </summary>
        private string _instanceName;
        public string InstanceName
        {
            get
            {
                return _instanceName;
            }
            set
            {
                if (value != _instanceName)
                {
                    _instanceName = value;
                    NotifyPropertyChanged("InstanceName");
                }
            }
        }

        public string InstanceFullName
        {
            get
            {
                return string.Format("{0} (T: 0x{1:X8} G: 0x{2:X8} I: 0x{3:X8})", _instanceName, _typeId, _groupContainer, _instanceId);
            }
        }


        public Int64 Offset { get; set; }
        public uint CompressedSize { get; set; }
        public uint DecompressedSize { get; set; }
        public short CompressedFlags { get; set; }
        public ushort Flags { get; set; }
        public uint Unknown { get; set; }

        private bool _isModified;
        public bool IsModified
        {
            get { return _isModified; }
            set
            {
                if (value != _isModified)
                {
                    _isModified = value;
                    NotifyPropertyChanged("IsModified");
                }
            }
        }
        public ModifiedDatabaseIndexData ModifiedData { get; set; }

        public byte[] GetIndexData(bool decompress)
        {
            byte[] data;

            if (ModifiedData != null)
            {
                data = ModifiedData.GetData();
            }
            else
            {
                data = Owner.GetData((uint)Offset, Compressed ? (int)CompressedSize : (int)DecompressedSize);
            }

            if (Compressed && decompress)
            {
                data = StreamHelpers.RefPackDecompress(new MemoryStream(data), CompressedSize, DecompressedSize);
            }

            return data;
        }

        public void CheckCompressed()
        {
            if (this.CompressedFlags != 0 && this.CompressedFlags != -1)
            {
                throw new InvalidDataException("compressed flags");
            }
            this.Compressed = this.CompressedFlags == -1 ? true : false;
        }


        public override string ToString()
        {
            return base.ToString() + ": " + this.TypeId.ToString("X8") + ", " + this.GroupContainer.ToString("X8") + " @ " + this.InstanceId.ToString("X8");
        }

        public int CompareTo(DatabaseIndex other)
        {
            if (TypeId == other.TypeId)
            {
                if (GroupContainer == other.GroupContainer)
                {
                    return Math.Sign(InstanceId - other.InstanceId);
                }
                else
                {
                    return Math.Sign(GroupContainer - other.GroupContainer);
                }
            }
            else
            {
                return Math.Sign(TypeId - other.TypeId);
            }
        }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            DatabaseIndex di = (DatabaseIndex)obj;
            return TypeId == di.TypeId &&
                   GroupContainer == di.GroupContainer &&
                   InstanceId == di.InstanceId;
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return TypeId.GetHashCode() ^
                   GroupContainer.GetHashCode() ^
                   InstanceId.GetHashCode();
        }
        public string GetExportFileName()
        {
            String name = "SCP_";
            if (!this.InstanceName.Equals(this.InstanceId.ToHex()))
            {
                name += this.InstanceName + "_";
            }
            name += this.TypeId.ToHex() + "-" + this.GroupContainer.ToHex() + "-" + this.InstanceId.ToHex();
            return Path.ChangeExtension(name, TGIRegistry.Instance.FileTypes.GetAbbreviation(this.TypeId).ToLower());
        }
        #endregion

        #region Utilities

        public static DatabaseIndex FindIndex(uint TypeId, uint? GroupId, uint? GroupType, uint InstanceId, bool searchSimCityData, bool searchEcoGame)
        {
            if (DatabaseManager.Instance.Indices.Exists(i => i.InstanceId == InstanceId && i.TypeId == TypeId && ((GroupId == null && GroupType == null) || i.GroupContainer == GroupId || i.GroupType == GroupType)))
            {
                return DatabaseManager.Instance.Indices.First(i => i.InstanceId == InstanceId && i.TypeId == TypeId && ((GroupId == null && GroupType == null) || i.GroupContainer == GroupId || i.GroupType == GroupType));
            }
            if (searchSimCityData)
            {
                string folderDataPath = SimCityPak.Properties.Settings.Default.SimCityFolder + @"\SimCityData\";
                DirectoryInfo folderData = new DirectoryInfo(folderDataPath);
                foreach (string scriptsPackage in SimCityPackages.SimCityDefaultPackages)
                {
                    FileInfo[] scriptsFiles = folderData.GetFiles(scriptsPackage, SearchOption.TopDirectoryOnly);
                    FileInfo scriptsFile = scriptsFiles.First(sf => sf.LastWriteTime == scriptsFiles.Max(sf2 => sf2.LastWriteTime));
                    DatabasePackedFile ScriptsPackage = DatabasePackedFile.LoadFromFile(scriptsFile.FullName);
                    if (ScriptsPackage.Indices.Exists(i => i.InstanceId == InstanceId && i.TypeId == TypeId && ((GroupId == null && GroupType == null) || i.GroupContainer == GroupId || i.GroupType == GroupType)))
                    {
                        return ScriptsPackage.Indices.First(i => i.InstanceId == InstanceId && i.TypeId == TypeId && ((GroupId == null && GroupType == null) || i.GroupContainer == GroupId || i.GroupType == GroupType));
                    }
                }
            }
            if(searchEcoGame)
            {

                string folderEcoGamePath = SimCityPak.Properties.Settings.Default.SimCityFolder + @"\SimCityUserData\EcoGame\";
                DirectoryInfo folderEcoGame = new DirectoryInfo(folderEcoGamePath);
                foreach (string scriptsPackage in SimCityPackages.SimCityScriptsPackages)
                {
                    //search the simcity data packages for this type/instance group id. if it doesn't exist, find it in the default simcity packages
                    FileInfo[] scriptsFiles = folderEcoGame.GetFiles(scriptsPackage, SearchOption.TopDirectoryOnly);
                    FileInfo scriptsFile = scriptsFiles.First(sf => sf.LastWriteTime == scriptsFiles.Max(sf2 => sf2.LastWriteTime));
                    DatabasePackedFile ScriptsPackage = DatabasePackedFile.LoadFromFile(scriptsFile.FullName);
                    if (ScriptsPackage.Indices.Exists(i => i.InstanceId == InstanceId && i.TypeId == TypeId && ((GroupId == null && GroupType == null) || i.GroupContainer == GroupId || i.GroupType == GroupType)))
                    {
                        return ScriptsPackage.Indices.First(i => i.InstanceId == InstanceId && i.TypeId == TypeId && ((GroupId == null && GroupType == null) || i.GroupContainer == GroupId || i.GroupType == GroupType));
                    }
                }
                
            }
        



            return null;
        }

        #endregion      


    }
        
}
