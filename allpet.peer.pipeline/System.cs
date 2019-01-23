using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AllPet.peer.tcp;

namespace AllPet.Pipeline
{
    class PipelineSystemV1 : ISystem
    {
        //本地创建的Actor实例
        global::System.Collections.Concurrent.ConcurrentDictionary<string, IModuleInstance> localActors;
        global::System.Collections.Concurrent.ConcurrentDictionary<IModuleInstance, string> localActorPath;
        //所有的Actor引用，无论是远程的还是本地的
        global::System.Collections.Concurrent.ConcurrentDictionary<string, IModulePipeline> refActors;

        global::System.Collections.Concurrent.ConcurrentDictionary<string, ISystemPipeline> refSystems;

        //建立连接时找ip用
        global::System.Collections.Concurrent.ConcurrentDictionary<UInt64, string> linkedIP;
        ISystemPipeline refSystemThis;

        public PipelineSystemV1()
        {
            localActors = new global::System.Collections.Concurrent.ConcurrentDictionary<string, IModuleInstance>();
            localActorPath = new global::System.Collections.Concurrent.ConcurrentDictionary<IModuleInstance, string>();
            refActors = new global::System.Collections.Concurrent.ConcurrentDictionary<string, IModulePipeline>();
            refSystems = new global::System.Collections.Concurrent.ConcurrentDictionary<string, ISystemPipeline>();
            linkedIP = new global::System.Collections.Concurrent.ConcurrentDictionary<ulong, string>();
            refSystemThis = new PipelineSystemRefLocal(this);
        }
        bool bStarted = false;
        public void Start()
        {
            bStarted = true;
            foreach (var pipe in this.localActors)
            {
                System.Threading.ThreadPool.QueueUserWorkItem((e) =>
                {
                    pipe.Value.OnStart();
                });
            }
        }
        public void Close()
        {
            this.Dispose();
        }

        public void Dispose()
        {

        }
        public void RegistModule(string path, IModuleInstance actor)
        {
            if (localActors.ContainsKey(path) == true)
                throw new Exception("already have that path.");

            localActors[path] = actor;
            localActorPath[actor] = path;
            actor.OnRegistered(this);

            if (bStarted)
            {
                System.Threading.ThreadPool.QueueUserWorkItem((e) =>
                {
                    actor.OnStart();
                });
            }
        }
        public void UnRegistModule(string path)
        {
            if (path.IndexOf("this/") != 0)
                path = "this/" + path;
            if (refActors.ContainsKey(path))
            {
                refActors.TryRemove(path, out IModulePipeline actor);
            }
            path = path.Substring(5);
            if (localActors.ContainsKey(path))
            {
                localActors.TryRemove(path, out IModuleInstance actor);
            }
        }
        public string GetModulePath(IModuleInstance actor)
        {
            if (localActorPath.TryGetValue(actor, out string path))
            {
                return path;
            }
            return null;
        }
        
        public IModulePipeline GetPipeline(IModuleInstance user, string urlActor)
        {
            if (bStarted == false)
                throw new Exception("must getpipeline after System.Start()");

            var userstr = "";
            if (user != null)
                userstr = localActorPath[user];
            var refName = userstr + "_" + urlActor;

            if (refActors.TryGetValue(refName, out IModulePipeline pipe))
            {
                return pipe;
            }

            if (urlActor.IndexOf("this/") == 0)
            {
                var actorpath = urlActor.Substring(5);
                var actor = this.localActors[actorpath];
                refActors[refName] = new PipelineRefLocal(refSystemThis, userstr, actorpath, actor);
                return refActors[refName];
            }
            else
            {
                var sppos = urlActor.IndexOf('/');
                var addr = urlActor.Substring(0, sppos);
                var path = urlActor.Substring(sppos + 1);
                ISystemPipeline refsys = null;
                if (refSystems.TryGetValue(addr, out refsys))
                {

                }
                else
                {//没连接
                }
                refActors[refName] = new PipelineRefRemote(refSystemThis, userstr, refsys as RefSystemRemote, path);
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
        public unsafe void OpenNetwork(PeerOption option)
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
                int seek = 0;
                var fromlen = data[seek]; seek++;
                string from = System.Text.Encoding.UTF8.GetString(data, seek, fromlen); seek += fromlen;
                var tolen = data[seek]; seek++;
                string to = System.Text.Encoding.UTF8.GetString(data, seek, tolen); seek += tolen;
                IModuleInstance user = null;
                if (this.localActors.TryGetValue(from, out user))
                {

                }
                var pipe = this.GetPipeline(user, "this/" + to);
                var outbytes = new byte[data.Length - seek];
                fixed (byte* pdata = data, pout = outbytes)
                {
                    Buffer.MemoryCopy(pdata + seek, pout, outbytes.Length, outbytes.Length);
                }
                pipe.Tell(outbytes);
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

        public async Task<ISystemPipeline> Connect(IPEndPoint remote)
        {
            if (peer == null)
                throw new Exception("not init peer.");

            var linkid = peer.Connect(remote);
            var remotestr = remote.ToString();
            linkedIP[linkid] = remotestr;
            while (true)
            {
                await global::System.Threading.Tasks.Task.Delay(100);
                if (this.refSystems.TryGetValue(remotestr, out ISystemPipeline sys))
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
        public ICollection<ISystemPipeline> GetAllSystems()
        {
            return refSystems.Values;
        }




    }
}
