using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace AllPet.Pipeline
{
    //管线系统
    public interface IPipelineSystem : IDisposable
    {
        void Start();
        void Close();

        void OpenNetwork(AllPet.peer.tcp.PeerOption option);
        void CloseNetwork();
        /// <summary>
        /// Listen 端口，不是必须的
        /// </summary>
        /// <param name="host"></param>
        /// <param name="option"></param>
        void OpenListen(IPEndPoint host);
        void CloseListen();
        /// <summary>
        /// 连接到另一个ActorSystem，也不是必须的，GetActorRemote会自己去做这件事
        /// </summary>
        /// <param name="remote"></param>
        Task<ISystemRef> Connect(IPEndPoint remote);//一个system 可以连接到另外一个系统,
        ICollection<string> GetAllSystemsPath();
        ICollection<ISystemRef> GetAllSystems();

        IModuleRef GetPipeline(IModuleInstance user, string urlActor);

        void RegistPipeline(string path, IModuleInstance actor);
        string GetPipelinePath(IModuleInstance actor);

        ICollection<string> GetAllPipelinePath();
        void UnRegistPipeline(string path);
    }

    public interface ISystemRef
    {
        bool IsLocal
        {
            get;
        }
        string remoteaddr
        {
            get;
        }
        bool linked
        {
            get;
        }
    }
    //连接到的actor
    public interface IModuleRef
    {
        ISystemRef system
        {
            get;
        }
        string path
        {
            get;
        }
        void Tell(byte[] data);
        bool vaild
        {
            get;
        }
    }
    public interface IModuleInstance
    {
        IPipelineSystem system
        {
            get;
        }
        IModuleRef GetPipeline(string urlActor);
        void OnStart();
        void OnTell(IModuleRef from, byte[] data);
    }
    public abstract class Pipeline : IModuleInstance
    {
        public Pipeline(IPipelineSystem system)
        {
            this.system = system;
        }
        public IPipelineSystem system
        {
            get;
            private set;
        }
        public IModuleRef GetPipeline(string urlActor)
        {
            return system.GetPipeline(this, urlActor);
        }
        public virtual void OnStart()
        {

        }
        public abstract void OnTell(IModuleRef from, byte[] data);
    }
}
