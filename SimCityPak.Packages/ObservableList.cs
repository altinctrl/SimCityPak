using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;

namespace SimCityPak.PackageReader
{
    public class ObservableList<T> : List<T>, INotifyCollectionChanged
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public void Changed()
        {
            // lock so we can send these event from other threads (Importers will use this)
            lock (CollectionChanged) { CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset)); }
        }
    }
}
