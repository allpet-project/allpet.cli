using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AllPet.peer.tcp;

namespace AllPet.Pipeline
{
    class System : IPipelineSystem
    {
        //本地创建的Actor实例
        global::System.Collections.Concurrent.ConcurrentDictionary<string, IPipelineInstance> localActors;
        global::System.Collections.Concurrent.ConcurrentDictionary<IPipelineInstance, string> localActorPath;
        //所有的Actor引用，无论是远程的还是本地的
        global::System.Collections.Concurrent.ConcurrentDictionary<string, IPipelineRef> refActors;

        global::System.Collections.Concurrent.ConcurrentDictionary<string, ISystemRef> refSystems;

        //建立连接时找ip用
        global::System.Collections.Concurrent.ConcurrentDictionary<UInt64, string> linkedIP;
        ISystemRef refSystemThis;

        public System()
        {
            localActors = new global::System.Collections.Concurrent.ConcurrentDictionary<string, IPipelineInstance>();
            localActorPath = new global::System.Collections.Concurrent.ConcurrentDictionary<IPipelineInstance, string>();
            refActors = new global::System.Collections.Concurrent.ConcurrentDictionary<string, IPipelineRef>();
            refSystems = new global::System.Collections.Concurrent.ConcurrentDictionary<string, ISystemRef>();
            linkedIP = new global::System.Collections.Concurrent.ConcurrentDictionary<ulong, string>();
            refSystemThis = new PipelineSystemRefLocal(this);
        }
        public void Dispose()
        {

        }
        public void RegistPipeline(string path, IPipelineInstance actor)
        {

            if (localActors.ContainsKey(path) == true)
                throw new Exception("already have that path.");

            localActors[path] = actor;
            localActorPath[actor] = path;

        }
        public void UnRegistPipeline(string path)
        {
            if (path.IndexOf("this/") != 0)
                path = "this/" + path;
            if (refActors.ContainsKey(path))
            {
                refActors.TryRemove(path, out IPipelineRef actor);
            }
            path = path.Substring(5);
            if (localActors.ContainsKey(path))
            {
                localActors.TryRemove(path, out IPipelineInstance actor);
            }
        }
        public string GetPipelinePath(IPipelineInstance actor)
        {
            if (localActorPath.TryGetValue(actor, out string path))
            {
                return path;
            }
            return null;
        }
        public IPipelineRef GetPipeline(IPipelineInstance user, string urlActor)
        {
            var userstr = "_";
            if (user != null)
                userstr = localActorPath[user] + "_";
            var refName = userstr + urlActor;

            if (refActors.TryGetValue(refName, out IPipelineRef pipe))
            {
                return pipe;
            }

            if (urlActor.IndexOf("this/") == 0)
            {
                var actorpath = urlActor.Substring(5);
                var actor = this.localActors[actorpath];
                refActors[refName] = new PipelineRefLocal(refSystemThis, actorpath, actor);
                return refActors[refName];
            }
            else
            {
                var sppos = urlActor.IndexOf('/');
                var addr = urlActor.Substring(0, sppos);
                var path = urlActor.Substring(sppos + 1);
                ISystemRef refsys = null;
                if (refSystems.TryGetValue(addr, out refsys))
                {

                }
                else
                {//没连接

                }
                refActors[refName] = new PipelineRefRemote(refsys as RefSystemRemote, path);
                return refActors[refName];
            }
        }
        //public IPipelineRef GetPipelineLocal(IPipelineInstance user, string path)
        //{
        //    return refActors["this/" + path];
        //}
        //public IPipelineRef GetPipelineRemote(IPipelineInstance user, IPEndPoint remote, string path)
        //{
        //    string url = remote.Address.ToString() + ":" + remote.Port + "/"+path;
        //    return refActors[url];
        //}
        AllPet.peer.tcp.IPeer peer;
        public void OpenNetwork(PeerOption option)
        {
            if (peer != null)
                throw new Exception("already have init peer.");
            peer = AllPet.peer.tcp.PeerV2.CreatePeer();
            peer.Start(option);
            peer.OnClosed += (id) =>
              {
                  Console.WriteLine("close line=" + id);
              };
            peer.OnLinkError += (id, err) =>
              {
                  Console.WriteLine("OnLinkError line=" + id);
              };
            peer.OnRecv += (id, data) =>
            {
                Console.WriteLine("; line=" + id + " len=" + data.Length);
            };

            peer.OnAccepted += (ulong id, IPEndPoint endpoint) =>
            {
                Console.WriteLine("on accepted." + id + " = " + endpoint);
            };

            peer.OnConnected += (ulong id) =>
              {
                  //主动连接成功，创建一个systemRef
                  var remotestr = this.linkedIP[id];
                  RefSystemRemote remote = new RefSystemRemote(peer, remotestr, id);
                  remote.linked = true;
                  this.refSystems[remotestr] = remote;
                  Console.WriteLine("on OnConnected.");
              };
        }
        public void CloseNetwork()
        {
            if (peer != null)
                throw new Exception("have not init peer.");
            peer.Close();
            peer = null;
        }
        public void OpenListen(IPEndPoint host)
        {
            if (peer == null)
                throw new Exception("not init peer.");

            peer.Listen(host);
        }

        public void CloseListen()
        {
            if (peer == null)
                throw new Exception("not init peer.");

            peer.StopListen();
        }

        public async Task<ISystemRef> Connect(IPEndPoint remote)
        {
            if (peer == null)
                throw new Exception("not init peer.");

            var linkid = peer.Connect(remote);
            var remotestr = remote.ToString();
            linkedIP[linkid] = remotestr;
            while (true)
            {
                await global::System.Threading.Tasks.Task.Delay(100);
                if (this.refSystems.TryGetValue(remotestr, out ISystemRef sys))
                {
                    return sys;
                }
            }
        }




        public ICollection<string> GetAllPipelinePath()
        {
            return refActors.Keys;
        }
        public ICollection<string> GetAllSystemsPath()
        {
            return refSystems.Keys;
        }
        public ICollection<ISystemRef> GetAllSystems()
        {
            return refSystems.Values;
        }




    }
}
