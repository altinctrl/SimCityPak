using System;
using System.Collections.Generic;
using System.IO;

namespace Gibbed.Spore.Properties
{
    [PropertyDefinitionAttribute("array", "arrays", 99)]
	public class ArrayProperty : Property
	{
		public Type PropertyType;
		public List<Property> Values = new List<Property>();

         public override string DisplayValue
        {
            get {  string retVal = string.Empty;
                foreach (Property p in Values)
                {
                    retVal += " " +  p.DisplayValue;
                 
                }
                return retVal; }
        }

		public override void ReadProp(Stream input, bool array)
		{
			throw new NotImplementedException();
		}

		public override void WriteProp(Stream output, bool array)
		{
			throw new NotImplementedException();
		}
	}
}
