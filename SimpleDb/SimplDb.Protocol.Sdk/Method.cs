using System;
using System.Collections.Generic;
using System.Text;

namespace SimplDb.Protocol.Sdk
{
    public enum Method
    {
        CreateTable=0x00,
        DeleteTable = 0x01,
        GetDirect = 0x02,
        GetUint64 = 0x03,
        PutDirect = 0x04,
        PutUint64=0x08,
        Delete=0x10,        
    }
}
