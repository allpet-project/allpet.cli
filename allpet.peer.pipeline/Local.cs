using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace AllPet.Pipeline
{
    class PipelineRefLocal : IPipelineRef
    {
        public PipelineRefLocal(ISystemRef system, string path, IPipelineInstance actor)
        {
            this.system = system;
            this.path = path;
            this.actorInstance = actor;
        }
        public IPipelineInstance actorInstance;

        public ISystemRef system
        {
            get;
            private set;
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
                return (system as PipelineSystemRefLocal).system.GetPipelinePath(actorInstance) != null;
            }
        }

        public void Tell(byte[] data)
        {
            global::System.Threading.ThreadPool.QueueUserWorkItem((s) =>
            {
                this.actorInstance.OnTell(null, data);
            }
            );
        }
    }
    class PipelineSystemRefLocal : ISystemRef
    {
        public PipelineSystemRefLocal(IPipelineSystem system)
        {
            this.system = system;
        }
        public IPipelineSystem system;
        public bool IsLocal => true;

        public string remoteaddr => null;

        public bool linked => false;
    }
}
