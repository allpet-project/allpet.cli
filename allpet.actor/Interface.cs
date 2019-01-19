using System;
using System.Collections.Generic;

namespace allpet.actor
{
    //Actor实例的容器
    public interface IActorInstanceContainer
    {
        void AddActor(string path, IActorInstance actor);
        void CloseActor(string path);
        ICollection<string> GetAllActorPath();
        IActorInstance GetActor(string path);
    }
    public interface IActorServer : IActorInstanceContainer
    {
        void Start(AllPet.peer.tcp.PeerOption option);
        void Close();
    }

    //實際Actor,接收事件
    public interface IActorInstance
    {
        string Path
        {
            get;
        }
        IActorInstanceContainer container
        {
            get;
        }

        void OnCreate(IActorInstanceContainer container);
        void OnClose();

        void OnTell(IActorLink from, byte[] data);

        IActorLink LinkTo(string addrActor);
        void OnLink(IActorLink link);
        void OnLinkFail(string addr, Exception error);
    }
    //ActorLink
    public interface IActorLink
    {
        IActorInstance From
        {
            get;
        }
        string TargetPath
        {
            get;
        }
        bool IsVaild
        {
            get;
        }
        void Tell(byte[] data);
    }

}
