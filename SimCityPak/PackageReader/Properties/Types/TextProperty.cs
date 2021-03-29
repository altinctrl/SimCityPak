using System;
using System.IO;
using System.Text;
using Gibbed.Spore.Helpers;
using SimCityPak;
using System.Diagnostics;

namespace Gibbed.Spore.Properties
{
	[PropertyDefinition("text", "texts", 34)]
	public class TextProperty : ComplexProperty
	{
		public uint TableId;
		public uint InstanceId;
		public string PlaceholderText;

        public override string DisplayValue
        {
            get { return LocaleRegistry.Instance.GetLocalizedString(TableId, InstanceId); }
        }

		public override void ReadProp(Stream input, bool array)
		{
				this.TableId = input.ReadU32BE();
				this.InstanceId = input.ReadU32BE();
	
		}

		public override void WriteProp(Stream output, bool array)
		{
				output.WriteU32BE(this.TableId);
				output.WriteU32BE(this.InstanceId);
		}
	}
}
