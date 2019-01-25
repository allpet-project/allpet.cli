using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace SimplDb.Protocol.Sdk.Message
{
    [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct DeleteTableCommand:ICommand
    {
        public byte[] TableId;
    }
}
