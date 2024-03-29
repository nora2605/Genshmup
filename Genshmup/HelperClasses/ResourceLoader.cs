﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Genshmup.HelperClasses
{
    public static class ResourceLoader
    {
        private static readonly PrivateFontCollection _privateFontCollection;

        static ResourceLoader()
        {
            _privateFontCollection = new PrivateFontCollection();
        }

        public static Stream? LoadResource(Assembly? a, string searchstring)
        {
            if (a == null) a = Assembly.GetExecutingAssembly();
            return a.GetManifestResourceStream(a.GetManifestResourceNames().ToList().Find(val => val.Contains("." + searchstring)) ?? "plc");
        }

        public static FontFamily? LoadFont(Assembly? a, string name)
        {
            if (a == null) a = Assembly.GetExecutingAssembly();
            Stream? st = a.GetManifestResourceStream(a.GetManifestResourceNames().ToList().Find(val => val.Contains("." + name)) ?? "plc");
            if (st == null) return null;
            List<byte> bytes = new();
            while (st.Position < st.Length)
            {
                int j = st.ReadByte();
                if (j == -1) break;
                else bytes.Add((byte)j);
            }
            byte[] fontBytes = bytes.ToArray();
            var handle = GCHandle.Alloc(fontBytes, GCHandleType.Pinned);
            IntPtr pointer = handle.AddrOfPinnedObject();
            try
            {
                _privateFontCollection.AddMemoryFont(pointer, fontBytes.Length);
            }
            finally
            {
                handle.Free();
            }
            return _privateFontCollection.Families.First();
        }
    }
}
