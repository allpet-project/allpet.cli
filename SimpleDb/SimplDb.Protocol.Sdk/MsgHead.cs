using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace SimplDb.Protocol.Sdk
{
    [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct MsgHead
    {
        public byte Version;
        public ushort Method;
        public int Len;
    }
}
