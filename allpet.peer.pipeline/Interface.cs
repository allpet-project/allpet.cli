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
        bool IsVaild
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
        void QueueTell(IModulePipeline from, byte[] data);
    }
    public abstract class Module : IModuleInstance
    {
        public Module(bool MultiThreadTell = true)
        {
            this.MultiThreadTell = MultiThreadTell;
            this.HasDisposed = false;
        }
        public bool MultiThreadTell
        {
            get;
            private set;
        }
        public ISystem _System
        {
            get;
            private set;
        }
        private int _inited;
        public bool Inited //是否已经初始化
        {
            get
            {
                return _inited > 0;
            }
        }
        public bool HasDisposed
        {
            get;
            private set;
        }
        public IModulePipeline GetPipeline(string urlActor)
        {
            return _System.GetPipeline(this, urlActor);
        }
        public void OnRegistered(ISystem system)
        {
            this._System = system;
        }
        public virtual void Dispose()
        {
            this.HasDisposed = true;
        }
        public void OnStarted()
        {
            if (MultiThreadTell == false)//如果是单线程投递，不用管，有dequeueThread处理
            {

            }
            else
            {//此时
                DequeueThread();
            }

            global::System.Threading.Interlocked.Exchange(ref this._inited, 1);

        }
        class QueueObj
        {
            public IModulePipeline from;
            public byte[] data;
        }
        System.Collections.Concurrent.ConcurrentQueue<QueueObj> queueObj;
        public void QueueTell(IModulePipeline _from, byte[] _data)
        {
            if (queueObj == null)
            {
                queueObj = new System.Collections.Concurrent.ConcurrentQueue<QueueObj>();
                if (MultiThreadTell == false)//单线程投递则必须开一个线程去处理队列消息
                {
                    global::System.Threading.Thread t = new System.Threading.Thread(DequeueThread);
                    t.IsBackground = true;
                    t.Start();
                }
            }
            queueObj.Enqueue(new QueueObj() { data = _data, from = _from });
        }
        void DequeueThread()
        {
            if (queueObj == null)
                return;
            while (MultiThreadTell == false || queueObj.IsEmpty == false)
            {
                if (queueObj.TryDequeue(out QueueObj queueobj))
                {
                    this.OnTell(queueobj.from, queueobj.data);
                }
                System.Threading.Thread.Sleep(1);
            }
        }

        public abstract void OnStart();

        public abstract void OnTell(IModulePipeline from, byte[] data);
    }
}
