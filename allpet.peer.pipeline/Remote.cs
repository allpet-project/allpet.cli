using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace AllPet.Pipeline
{
    class RefSystemRemote : ISystemPipeline
    {
        public AllPet.peer.tcp.IPeer peer;
        public UInt64 peerid;
        PipelineSystemV1 _System;
        global::System.Collections.Concurrent.ConcurrentDictionary<string, IModulePipeline> refPipelines;
        public RefSystemRemote(PipelineSystemV1 system, AllPet.peer.tcp.IPeer peer, string remoteaddr, UInt64 id)
        {
            this._System = system;
            this.peer = peer;
            this.peerid = id;
            this.remoteaddr = remoteaddr;
            refPipelines = new System.Collections.Concurrent.ConcurrentDictionary<string, IModulePipeline>();
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

        public event Action OnClose;
        public void Close()
        {
            this.linked = false;
            foreach (var pipe in this.refPipelines)
            {

            }
            this?.OnClose();
        }
        public IModulePipeline GetPipeline(IModuleInstance user, string path)
        {
            var pipestr = this.remoteaddr + "/" + path + "_" + user.path;
            if (this.refPipelines.TryGetValue(pipestr, out IModulePipeline pipe))
            {
                return pipe;
            }
            PipelineRefRemote _pipe = new PipelineRefRemote(_System.refSystemThis, user.path, this, path);
            this.refPipelines[pipestr] = _pipe;

            return _pipe;
        }

        public IModulePipeline GetPipeLineByFrom(IModulePipeline from, IModuleInstance to)
        {
            throw new NotImplementedException("all GetPipeline By From is To Local");
        }
    }
    class PipelineRefRemote : IModulePipeline
    {
        public PipelineRefRemote(ISystemPipeline usersystem, string userPath, RefSystemRemote remotesystem, string path)
        {
            this._usersystem = usersystem;
            this.userpath = userPath;

            this._remotesystem = remotesystem;
            this.path = path;
        }

        RefSystemRemote _remotesystem;
        ISystemPipeline _usersystem;
        string userpath;
        public ISystemPipeline system
        {
            get
            {
                return _remotesystem;
            }
        }

        public string path
        {
            get;
            private set;
        }

        public bool IsVaild
        {
            get
            {
                return system.linked;
            }
        }
        public bool IsLocal => false;
        byte[] GetFromBytes()
        {
            return System.Text.Encoding.UTF8.GetBytes(this.userpath);
        }
        byte[] GetToBytes()
        {
            return System.Text.Encoding.UTF8.GetBytes(this.path);
        }
        public unsafe void Tell(byte[] data)
        {
            if (data.Length == 0)
                throw new Exception("do not support  zero length bytearray.");

            byte[] from = GetFromBytes();
            byte[] to = GetToBytes();
            byte[] outbuf = new byte[from.Length + 1 + to.Length + 1 + data.Length];
            fixed (byte* pdiao = outbuf, pfrom = from, pto = to, pdata = data)
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
            _remotesystem.peer.Send(_remotesystem.peerid, outbuf);
        }
        public void TellLocalObj(object obj)
        {
            throw new Exception("not support to telllocal obj on remote pipeline.");
        }
    }
}
