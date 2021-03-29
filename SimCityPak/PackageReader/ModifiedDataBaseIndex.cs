using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gibbed.Spore.Properties;
using System.IO;

namespace Gibbed.Spore.Package
{
    public abstract class ModifiedDatabaseIndexData
    {
        public abstract byte[] GetData();
    }



    public class ModifiedTextFile : ModifiedDatabaseIndexData
    {
        public string Text { get; set; }

        public override byte[] GetData()
        {
            return Encoding.UTF8.GetBytes(Text);
        }
    }

     public class ModifiedPropertyFile : ModifiedDatabaseIndexData
    {
        public PropertyFile PropertyFile { get; set; }

        public override byte[] GetData()
        {
            if (PropertyFile != null)
            {
                using (MemoryStream strm = new MemoryStream())
                {
                    PropertyFile.Write(strm);
                    return strm.ToArray();
                }
            }
            return null;
        }
    }

     public class ModifiedRasterFile : ModifiedDatabaseIndexData
     {
         public byte[] ImageFileData { get; set; }

         public override byte[] GetData()
         {
             if (ImageFileData != null)
             {
                 return ImageFileData;
             }
             return null;
         }
     }

     public class ModifiedRawImage : ModifiedDatabaseIndexData
     {
         public byte[] ImageFileData { get; set; }

         public override byte[] GetData()
         {
             if (ImageFileData != null)
             {
                 return ImageFileData;
             }
             return null;
         }
     }

     public class ModifiedPngFile : ModifiedDatabaseIndexData
     {
         public byte[] ImageFileData { get; set; }

         public override byte[] GetData()
         {
             if (ImageFileData != null)
             {
                 return ImageFileData;
             }
             return null;
         }
     }

     public class ModifiedTgaFile : ModifiedDatabaseIndexData
     {
         public byte[] ImageFileData { get; set; }

         public override byte[] GetData()
         {
             if (ImageFileData != null)
             {
                 return ImageFileData;
             }
             return null;
         }
     }

     public class ModifiedRW4File : ModifiedDatabaseIndexData
     {
         public byte[] RW4FileData { get; set; }

         public override byte[] GetData()
         {
             if (RW4FileData != null)
             {
                 return RW4FileData;
             }
             return null;
         }
     }
     public class ModifiedGenericFile : ModifiedDatabaseIndexData
     {
         public byte[] FileData { get; set; }

         public override byte[] GetData()
         {
             if (FileData != null)
             {
                 return FileData;
             }
             return null;
         }
     }
}
