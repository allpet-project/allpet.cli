using AllPet.peer.tcp;
using AllPet.Pipeline;
using AllPet.Pipeline.MsgPack;
using MsgPack;
using System;
using System.Linq;
using AllPet.Common;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Text;
using System.Collections.Generic;
using System.IO;

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

    public class LinkObj
    {
        public Hash256 ID;//节点ID，不能重复，每个节点自己生成，重复则不接受第二个节点
        public IModulePipeline remoteNode;
        public System.Net.IPEndPoint publicEndPoint;//公开的地址好让人进行P2P连接
        public bool hadJoin;//是否被允许加入了网络
        public byte[] CheckInfo;
        public byte[] PublicKey;
    }
    public class CanLinkObj : IEquatable<CanLinkObj>
    {
        public override string ToString()
        {
            return remote.ToString();
        }
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
        public override bool Equals(object obj)
        {
            return ToString().Equals(obj.ToString());
        }
        public bool Equals(CanLinkObj other)
        {
            return ToString().Equals(other.ToString());
        }


        public IPEndPoint remote;
        public Hash256 ID;
        public byte[] PublicKey;
        public int weight = 10;//节点重连权重
        public int linkCount;
    }

    public partial class Module_Node : Module_MsgPack
    {
        AllPet.Common.ILogger logger;
        Config_Module config;
        Hash256 guid;
        Hash256 chainHash;
        bool isBookKeeper;//本节点是否是记账人        

        byte[] prikey;
        byte[] pubkey;

        System.Collections.Concurrent.ConcurrentDictionary<UInt64, LinkObj> linkNodes;
        System.Collections.Concurrent.ConcurrentDictionary<UInt64, LinkObj> bookKeeperNodes;//记账人列表
        System.Collections.Concurrent.ConcurrentDictionary<string, UInt64> linkIDs;
        Struct.ThreadSafeQueueWithKey<CanLinkObj> listCanlink;
        static string NodePath = "./node.data";


        public Node.TXPool txpool;//交易池
        public Module_Node(AllPet.Common.ILogger logger, Newtonsoft.Json.Linq.JObject configJson) : base(true)
        {
            this.guid = Helper_NEO.CalcHash256(Guid.NewGuid().ToByteArray());
            this.logger = logger;
            this.config = new Config_Module(configJson);
            this.chainHash = Helper_NEO.CalcHash256(this.config.ChainInfo.ToInitScript());
            //this.config = new Config_ChainInit(configJson);
            this.linkNodes = new System.Collections.Concurrent.ConcurrentDictionary<ulong, LinkObj>();
            this.bookKeeperNodes = new System.Collections.Concurrent.ConcurrentDictionary<ulong, LinkObj>();
            this.linkIDs = new System.Collections.Concurrent.ConcurrentDictionary<string, ulong>();
            this.listCanlink = new Struct.ThreadSafeQueueWithKey<CanLinkObj>();
            try
            {
                if (configJson.ContainsKey("Key_Nep2") && configJson.ContainsKey("Key_Password"))
                {
                    var nep2 = configJson["Key_Nep2"].AsString();
                    var password = configJson["Key_Password"].AsString();
                    this.prikey = Helper_NEO.GetPrivateKeyFromNEP2(nep2, password);
                    this.pubkey = Helper_NEO.GetPublicKey_FromPrivateKey(prikey);
                    //区分记账人                    
                    var address = Helper_NEO.GetAddress_FromPublicKey(pubkey);//证明人的地址
                   //如果证明人的地址和初始记账人的地址相同即为记账人
                   if(this.config.ChainInfo.InitOwner.Contains(address))
                    {
                        this.isBookKeeper = true;
                    }
                }
            }
            catch (Exception err)
            {
                logger.Error("Error in Get Prikey：" + err.ToString());
                throw new Exception("error in get prikey.", err);
            }

            this.txpool = new Node.TXPool();
            ResetCanlinkList();
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
            var remotestr = node.remoteNode.system.Remote.ToString();
            this.linkIDs.TryRemove(remotestr, out ulong v);

            bookKeeperNodes.TryRemove(id, out LinkObj keepernode);
            var canlink = this.listCanlink.Getqueue(remotestr);
            if(canlink?.weight > 0)
            {
                canlink.weight--;
            }
            logger.Info("_OnPeerClose" + id);

        }
        public override void OnStart()
        {
            foreach (var p in this.config.InitPeer)
            {
                //initpeer 作为入口，就不尝试重新连接了

                //CanLinkObj link = new CanLinkObj();
                //link.PublicKey = null;
                //link.remote = p;
                //link.ID = new Hash256(new byte[32]);
                //this.listCanlink.Enqueue(link);
                ConnectOne(p);
                //actorpeer.IsVaild 此时这个pipeline不是立即可用的，需要等待
                this.listCanlink.Enqueue(new CanLinkObj()
                {
                    remote = p
                });
            }
            WatchNetwork();

        }

        private void ConnectOne(IPEndPoint p)
        {                
            //让GetPipeline来自动连接,此时remotenode 不可立即通讯，等回调，见RegNetEvent
            var remotenode = this.GetPipeline(p.ToString() + "/node");//模块的名称是固定的
            linkNodes[remotenode.system.PeerID] = new LinkObj()
            {
                ID = null,
                remoteNode = remotenode,
                publicEndPoint = null
            };
            linkIDs[remotenode.system.Remote.ToString()] = remotenode.system.PeerID;
            RegNetEvent(remotenode.system);
            logger.Info("try to link to=>" + remotenode.system.Remote.ToString());

        }

        private void SaveCanlinkList()
        {
            var sb = new StringBuilder();
            
            while (this.listCanlink.Count > 0)
            {
                var item = this.listCanlink.Dequeue();
                if (item != null && item.remote != null)
                {
                    sb.Append(item.remote.Address);
                    sb.Append(":");
                    sb.Append(item.remote.Port);
                    sb.Append("\n");
                }                
            }
            try
            {
                if (File.Exists(NodePath))
                {
                    File.Delete(NodePath);
                }
                System.IO.File.AppendAllText(NodePath, sb.ToString(), System.Text.Encoding.UTF8);

            }
            catch(Exception ex)
            {
                
            }
        }

        private void ResetCanlinkList()
        {
            if (!File.Exists(NodePath))
            {
                return;
            }
            using (StreamReader sr = new StreamReader(NodePath, false))
            {
                while (!sr.EndOfStream)
                {
                    var str = sr.ReadLine();
                    IPEndPoint.TryParse(str, out IPEndPoint p);
                    if (p != null)
                    {
                        this.listCanlink.Enqueue(new CanLinkObj()
                        {
                            remote = p
                        });
                    }
                }
            }                
        }

        public override void Dispose()
        {
            base.Dispose();
            SaveCanlinkList();
        }
        public async void WatchNetwork()
        {
            int refreshnetwaiter = 0;
            int connectwaiter = 0;
            while (true)
            {
                refreshnetwaiter++;
                if (refreshnetwaiter > 60)
                {
                    refreshnetwaiter = 0;
                    //一分钟刷新一下网络
                    foreach (var n in this.linkNodes.Values)
                    {
                        if (n.hadJoin)
                            this.Tell_Request_PeerList(n.remoteNode);
                    }
                }

                connectwaiter++;
                if (connectwaiter > 5)
                {
                    connectwaiter = 0;
                    //5秒钟处理一下连接
                    int linked = 0;
                    foreach (var n in this.linkNodes.Values)
                    {
                        if (n.hadJoin)
                            linked++;
                    }
                    var maxc = Math.Min(100 - linked, this.listCanlink.Count);
                    for (var i = 0; i < maxc; i++)
                    {
                        var canlink = listCanlink.Dequeue();
                        if (canlink.ID != null && canlink.ID.Equals(this.guid))//这是我自己，不要连
                            continue;
                        if (this.linkIDs.ContainsKey(canlink.remote.ToString()) == false)
                        {
                            var times = (10-canlink.weight)/2;
                            if (canlink.linkCount == times)
                            {
                                ConnectOne(canlink.remote);
                                canlink.linkCount = 0;
                            }
                            else
                            {
                                canlink.linkCount++;
                            }
                        }
                        if (canlink.weight > 0)//权重等于0的话会被移除
                        {
                            listCanlink.Enqueue(canlink);
                        }
                    }                    
                }

                await System.Threading.Tasks.Task.Delay(1000);//1秒刷新一次
            }
        }


    }
}
