using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gibbed.Spore.Package;
using System.Windows.Data;
using System.Collections.ObjectModel;

namespace SimCityPak.PackageReader
{
    public class DatabaseManager
    {
        public ObservableList<DatabaseIndex> Indices { get; private set; }
        public ObservableList<DatabasePackedFile> PackageFiles { get; private set; }
        public List<uint> LoadedInstanceIds { get; private set; } // cache to speed up comboboxes
        public List<uint> LoadedGroupIds { get; private set; } // should fix this properly sometime..
        public List<uint> LoadedFileTypeIds { get; private set; }

        private static DatabaseManager _instance;
        public static DatabaseManager Instance
        {
            get
            {
                if (_instance != null)
                {
                    return _instance;
                }
                else
                {
                    _instance = Create();
                    return _instance;
                }

            }
            set { _instance = value; }
        }

        public static DatabaseManager Create()
        {
            return new DatabaseManager();

        }

        private DatabaseManager()
        {
            Indices = new ObservableList<DatabaseIndex>();
            PackageFiles = new ObservableList<DatabasePackedFile>();
            LoadedInstanceIds = new List<uint>();
            LoadedGroupIds = new List<uint>();
            LoadedFileTypeIds = new List<uint>();

            TGIRegistry.Instance.Instances.RegistryChanged += InstanceRegistryChanged;

            //IndexView = new ListCollectionView(Indices);
            //PackageView = new ListCollectionView(PackageFiles);
        }

        private void InstanceRegistryChanged(object sender, TGIRecord record)
        {
            foreach (DatabaseIndex i in Indices)
            {
                if (record.Id == i.InstanceId)
                {
                    i.InstanceName = record.DisplayName;
                }
            }
            Indices.Changed();
        }

        public void LoadPackage(string file)
        {
            load(file);
            Update();
        }

        public void LoadPackages(string[] files)
        {
            foreach (string f in files)
            {
                load(f);
            }

            Update();
        }

        public void ClosePackage(DatabasePackedFile package)
        {
            if (PackageFiles.Remove(package))
            {
                Update();
            }
        }

        public void CloseAll()
        {
            PackageFiles.Clear();
            Update();
        }

        public DatabaseIndex Find(Predicate<DatabaseIndex> pred)
        {
            return Indices.Find(pred);
        }

        public IEnumerable<DatabaseIndex> Where(Func<DatabaseIndex, bool> pred)
        {
            return Indices.Where(pred);
        }

        private void load(string file)
        {
            //see if the package is already loaded
            foreach (DatabasePackedFile packed in PackageFiles)
            {
                if (packed.packageFileInfo.FullName.Equals(file))
                {
                    return;
                }
            }

            DatabasePackedFile package = DatabasePackedFile.LoadFromFile(file);
            package.Indices.CollectionChanged += PackageIndicesChanged;
            
            PackageFiles.Add(package);
        }

        private void PackageIndicesChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Update();
        }

        private void Update()
        {
            Indices.Clear();
            LoadedInstanceIds.Clear();
            LoadedGroupIds.Clear();
            LoadedFileTypeIds.Clear();

            foreach (DatabasePackedFile packed in PackageFiles)
            {
                Indices.AddRange(packed.Indices);
            }

            foreach (DatabaseIndex index in Indices)
            {
                if (!LoadedInstanceIds.Contains(index.InstanceId))
                {
                    LoadedInstanceIds.Add(index.InstanceId);
                }

                if (!LoadedGroupIds.Contains(index.GroupId))
                {
                    LoadedGroupIds.Add(index.GroupId);
                }

                if (!LoadedFileTypeIds.Contains(index.TypeId))
                {
                    LoadedFileTypeIds.Add(index.TypeId);
                }
            }

            Indices.Changed();
            PackageFiles.Changed();
        }
    }
}
