using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace AllPet.Pipeline
{
    //管线系统
    public interface ISystem : IDisposable
    {
        void Start();

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
        Task<ISystemPipeline> Connect(IPEndPoint remote);//一个system 可以连接到另外一个系统,
        ICollection<string> GetAllSystemsPath();
        ICollection<ISystemPipeline> GetAllSystems();

        IModulePipeline GetPipeline(IModuleInstance user, string urlActor);

        void RegistModule(string path, IModuleInstance actor);
        string GetModulePath(IModuleInstance actor);

        ICollection<string> GetAllPipelinePath();
        //void UnRegistModule(string path);
    }

    public interface ISystemPipeline
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
    public interface IModulePipeline
    {
        ISystemPipeline system
        {
            get;
        }
        string path
        {
            get;
        }
        void Tell(byte[] data);
        void TellLocalObj(object obj);
        bool IsVaild
        {
            get;
        }
        bool IsLocal
        {
            get;
        }
    }

    public interface IModuleInstance : IDisposable
    {
        ISystem _System
        {
            get;
        }
        bool MultiThreadTell
        {
            get;
        }
        bool Inited //是否已经初始化
        {
            get;
        }
        bool HasDisposed
        {
            get;
        }
        IModulePipeline GetPipeline(string urlActor);
        void OnRegistered(ISystem system);
        void OnStart();
        void OnStarted();
        void OnTell(IModulePipeline from, byte[] data);
        void OnTellLocalObj(IModulePipeline from, object obj);
        void QueueTell(IModulePipeline from, byte[] data);
        void QueueTellLocalObj(IModulePipeline from, object obj);
    }
}
