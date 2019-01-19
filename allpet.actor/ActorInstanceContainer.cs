using System;
using System.Collections.Generic;
using System.Text;

namespace allpet.actor
{
    public class ActorInstanceContainer : IActorInstanceContainer
    {
        System.Collections.Concurrent.ConcurrentDictionary<string, IActorInstance> instance;
        public ActorInstanceContainer()
        {
            instance = new System.Collections.Concurrent.ConcurrentDictionary<string, IActorInstance>();
        }

        //这个操作可以被异步管理
        public void AddActor(string path, IActorInstance actor)
        {
            if (instance.TryAdd(path, actor))
            {
                actor.OnCreate(this);
            }
        }

        //这个操作可以被异步管理
        public void CloseActor(string path)
        {
            if (instance.TryRemove(path, out IActorInstance actor))
            {
                actor.OnClose();
            }
        }

        public IActorInstance GetActor(string path)
        {
            if (instance.TryGetValue(path, out IActorInstance actor))
            {
                return actor;
            }
            return null;
        }

        public ICollection<string> GetAllActorPath()
        {
            return instance.Keys;
        }
    }
}
