using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace AllPet.Pipeline
{
    class RefSystemRemote : ISystemRef
    {
        public AllPet.peer.tcp.IPeer peer;
        public UInt64 peerid;
        public RefSystemRemote(AllPet.peer.tcp.IPeer peer, string remoteaddr, UInt64 id)
        {
            this.peer = peer;
            this.peerid = id;
            this.remoteaddr = remoteaddr;
        }
        public bool IsLocal => false;

        public string remoteaddr
        {
            get;
            private set;
        }

        public bool linked
        {
            get;
            set;
        }
    }
    class PipelineRefRemote : IPipelineRef
    {
        public PipelineRefRemote(RefSystemRemote system, string path)
        {
            this._system = system;
            this.path = path;
        }

        RefSystemRemote _system;
        public ISystemRef system
        {
            get
            {
                return _system;
            }
        }

        public string path
        {
            get;
            private set;
        }

        public bool vaild
        {
            get
            {
                return system.linked;
            }
        }
        byte[] GetFromBytes()
        {
            return null;
        }
        byte[] GetToBytes()
        {
            return System.Text.Encoding.UTF8.GetBytes(this.path);
        }
        public unsafe void Tell(byte[] data)
        {
            byte[] from = GetFromBytes();
            byte[] to = GetToBytes();
            byte[] outbuf = new byte[from.Length + 1 + to.Length + 1 + data.Length];
            fixed (byte* pdiao = outbuf,pfrom=from,pto=to,pdata=data)
            {
                int seek = 0;
                outbuf[seek] = (byte)from.Length;
                seek++;

                Buffer.MemoryCopy(pfrom, pdiao + seek, from.Length, from.Length);
                seek += from.Length;

                outbuf[seek] = (byte)to.Length;
                seek++;

                Buffer.MemoryCopy(pto, pdiao + seek, to.Length, to.Length);
                seek += to.Length;

                Buffer.MemoryCopy(pdata, pdiao + seek, data.Length, data.Length);

            }
            _system.peer.Send(_system.peerid, outbuf);
        }
    }
}
