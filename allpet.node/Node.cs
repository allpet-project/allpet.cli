using AllPet.peer.tcp;
using AllPet.Pipeline;
using AllPet.Pipeline.MsgPack;
using MsgPack;
using System;

using AllPet.Common;
using Newtonsoft.Json.Linq;
using System.Net;

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
        //Request_ 开头的都是一对一消息，发往单个节点
        //Response_ 开头的都是一对一消息，发往单个节点
        //BoardCast_ 开头的，会向自己平级和低级的节点发送
        //POST_ 开头的,会向比自己高级的节点发送

        Request_JoinPeer = 0x0100,//告知其他节点我的存在，包括是不是共识节点之类的
        Response_AcceptJoin,//同意他加入，并给他一个测试信息
        Request_ProvePeer,//用测试信息+响应信息，做一个签名返回，对方就知道我拥有某一个公钥

        Request_PeerList,//询问一个节点所能到达的节点
        Response_PeerList,//告知一个节点所能到达的节点

        Post_TouchProvedPeer,//请求寻找一个证明的节点
        Response_Iamhere,
        Post_SendRaw,//产生新的消息

        BoradCast_PeerProved,//一个节点证明了他自己
        BoardCast_NewBlock,//新的块产生了
    }
    public class LinkObj
    {
        public Hash256 ID;//节点ID，不能重复，每个节点自己生成，重复则不接受第二个节点
        public IModulePipeline remoteNode;
        public System.Net.IPEndPoint publicEndPoint;//公开的地址好让人进行P2P连接
        public bool hadJoin;//是否被允许加入了网络
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
                _OnPeerLink(syspipe.PeerID, syspipe.IsHost, syspipe.Remote);
            }
            else
            {
                syspipe.OnPeerLink += _OnPeerLink;
            }
            syspipe.OnPeerClose += _OnPeerClose;
        }
        void _OnPeerLink(UInt64 id, bool accept, IPEndPoint remote)//现在能区分主叫 connect 和 被叫 accept了
        {
            var pipe = linkNodes[id];

            //主叫被叫都尝试加入对方网络

            Tell_ReqJoinPeer(pipe.remoteNode);
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
            if (this.linkNodes.TryGetValue(from.system.PeerID, out LinkObj link) == false)
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
                case CmdList.Request_JoinPeer:
                    {
                        OnRecv_RequestJoinPeer(from, dict);
                    }
                    break;
                case CmdList.Response_AcceptJoin:
                    {
                        OnRecv_ResponseJoin(from, dict);
                    }
                    break;
                case CmdList.Request_PeerList:
                    break;
                case CmdList.Response_PeerList:
                    break;

            }
        }
        void Tell_ReqJoinPeer(IModulePipeline remote)
        {
            var dict = new MessagePackObjectDictionary();
            dict["cmd"] = (UInt16)CmdList.Request_JoinPeer;
            dict["id"] = this.guid.data;
            dict["pubep"] = this.config.PublicEndPoint.ToString();
            dict["chaininfo"] = chainHash.data;
            remote.Tell(new MessagePackObject(dict));
        }
        void Tell_ResponseAcceptJoin(IModulePipeline remote)
        {
            var dict = new MessagePackObjectDictionary();
            dict["cmd"] = (UInt16)CmdList.Response_AcceptJoin;
            //选个挑战信息
            remote.Tell(new MessagePackObject(dict));
        }
        void OnRecv_RequestJoinPeer(IModulePipeline from, MessagePackObjectDictionary dict)
        {
            logger.Info("there is a peer what to join here.:");

            Hash256 id = dict["id"].AsBinary();
            if (this.guid.Equals(id))
            {
                logger.Warn("Join Err:my self in.");
                this._System.DisConnect(from.system);//断开这个连接
                return;
            }

            Hash256 hash = dict["chaininfo"].AsBinary();
            if (hash.Equals(this.chainHash) == false)
            {
                logger.Warn("Join Err:chaininfo is diff.");
                //this._System.Disconnect(from.system);//断开这个连接
                //return;
            }
            var link = this.linkNodes[from.system.PeerID];
            link.ID = id;
            System.Net.IPEndPoint pubeb = null;
            if (dict.ContainsKey("pubep"))
            {
                pubeb = dict["pubep"].AsString().AsIPEndPoint();
            }
            if (pubeb.Port != 0)
            {
                if (pubeb.Address == IPAddress.Any)
                {
                    pubeb.Address = from.system.Remote.Address;

                }
                link.publicEndPoint = pubeb;
            }



            //and accept
            Tell_ResponseAcceptJoin(from);
        }
        void OnRecv_ResponseJoin(IModulePipeline from, MessagePackObjectDictionary dict)
        {
            logger.Info("had join chain");
            var link = this.linkNodes[from.system.PeerID];
            link.hadJoin = true;//已经和某个节点接通
        }
    }
}
