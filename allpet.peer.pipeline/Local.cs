using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace AllPet.Pipeline
{
    class PipelineRefLocal : IModulePipeline
    {
        public PipelineRefLocal(ISystemPipeline system, string userPath, string path, IModuleInstance actor)
        {
            this.system = system;
            if (string.IsNullOrEmpty(userPath))
                this.userUrl = null;
            else if (userPath[0] == '@')
                this.userUrl = userPath;
            else
                this.userUrl = "this/" + userPath;
            this.path = path;
            this.actorInstance = actor;
        }
        public IModuleInstance actorInstance;
        public string userUrl;
        public ISystemPipeline system
        {
            get;
            private set;
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
                var path = (system as PipelineSystemRefLocal).system.GetModulePath(actorInstance);
                bool bExist = string.IsNullOrEmpty(path) == false;
                if (bExist && actorInstance.HasDisposed == true)
                {
                    ((system as PipelineSystemRefLocal).system as PipelineSystemV1).UnRegistModule(path);
                    return false;
                }
                return !actorInstance.HasDisposed;
            }
        }

        public void Tell(byte[] data)
        {
            var _system = (system as PipelineSystemRefLocal).system;

            IModulePipeline pipeline = null;
            try
            {
                pipeline = userUrl == null ? null : _system.GetPipeline(actorInstance, userUrl);
            }
            catch
            {
                Console.WriteLine("error here.");
            }
            if (actorInstance.MultiThreadTell == true && actorInstance.Inited)
            {//直接开线程投递，不阻塞
                global::System.Threading.ThreadPool.QueueUserWorkItem((s) =>
                {
                    this.actorInstance.OnTell(pipeline, data);
                }
                );
            }
            else
            {
                //队列投递,不阻塞，队列在内部实现
                this.actorInstance.QueueTell(pipeline, data);
            }
        }
    }
    class PipelineSystemRefLocal : ISystemPipeline
    {
        public PipelineSystemRefLocal(ISystem system)
        {
            this.system = system;
        }
        public ISystem system;
        public bool IsLocal => true;

        public string remoteaddr => null;

        public bool linked => false;
    }
}
