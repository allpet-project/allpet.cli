﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace AllPet.Pipeline
{
    class PipelineRefLocal : IModuleRef
    {
        public PipelineRefLocal(ISystemRef system, string userPath, string path, IModuleInstance actor)
        {
            this.system = system;
            if (string.IsNullOrEmpty(userPath))
                this.userUrl = null;
            else
                this.userUrl = "this/"+ userPath;
            this.path = path;
            this.actorInstance = actor;
        }
        public IModuleInstance actorInstance;
        public string userUrl;
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
            var _system = (system as PipelineSystemRefLocal).system;

            var pipeline = userUrl == null ? null : _system.GetPipeline(actorInstance, userUrl);
            global::System.Threading.ThreadPool.QueueUserWorkItem((s) =>
            {
                this.actorInstance.OnTell(pipeline, data);
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
