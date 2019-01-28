using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace AllPet.Pipeline
{
    class PipelineRefLocal : IModulePipeline
    {
        public PipelineRefLocal(ISystemPipeline system, string userPath, string pathModule, IModuleInstance module)
        {
            this.system = system;
            if (string.IsNullOrEmpty(userPath))
                this.userUrl = null;
            else if (userPath[0] == '@')
                this.userUrl = userPath;
            else
                this.userUrl = "this/" + userPath;

            var _system = (system as PipelineSystemRefLocal).system;
            try
            {
                fromPipeline = userUrl == null ? null : _system.GetPipeline(targetModule, userUrl);
            }
            catch
            {
                Console.WriteLine("error here.");
            }

            this.path = pathModule;
            this.targetModule = module;
        }

        IModulePipeline fromPipeline;
        //指向的模块
        public IModuleInstance targetModule;
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
                var path = (system as PipelineSystemRefLocal).system.GetModulePath(targetModule);
                bool bExist = string.IsNullOrEmpty(path) == false;
                if (bExist && targetModule.HasDisposed == true)
                {
                    ((system as PipelineSystemRefLocal).system as PipelineSystemV1).UnRegistModule(path);
                    return false;
                }
                return !targetModule.HasDisposed;
            }
        }
        public bool IsLocal => true;

        public void TellDirect(byte[] data)
        {
            this.targetModule.OnTell(fromPipeline, data);

        }
        public void Tell(byte[] data)
        {
            if (data.Length == 0)
                throw new Exception("do not support  zero length bytearray.");

            if (targetModule.MultiThreadTell == true && targetModule.Inited)
            {//直接开线程投递，不阻塞
                global::System.Threading.ThreadPool.QueueUserWorkItem((s) =>
                {
                    this.targetModule.OnTell(fromPipeline, data);
                }
                );
            }
            else
            {
                //队列投递,不阻塞，队列在内部实现
                this.targetModule.QueueTell(fromPipeline, data);
            }
        }
        public void TellLocalObj(object obj)
        {
            if (obj == null)
                throw new Exception("do not support  null.");

            if (targetModule.MultiThreadTell == true && targetModule.Inited)
            {//直接开线程投递，不阻塞
                global::System.Threading.ThreadPool.QueueUserWorkItem((s) =>
                {
                    this.targetModule.OnTellLocalObj(fromPipeline, obj);
                }
                );
            }
            else
            {
                //队列投递,不阻塞，队列在内部实现
                this.targetModule.QueueTellLocalObj(fromPipeline, obj);
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
