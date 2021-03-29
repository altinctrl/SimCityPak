using System;
using System.IO;
using Gibbed.Spore.Helpers;
using SimCityPak;
using System.Diagnostics;
//using SporeMaster;

namespace Gibbed.Spore.Properties
{
    [PropertyDefinition("Key", "keys", 32, "viewKeyProperty", "viewEditKeyProperty")]
    public class KeyProperty : Property
    {
        private uint _typeId;
        public uint TypeId
        { 
            get
            {
                return _typeId;
            }
            set
            {
                _typeId = value;
                OnPropertyChanged(new EventArgs());
            }
        }

        private uint _groupContainer;
        public uint GroupContainer
        {
            get
            {
                return _groupContainer;
            }
            set
            {
                _groupContainer = value;
                OnPropertyChanged(new EventArgs());
            }
        }

        private uint _instanceId;
        public uint InstanceId
        {
            get
            {
                return _instanceId;
            }
            set
            {
                _instanceId = value;
                OnPropertyChanged(new EventArgs());
            }
        }        

        public override string DisplayValue
        {
            get
            {
                return string.Empty;
               // return string.Format("Type: {0} ({3:x}) - G {1:x}-I {2:x}", TGIRegistry.Instance.Instances.GetAbbreviation(TypeId), GroupContainer, TGIRegistry.Instance.Instances.GetName(InstanceId), TypeId);
            }
        }

        public string TypeName
        {
            get
            {
                return string.Empty;
                //return string.Format("{0} ({1})", TGIRegistry.Instance.Instances.GetName(TypeId), TypeId.ToHex());
            }
        }

        public string InstanceName
        {
            get
            {
                return string.Empty;
                //return TGIRegistry.Instance.Instances.GetName(InstanceId);
            }
        }

        public string GroupName
        {
            get
            {
                return string.Empty;
               // return TGIRegistry.Instance.Instances.GetName(GroupContainer);
            }
        }

        public override void OnPropertyChanged(EventArgs e)
        {
            base.OnPropertyChanged(e);
        }

        public override void ReadProp(Stream input, bool array)
        {
            this._instanceId = input.ReadU32BE();
            this._typeId = input.ReadU32BE();
            this._groupContainer = input.ReadU32BE();
        }

        public override void WriteProp(Stream output, bool array)
        {
            output.WriteU32BE(this._instanceId);
            output.WriteU32BE(this._typeId);
            output.WriteU32BE(this._groupContainer);
        }
    }
}
