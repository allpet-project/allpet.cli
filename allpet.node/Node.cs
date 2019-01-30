using AllPet.peer.tcp;
using AllPet.Pipeline;
using AllPet.Pipeline.MsgPack;
using MsgPack;
using System;

using AllPet.Common;
using Newtonsoft.Json.Linq;

namespace AllPet.Module
{
    class ChainInfo
    {
        public ChainInfo(Newtonsoft.Json.Linq.JObject configJson)
        {
            MagicStr = configJson["MagicStr"].AsString();
            InitOwner = configJson["InitOwner"].AsStringArray();
        }
        public string MagicStr;
        public string[] InitOwner;
        public byte[] ToInitScript()
        {
            var dict = new MessagePackObjectDictionary();
            dict["magicstr"] = MagicStr;
            var array = new MessagePackObject[InitOwner.Length];
            dict["initowner"] = array;
            for (var i = 0; i < InitOwner.Length; i++)
            {
                array[i] = InitOwner[i];
            }

            var arr = MsgPack_Helper.Pack(new MessagePackObject(dict));
            return arr.ToArray();
        }
    }
    class Config_Module
    {
        public System.Net.IPEndPoint PublicEndPoint;
        public System.Net.IPEndPoint[] InitPeer;
        public ChainInfo ChainInfo;
        public Config_Module(Newtonsoft.Json.Linq.JObject json)
        {
            PublicEndPoint = json["PublicEndPoint"].AsIPEndPoint();
            InitPeer = json["InitPeer"].AsIPEndPointArray();
            ChainInfo = new ChainInfo(json["ChainInfo"] as JObject);
        }
    }
    public enum CmdList : UInt16
    {
        Want_JoinPeer = 0x0100,//告知其他节点我的存在
        Tell_AcceptJoin,//同意他加入
        Want_PeerList,//询问一个节点所能到达的节点
        Tell_PeerList,//告知一个节点所能到达的节点
    }
    public class LinkObj
    {
        public Hash256 ID;//节点ID，不能重复，每个节点自己生成，重复则不接受第二个节点
        public IModulePipeline remoteNode;
        public System.Net.IPEndPoint publicEndPoint;//公开的地址好让人进行P2P连接
    }
    public class Module_Node : Module_MsgPack
    {
        AllPet.Common.ILogger logger;
        Config_Module config;
        Hash256 guid;
        Hash256 chainHash;
        System.Collections.Concurrent.ConcurrentDictionary<UInt64, LinkObj> linkNodes;
        public Module_Node(AllPet.Common.ILogger logger, Newtonsoft.Json.Linq.JObject configJson) : base(true)
        {
            this.guid = Helper_NEO.CalcHash256(Guid.NewGuid().ToByteArray());
            this.logger = logger;
            this.config = new Config_Module(configJson);
            this.chainHash = Helper_NEO.CalcHash256(this.config.ChainInfo.ToInitScript());
            //this.config = new Config_ChainInit(configJson);
            this.linkNodes = new System.Collections.Concurrent.ConcurrentDictionary<ulong, LinkObj>();
        }
        //peerid 是连接id，而每个节点，需要一个唯一不重复的节点ID，以方便进行识别


        void RegNetEvent(ISystemPipeline syspipe)
        {
            if (syspipe.linked)
            {
                _OnPeerLink(syspipe.PeerID);
            }
            else
            {
                syspipe.OnPeerLink += _OnPeerLink;
            }
            syspipe.OnPeerClose += _OnPeerClose;
        }
        void _OnPeerLink(UInt64 id)//不知道这个连接是进来的，还是我连出去的，也不知道这个是不是我自己
        {
            var pipe = linkNodes[id];
            var dict = new MessagePackObjectDictionary();
            dict["cmd"] = (UInt16)CmdList.Want_JoinPeer;
            dict["id"] = this.guid.data;
            dict["pubep"] = this.config.PublicEndPoint.ToString();
            dict["chaininfo"] = chainHash.data;
            pipe.remoteNode.Tell(new MessagePackObject(dict));
            logger.Info("_OnPeerLink" + id);
        }
        void _OnPeerClose(UInt64 id)
        {
            linkNodes.TryRemove(id, out LinkObj node);
            logger.Info("_OnPeerClose" + id);

        }
        public override void OnStart()
        {
            foreach (var p in this.config.InitPeer)
            {
                //让GetPipeline来自动连接
                var remotenode = this.GetPipeline(p.ToString() + "/node");//模块的名称是固定的
                linkNodes[remotenode.system.PeerID] = new LinkObj()
                {
                    ID = null,
                    remoteNode = remotenode,
                    publicEndPoint = null
                };
                RegNetEvent(remotenode.system);

                //actorpeer.IsVaild 此时这个pipeline不是立即可用的，需要等待
            }
        }

        public override void OnTell(IModulePipeline from, MessagePackObject? obj)
        {
            if (this.linkNodes.TryGetValue(from.system.PeerID, out LinkObj link)==false)
            {
                linkNodes[from.system.PeerID] = new LinkObj()
                {
                    ID = null,
                    remoteNode = from,
                    publicEndPoint = null
                };
                RegNetEvent(from.system);
            }
            var dict = obj.Value.AsDictionary();
            var cmd = (CmdList)dict["cmd"].AsInt16();
            logger.Info(obj.Value.ToString());
            switch (cmd)
            {
                case CmdList.Want_JoinPeer:
                    {
                        Want_JoinPeer(from, dict);
                    }
                    break;
                case CmdList.Tell_AcceptJoin:
                    break;
                case CmdList.Want_PeerList:
                    break;
                case CmdList.Tell_PeerList:
                    break;

            }
        }
        void Want_JoinPeer(IModulePipeline from, MessagePackObjectDictionary dict)
        {

            Hash256 id = dict["id"].AsBinary();
            if (this.guid.Equals(id))
            {
                logger.Info("my self in.");
                this._System.DisConnect(from.system);//断开这个连接
                return;
            }
            
            Hash256 hash = dict["chaininfo"].AsBinary();
            if (hash.Equals(this.chainHash)==false)
            {
                logger.Info("join hash is diff.");
                //this._System.Disconnect(from.system);//断开这个连接
                //return;
            }
            System.Net.IPEndPoint pubeb = null;
            if (dict.ContainsKey("pubep"))
            {
                pubeb = dict["pubep"].AsString().AsIPEndPoint();
            }

            var link = this.linkNodes[from.system.PeerID];
            link.ID = id;
            logger.Info("there is a peer what to join here.:");

        }
    }
}
