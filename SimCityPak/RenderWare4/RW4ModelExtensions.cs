using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SporeMaster.RenderWare4
{
    public partial class RW4Model
    {

        public List<MaterialTextureReference> Materials
        {
            get
            {
                try
                {
                    RW4Material materialSection = Sections.First(s => s.TypeCode == SectionTypeCodes.Material).obj as RW4Material;
                    return materialSection.Materials;
                }
                catch
                {
                    return null;
                }
            }
        }

        public List<Texture> Textures
        {
            get
            {
                try
                {
                    List<Texture> texData = new List<Texture>();
                    foreach (RW4Section textureSection in Sections.Where(s => s.TypeCode == SectionTypeCodes.Texture).ToList<RW4Section>())
                    {
                        texData.Add(textureSection.obj as Texture);
                    }
                    return texData;
                }
                catch
                {
                    return null;
                }
            }


        }




    }
}
