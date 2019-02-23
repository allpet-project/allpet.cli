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
using System.Threading.Tasks;
using System.Threading;
using AllPet.Module.block;
using System.Timers;

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
        public string SimpleDbPath;
        public Config_Module(Newtonsoft.Json.Linq.JObject json)
        {
            PublicEndPoint = json["PublicEndPoint"].AsIPEndPoint();
            InitPeer = json["InitPeer"].AsIPEndPointArray();
            ChainInfo = new ChainInfo(json["ChainInfo"] as JObject);
            SimpleDbPath = json["SimpleDbPath"].AsString();
        }
    }

    public class LinkObj
    {
        public IPEndPoint from;//哪个节点告诉你建立这个连接的
        public bool beAccepted;
        public Hash256 ID;//节点ID，不能重复，每个节点自己生成，重复则不接受第二个节点
        public IModulePipeline remoteNode;
        public System.Net.IPEndPoint publicEndPoint;//公开的地址好让人进行P2P连接
        public bool hadJoin;//是否被允许加入了网络
        public byte[] CheckInfo;
        public byte[] PublicKey;
        public int pLevel=-1;
        public string provedPubep;
        public bool isProved;//是否是记账人节点
    }

    public enum LinkFromEnum
    {
        InitPeer,
        ResetLink,
        ResponsePeers
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

        public IPEndPoint from;
        public LinkFromEnum fromType;

        public IPEndPoint remote;
        public Hash256 ID;
        public byte[] PublicKey;
        public int weight = 10;//节点重连权重
        public int linkCount;//调连接的次数
    }

    public partial class Module_Node : Module_MsgPack
    {
        AllPet.Common.ILogger logger;
        Config_Module config;
        Hash256 guid;
        Hash256 chainHash;
        bool isProved;//本节点是否是记账人        
        int pLevel = -1;//节点优先级，记账节点为0。默认值为-1，意味着本身不是记账节点，也没有连上任何其他节点

        byte[] prikey;
        byte[] pubkey;

        System.Collections.Concurrent.ConcurrentDictionary<UInt64, LinkObj> linkNodes;
        System.Collections.Concurrent.ConcurrentDictionary<UInt64, LinkObj> provedNodes;//记账人列表
        System.Collections.Concurrent.ConcurrentDictionary<string, UInt64> linkIDs;
        Struct.ThreadSafeQueueWithKey<CanLinkObj> listCanlink;
        System.Timers.Timer blockTimer;
        BlockChain blockChain;

        static string NodePath = "./node.data";
        static ulong blockTime = 5;//出块间隔时间
        static  object blockTimerLock = new object();


        public Node.TXPool txpool;//交易池

        /// <summary>
        /// 测试用，开关是否请求peerlist
        /// </summary>
        public bool beEnableQueryPeers = true;
        public bool beObserver = false;

        public Module_Node(AllPet.Common.ILogger logger, Newtonsoft.Json.Linq.JObject configJson) : base(true)
        {
            this.guid = Helper_NEO.CalcHash256(Guid.NewGuid().ToByteArray());
            this.logger = logger;
            this.config = new Config_Module(configJson);
            this.chainHash = Helper_NEO.CalcHash256(this.config.ChainInfo.ToInitScript());
            //this.config = new Config_ChainInit(configJson);
            this.linkNodes = new System.Collections.Concurrent.ConcurrentDictionary<ulong, LinkObj>();
            this.provedNodes = new System.Collections.Concurrent.ConcurrentDictionary<ulong, LinkObj>();
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
                    if (this.config.ChainInfo.InitOwner.Contains(address))
                    {
                        this.isProved = true;
                        this.pLevel = 0;//记账节点
                    }
                }
                //return;
                blockChain = new BlockChain();
                blockChain.InitChain(this.config.SimpleDbPath, this.config.ChainInfo);
                this.lastIndex = blockChain.GetLastIndex();
                this.blockIndex = blockChain.GetBlockIndex();
                this.txIndex = blockChain.GetBlockTxIndex();                

                //记账节点才需要出块
                if (this.isProved)
                {
                    this.blockTimer = new System.Timers.Timer();
                    this.blockTimer.Interval = blockTime * 1000;//毫秒
                    this.blockTimer.Enabled = true;
                    this.blockTimer.AutoReset = true;//一直执行true 
                    this.blockTimer.Elapsed += new System.Timers.ElapsedEventHandler(MakeBlock);
                    this.blockTimer.Start();
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

        private void BlockTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            throw new NotImplementedException();
        }

        //peerid 是连接id，而每个节点，需要一个唯一不重复的节点ID，以方便进行识别


        void RegNetEvent(ISystemPipeline syspipe)
        {
            //    //syspipe.HaltLink();
            //    if (syspipe.linked)
            //    {
            //        //logger.Info("==reg net event direct");
            //        _OnPeerLink(syspipe.PeerID, syspipe.IsHost, syspipe.Remote);
            //    }
            //    else
            //    {
            //        //logger.Info("==reg net event delay");
            //        syspipe.OnPeerLink += _OnPeerLink;
            //    }
            //    syspipe.OnPeerClose += _OnPeerClose;
            //    //syspipe.ResumeLink();

            syspipe.SetLinkEvent(this._OnPeerLink);
            syspipe.SetLinkCloseEvent(this._OnPeerClose);
        }
        void _OnPeerLink(ISystemPipeline system)//现在能区分主叫 connect 和 被叫 accept了
        {
            var pipe = linkNodes[system.PeerID];

            //主叫被叫都尝试加入对方网络
            logger.Info("--------------------------》_OnPeerLink：id/" + system.PeerID + "         IPEndPoint/" + system.Remote + "              BeAccepted:" + system.IsHost);

            Tell_ReqJoinPeer(pipe.remoteNode);
            if (this.isProved)
            {
                //告诉我你是否是共识节点或者能否到达共识节点
                Tell_Post_TouchProvedPeer(pipe.remoteNode, string.Empty, this.guid.ToString());
            }
            logger.Info("_OnPeerLink" + system.PeerID);
        }
        void _OnPeerClose(ISystemPipeline system)
        {
            linkNodes.TryRemove(system.PeerID, out LinkObj node);
            var remotestr = node.remoteNode.system.Remote.ToString();
            this.linkIDs.TryRemove(remotestr, out ulong v);

            provedNodes.TryRemove(system.PeerID, out LinkObj keepernode);
            var canlink = this.listCanlink.Getqueue(remotestr);
            if (canlink?.weight > 0)
            {
                canlink.weight--;
            }
            logger.Info("_OnPeerClose" + system.PeerID);

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
                    remote = p,
                    fromType = LinkFromEnum.InitPeer
                });
            }
            startWathNetWork();
            //WatchNetwork();

        }

        private void ConnectOne(IPEndPoint p, IPEndPoint whoTell = null)
        {
            //让GetPipeline来自动连接,此时remotenode 不可立即通讯，等回调，见RegNetEvent
            var remotenode = this.GetPipeline(p.ToString() + "/node");

            var system = remotenode.system;
            linkNodes[system.PeerID] = new LinkObj()
            {
                from = whoTell,
                ID = null,
                remoteNode = remotenode,
                publicEndPoint = null,
                beAccepted = false
            };

            linkIDs[system.Remote.ToString()] = system.PeerID;

            RegNetEvent(system);

            //,(node) =>
            //{
            //    var system = node.system;
            //    linkNodes[node.system.PeerID] = new LinkObj()
            //    {
            //        from = whoTell,
            //        ID = null,
            //        remoteNode = node,
            //        publicEndPoint = null,
            //        beAccepted = false
            //    };

            //    linkIDs[system.Remote.ToString()] = system.PeerID;
            //    RegNetEvent(system);
            //    //var fromstr = whoTell != null ? whoTell.ToString() : "";
            //    logger.Info("try to link to=>" + p.ToString());

            //});//模块的名称是固定的

            //var system = remotenode.system;
            //Console.WriteLine("@@@@@@@@@@@@@ systemid"+remotenode.system.PeerID);



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
            catch (Exception ex)
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
                            remote = p,
                            fromType = LinkFromEnum.ResetLink
                        });
                    }
                }
            }
        }
        /// <summary>
        /// txIndex:现在出到哪个交易啦
        /// lastIndex:现在交易的最大号
        /// 如果当前还有哪个交易没有出到块里，那就把出到块里
        /// 如果当前所有交易都出块了，那就每隔15秒出个空块
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void MakeBlock(object source, ElapsedEventArgs e)
        {
            lock (blockTimerLock)
            {
                List<MessagePackObject> block = null;

                //没有交易时要出空块
                //有交易且所有交易都已出块，也要出空块
                if ((this.txIndex != 0 && this.txIndex >= (this.lastIndex - 1)) || this.lastIndex <=0)
                {
                    if (blockCount >= (15/ blockTime))
                    {
                        //出空块
                        blockCount = 0;
                        block = new List<MessagePackObject>();
                    }
                    else
                    {
                        blockCount++;
                    }
                }
                else
                {
                    blockCount = 0;
                    ulong length = this.txIndex==0?(this.lastIndex - this.txIndex):(this.lastIndex - this.txIndex-1);
                    if (length > 0)
                    {
                        block = new List<MessagePackObject>();
                        ulong start = (this.txIndex == 0 ? 0 : (this.txIndex + 1));
                        for (ulong i = 0; i < length; i++)
                        {
                            block.Add(new MessagePackObject(start));
                            this.txIndex = start;
                            start++;
                        }
                    }
                }
                if (block != null)
                {
                    var data = MsgPack_Helper.Pack(new MessagePackObject(block));
                    var bindex = this.blockIndex;
                    this.blockIndex++;
                    this.blockChain.MakeBlock(bindex, data.ToArray(), this.txIndex, this.blockIndex);
                }
            }
        }
        public override void Dispose()
        {
            base.Dispose();
            this.blockChain.Dispose();
            SaveCanlinkList();
            stopWatchNetWork();            
        }
        private CancellationTokenSource cts;
        public void stopWatchNetWork()
        {
            cts.Cancel();
        }
        public void startWathNetWork()
        {
            var _cts = new CancellationTokenSource();
            var ct = _cts.Token;
            var wathTask = new Task(async () =>
            {
                await this.WatchNetwork(ct);
            }, ct);
            wathTask.Start();
            this.cts = _cts;
        }
        public async Task WatchNetwork(CancellationToken token)
        {
            int refreshnetwaiter = 0;
            int connectwaiter = 0;
            while (true)
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }

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
                            var times = (10 - canlink.weight) / 2;
                            if (canlink.linkCount == times)
                            {
                                var fromstr = canlink.from != null ? canlink.from.ToString() : "";
                                logger.Info("------------------------------------ConnectOne=>" + canlink.remote.ToString() + "       from:" + fromstr + "  fromtype:" + canlink.fromType);

                                ConnectOne(canlink.remote, canlink.from);

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
