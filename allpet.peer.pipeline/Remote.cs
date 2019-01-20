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

        public void Tell(byte[] data)
        {
            _system.peer.Send(_system.peerid, data);
        }
    }
}
