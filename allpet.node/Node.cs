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
            return null;
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

    public class Module_Node : Module_MsgPack
    {
        AllPet.Common.ILogger logger;
        Config_Module config;
        public Module_Node(AllPet.Common.ILogger logger, Newtonsoft.Json.Linq.JObject configJson) : base(true)
        {
            this.logger = logger;
            this.config = new Config_Module(configJson);
            //this.config = new Config_ChainInit(configJson);
        }

        public override void OnStart()
        {
            foreach(var p in this.config.InitPeer)
            {
                var actorpeer = this.GetPipeline(p.ToString() + "/node");//模块的名称是固定的
                //actorpeer.IsVaild 此时这个pipeline不是立即可用的，需要等待
            }
        }

        public override void OnTell(IModulePipeline from, MessagePackObject? obj)
        {
            var dict = obj.Value.AsDictionary();
            var cmd = (CmdList)dict["cmd"].AsInt16();
            switch (cmd)
            {
                case CmdList.Want_JoinPeer:
                    break;
                case CmdList.Tell_AcceptJoin:
                    break;
                case CmdList.Want_PeerList:
                    break;
                case CmdList.Tell_PeerList:
                    break;

            }
        }
    }
}
