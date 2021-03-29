using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Gibbed.Spore.Helpers;
using System.Diagnostics;
using SimCityPak;
using System.Globalization;

namespace SporeMaster.RenderWare4
{
    public class Buffer<T> : RW4Object, IEnumerable<T> where T : IRW4Struct, new()
    {
        public const uint type_code = 0x10030;
        static uint Tsize = (new T()).Size();
        T[] items;
        public Buffer(int size) { items = new T[size]; }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return ((IEnumerable<T>)this).GetEnumerator(); }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            foreach (var t in items)
                yield return t;
        }
        public T this[int index]
        {
            get { return items[index]; }
            set { items[index] = value; }
        }
        public int Length
        {
            get { return items.Length; }
            set
            {
                var v = new T[value];
                for (int i = 0; i < value && i < items.Length; i++)
                    v[i] = items[i];
                items = v;
            }
        }
        public void Assign(IList<T> data)
        {
            items = data.ToArray();
        }
        public override int ComputeSize() { return (int)(items[0].Size() * items.Length); }
        public override void Read(RW4Model m, RW4Section s, Stream r)
        {
            if (s.type_code != type_code) throw new ModelFormatException(r, "VB000 Bad type code", s.type_code);
            for (int i = 0; i < items.Length; i++)
                items[i].Read(r);
        }
        public override void Write(RW4Model m, RW4Section s, Stream w)
        {
            for (int i = 0; i < items.Length; i++)
                items[i].Write(w);
        }
    };


    public class VertexBuffer : RW4Object, IEnumerable<Vertex>
    {
        public const uint type_code = 0x10030;
        static uint Tsize = (new Vertex()).Size();
        Vertex[] items;
        public VertexBuffer(int size) { items = new Vertex[size]; }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return ((IEnumerable<Vertex>)this).GetEnumerator(); }

        IEnumerator<Vertex> IEnumerable<Vertex>.GetEnumerator()
        {
            foreach (var t in items)
                yield return t;
       
        }

        public void ExpandBy(int i)
        {
            Array.Resize(ref items, items.Length + i);
        }

        public Vertex this[int index]
        {
            get { return items[index]; }
            set { items[index] = value; }
        }
        public int Length
        {
            get { return items.Length; }
            set
            {
                var v = new Vertex[value];
                for (int i = 0; i < value && i < items.Length; i++)
                    v[i] = items[i];
                items = v;
            }
        }
        public void Assign(IList<Vertex> data)
        {
            items = data.ToArray();
        }
        public override int ComputeSize() { return (int)(items[0].Size() * items.Length); }
     
        public override void Read(RW4Model m, RW4Section s, Stream r)
        {
            VertexFormat vFormat = null;
            if(m.Sections.Exists(sc => sc.TypeCode == SectionTypeCodes.VertexFormat))
            {
                vFormat = m.Sections.Single(sc => sc.TypeCode == SectionTypeCodes.VertexFormat).obj as VertexFormat;
            }
            if (s.type_code != type_code) throw new ModelFormatException(r, "VB000 Bad type code", s.type_code);
            {
                for (int i = 0; i < items.Length; i++)
                {
                    items[i].Read(r, vFormat);
                }
            }
        }
        public override void Write(RW4Model m, RW4Section s, Stream w)
        {
            for (int i = 0; i < items.Length; i++)
                items[i].Write(w);
        }
    };
}
