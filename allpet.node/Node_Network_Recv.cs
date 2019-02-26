using AllPet.Pipeline;
using AllPet.Pipeline.MsgPack;
using MsgPack;
using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using AllPet.Common;
using System.Linq;
using AllPet.Module.Node;
using allpet.module.node;
using AllPet.Module.block;

namespace AllPet.Module
{
    partial class Module_Node : Module_MsgPack
    {

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
                this._System.DisConnect(from.system);//断开这个连接
                return;
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
                if (pubeb.Address.ToString() == IPAddress.Any.ToString())
                {//remote.address 可能是ipv6 也有ipv4 ，当为ipv6即会出现::ffff:
                    pubeb.Address = from.system.Remote.Address.MapToIPv4();
                    //pubeb.Address = from.system.Remote.Address;

                }
                link.publicEndPoint = pubeb;
            }



            //and accept
            Tell_ResponseAcceptJoin(from);
        }
        void OnRecv_ResponseAcceptJoin(IModulePipeline from, MessagePackObjectDictionary dict)
        {
            logger.Info("had join chain");
            var link = this.linkNodes[from.system.PeerID];
            link.hadJoin = true;//已经和某个节点接通
            //如果连上了,标识连上的节点的优先级
            var plevel = dict["plevel"].AsInt32();
            this.getPlevelFromLinkObj(link, plevel);
            //Console.WriteLine("@ from:" + link.publicEndPoint + " plevel:" + link.pLevel + " node:" + this.config.PublicEndPoint + " plevel:" + this.pLevel);

            if (this.prikey != null)//有私钥证明一下
            {
                var check = dict["checkinfo"].AsBinary();
                var addinfo = Guid.NewGuid().ToByteArray();
                var message = addinfo.Concat(check).ToArray();
                var signdata = Helper_NEO.Sign(message, this.prikey);
                Tell_Request_ProvePeer(from, addinfo, signdata);
            }
            Tell_Request_PeerList(from);
            //如果连接上了，要更新自己的优先级
            //if (this.pLevel < 0)
            //{
            //    if (link.pLevel >= 0)//加入的节点优先级有效，且本身节点不是记账人
            //    {
            //        this.pLevel = link.pLevel + 1;
            //    }
            //}
            //else if(this.pLevel > link.pLevel)
            //{
            //    this.pLevel = link.pLevel + 1;
            //    //如果是变更，则广播低优先级节点
            //    foreach (var item in this.linkNodes)
            //    {
            //        if (item.Value.hadJoin && item.Value.pLevel < this.pLevel)
            //        {
            //            Tell_BoradCast_PeerState(item.Value.remoteNode);
            //        }
            //    }
            //}

            //System.Console.WriteLine($"node:{this.config.PublicEndPoint} pLeve:{this.pLevel}  isProved:{this.isProved}");
        }

        void OnRecv_RequestProvePeer(IModulePipeline from, MessagePackObjectDictionary dict)
        {
            var link = this.linkNodes[from.system.PeerID];
            var addinfo = dict["addinfo"].AsBinary();
            var pubkey = dict["pubkey"].AsBinary();
            var signdata = dict["signdata"].AsBinary();
            var message = addinfo.Concat(link.CheckInfo).ToArray();
            bool sign = Helper_NEO.VerifySignature(message, signdata, pubkey);
            if (sign)
            {
                link.PublicKey = pubkey;
                logger.Info("had a proved peer:" + Helper.Bytes2HexString(pubkey));
            }
            else
            {
                logger.Info("had a error proved peer:" + Helper.Bytes2HexString(pubkey));
            }            
        }
        void OnRecv_Request_PeerList(IModulePipeline from, MessagePackObjectDictionary dict)
        {
            Tell_Response_PeerList(from);
        }
        void OnRecv_Response_PeerList(IModulePipeline from, MessagePackObjectDictionary dict)
        {
            var nodes = dict["nodes"].AsList();
            foreach (var n in nodes)
            {
                var subobj = n.AsDictionary();
                CanLinkObj canlink = new CanLinkObj();
                canlink.fromType = LinkFromEnum.ResponsePeers;
                canlink.from = from.system.Remote;
                canlink.ID = subobj["id"].AsBinary();
                canlink.remote = IPEndPoint.Parse(subobj["pubep"].AsString());
                canlink.PublicKey = subobj["pubkey"].AsBinary();
                
                if (this.listCanlink.Contains(canlink))//检查我的连接列表
                {
                    var link = this.listCanlink.Getqueue(canlink.remote.ToString());
                    link.ID = canlink.ID;
                    link.PublicKey = canlink.PublicKey;
                }
                else
                {
                    this.listCanlink.Enqueue(canlink);
                }
            }
        }
        /// <summary>
        /// 交易业务说明
        /// SendRaw是将交易数据从 任意节点（任意节点包括共识节点） 流转到 一个共识节点的过程
        /// 
        /// 1.流转说明
        /// 如果SendRaw发起者就是共识节点，则不需要再流转了，直接处理交易
        /// 如果不是，只流转给自己连接的节点中plevel 小于等于自己的，这个没见到
        /// 2.sendraw参数
        /// SendRaw(byte[] message,SignData)
        /// 发送交易只需要两个数据，一个 bytearray，一个signdata，一定是byte[]，不要整什么MessagePackObjectList
        /// signdata参考neo的最简形态，我们只支持最简的 txpool.TransactionSign 写在那里的
        /// 3.流转验证
        /// 每一次流转，都要检查sendraw 数据对不对
        /// signdata里面的vscript 包含公钥，iscript包含签名数据，执行ecc验签，不通过不转发，还记录本地黑名单，第二次收到，都不需要验证
        /// 
        /// 4.txid
        /// 交易的id ，就是交易message 的 hash256，保持和neo兼容
        /// 
        /// 5.共识节点收到交易的处理
        /// 交易不是块，和块没有关系
        /// 先忽略共识过程，系统就一个共识节点，自己就是议长。议长干的第一件事是构造一个统一的交易内存池
        /// 收到交易，只需要存在内存里
        /// 我们先假设所有共识节点有同样的交易index，交易的index是由议长分配的，议长分配完id，告知所有的共识节点
        /// 
        /// 当前共识节点收到交易，给他分配一个index，就存在自己的内存池里，不需要存数据库。
        /// 分配了index的交易就可以全网广播，>=plevel
        /// 然后开一个定时器，定时从自己的内存池里挑一些交易，组装成块，广播
        /// 
        /// block 的header 包括当前块所包含的交易的hash
        /// block 的body 就是 所有的包含的交易的 message 和 signdata
        /// 只需要广播block的header，block的body 各个节点自己就可以组装
        /// 
        /// 6.错过广播
        /// 错过广播是很正常的，所以每个节点有一个高度设计，就是block的index
        /// 可以找任意高度大于自己的节点索要指定的block header(by block index or block id) 和 指定的hash(by txid)
        /// 
        /// 7.数据存储
        /// 对共识节点，仅当组装成块的时候，一次性写入 block header、block涉及的交易、当前高度，用db 的 writebatch 方式
        /// 刚收到的交易不写数据库，仅有随块一起写入的，并且已写入的交易，内存池里就不必保持了。
        /// 但是所有的txid->block index 的映射，内存池里要保持
        /// </summary>
        /// <param name="from"></param>
        /// <param name="dict"></param>
        void OnRecv_Post_SendRaw(IModulePipeline from, MessagePackObjectDictionary dict)
        {
            logger.Info($"------OnRecv_Post_SendRaw  From:{from?.system?.Remote?.ToString()??"Local"} -------");
            //验证交易合法性，合法就收
            var signData = SerializeHelper.DeserializeWithBinary<TransactionSign>(dict["signData"].AsBinary());
            bool sign = Helper_NEO.VerifySignature(dict["message"].AsBinary(), signData.IScript, signData.VScript);
            if (!sign)
            {
                return;
            }
            //收到消息后要么转发,要么保存
            if (this.isProved)
            {
                Transaction trans = new Transaction();
                trans.Index = this.txpool.MaxTransactionID;
                trans.message = dict["message"].AsBinary();
                trans.signdata = signData;
                lock (blockTimerLock)
                {
                    this.txpool.AddTx(trans);
                }
                //向内存池保存完，向全网广播这个交易
                foreach (var item in this.linkNodes)
                {
                    if (item.Value.hadJoin)
                    {
                        Tell_BoardCast_Tx(item.Value.remoteNode, dict["message"].AsBinary(), dict["signData"].AsBinary());
                    }
                }
            }
            else
            {
                //只流转给非记账节点，按照优先级，小于等于自己的其中一个
                LinkObj minLink = null;
                foreach (var item in this.linkNodes)
                {
                    if ((minLink == null || item.Value.pLevel < minLink.pLevel) && (item.Value.hadJoin))
                    {
                        minLink = item.Value;
                    }
                }
                if (minLink != null)
                {
                    Tell_SendRaw(minLink.remoteNode, dict["message"].AsBinary(), dict["signData"].AsBinary());
                }
            }
        }
        
        void OnRecv_BoradCast_PeerState(IModulePipeline from, MessagePackObjectDictionary dict)
        {
            var parentPleve = dict["plevel"].AsInt32();

            if (this.linkNodes.TryGetValue(from.system.PeerID, out LinkObj link))
            {
                this.getPlevelFromLinkObj(link, parentPleve);
            }
            //Console.WriteLine("# from:" + link.publicEndPoint + " plevel:" + link.pLevel + " node:" + this.config.PublicEndPoint + " plevel:" + this.pLevel);

            //if (this.pLevel > parentPleve||this.pLevel==-1)
            //{
            //    this.pLevel = parentPleve + 1;
            //}
        }
        void OnRecv_Post_TouchProvedPeer(IModulePipeline from, MessagePackObjectDictionary dict)
        {
            var pubep = dict["pubep"].AsString();
            var nodeid = dict["nodeid"].AsString();
            if (this.isProved)
            {
                if (nodeid != this.guid.ToString())
                {
                    //最终找到了记账节点
                    if (string.IsNullOrEmpty(pubep))
                    {
                        //本身就是记账人节点，直接返回
                        Tell_Response_Iamhere(from, this.config.PublicEndPoint.ToString());
                    }
                    else
                    {
                        var subPubep = pubep.Substring(pubep.IndexOf("$") + 1);
                        Tell_Response_ProvedRelay(from, subPubep, this.config.PublicEndPoint.ToString());
                    }
                }
                return;
            }
            this.linkNodes.TryGetValue(from.system.PeerID,out LinkObj link);

            pubep = string.IsNullOrEmpty(pubep)? link.publicEndPoint?.ToString()??string.Empty : (link.publicEndPoint?.ToString()??string.Empty + "$"+ pubep);

            bool isSend = false;
            var initAddr = pubep.Substring(pubep.LastIndexOf("$")+1, pubep.Length -1- pubep.LastIndexOf("$"));
            foreach (var item in this.linkNodes)
            {
                if (!from.system.Remote.Equals(item.Value.remoteNode.system.Remote) 
                    && !initAddr.Equals(item.Value.remoteNode.system.Remote.ToString())
                    && item.Value.hadJoin)
                {
                    Tell_Post_TouchProvedPeer(item.Value.remoteNode, pubep, nodeid);
                    isSend = true;
                    //System.Console.WriteLine("OnRecv_Post_TouchProvedPeer:" + item.Value.remoteNode.system.Remote.ToString());
                }                
            }
            if(!isSend)
            {
                //最终没有找到记账节点
                var subPubep = pubep.Substring(pubep.IndexOf("$")+1);
                Tell_Response_ProvedRelay(from, subPubep, string.Empty);
            }
        }
        void OnRecv_Response_Iamhere(IModulePipeline from, MessagePackObjectDictionary dict)
        {
            var link = this.linkNodes[from.system.PeerID];
            link.provedPubep = dict["provedpubep"].AsString();            
            link.isProved = dict["isProved"].AsBoolean();
            if (!ContainsRemote(link.publicEndPoint))
            {
                this.provedNodes[from.system.PeerID] = link;
            }
        }
        void OnRecv_Response_ProvedRelay(IModulePipeline from, MessagePackObjectDictionary dict)
        {
            var pubep = dict["pubep"].AsString();
            var provedpubep = dict["provedpubep"].AsString();
            var isProved = dict["isProved"].AsBoolean();

            if (pubep.Contains("$"))
            {
                var url = pubep.Substring(0, pubep.IndexOf("$"));
                var link = this.linkNodes.GetLinkNode(url);
                if (link != null)
                {
                    var subPubep = pubep.Substring(pubep.IndexOf("$"));
                    Tell_Response_ProvedRelay(link.remoteNode, subPubep, provedpubep);
                }
            }
            else if(!string.IsNullOrEmpty(provedpubep) || isProved)
            {
                var link = this.linkNodes.GetLinkNode(pubep);
                if (link != null)
                {
                    Tell_Response_Iamhere(link.remoteNode, provedpubep);
                }
            }
        }
        void OnRecv_BoardCast_Tx(IModulePipeline from, MessagePackObjectDictionary dict)
        {
            var signData = SerializeHelper.DeserializeWithBinary<TransactionSign>(dict["signData"].AsBinary());
            bool sign = Helper_NEO.VerifySignature(dict["message"].AsBinary(), signData.IScript, signData.VScript);
            if (!sign)
            {
                return;
            }
            Transaction trans = new Transaction();
            trans.Index = this.txpool.MaxTransactionID;
            trans.message = dict["message"].AsBinary();
            trans.signdata = signData;
            this.txpool.AddTx(trans);
        }

        void OnRecv_Request_BlockHeight(IModulePipeline from)
        {
            Tell_Response_BlockHeight(from);
        }
        void OnRecv_Response_BlockHeight(IModulePipeline from, MessagePackObjectDictionary dict)
        {
            var index = dict["blockIndex"].AsUInt64();
            if(index >= this.blockIndex)//blockIndex始终要比当前存在的block高度大一个
            {
                for (ulong i = this.blockIndex; i < index; i++)
                {
                    Tell_Request_Block(from, this.blockIndex);
                }
            }
        }
        void OnRecv_Request_Block(IModulePipeline from, MessagePackObjectDictionary dict)
        {
            var index = dict["blockIndex"].AsUInt64();
            var blockheader = this.blockChain.GetBlockHeader(index);
            Tell_Response_Block(from, blockheader);
        }
        void OnRecv_Response_Block(IModulePipeline from, MessagePackObjectDictionary dict)
        {
            var header = dict["blockHeader"].AsBinary();
            if (header != null)
            {
                var block = new Block();
                block.index = BitConverter.GetBytes(this.GetLastIndex());
                block.header = new BlockHeader();
                block.header.TxidsHash = header;
                this.blockChain.SaveBlock(block,this.blockIndex);
            }
        }
        private bool ContainsRemote(IPEndPoint ipEndPoint)
        {
            var linkRemote = ipEndPoint.ToString();
            foreach (var item in this.provedNodes.Values)
            {
                if ((item.publicEndPoint.ToString() == linkRemote)
                    ||(item.provedPubep == linkRemote))
                {
                    return true;
                }
            }
            return false;
        }

        //private IModulePipeline observer;
        //void OnRecv_IamObserver(IModulePipeline from, MessagePackObjectDictionary dict)
        //{
        //    observer = from;
        //}

        void OnRecv_Response_plevel(IModulePipeline from,MessagePackObjectDictionary dict)
        {
            var id=from.system.PeerID;
            var plevel = dict["plevel"].AsInt32();
            if(this.linkNodes.TryGetValue(id, out LinkObj obj))
            {
                logger.Info(" plevel:"+ plevel+ "from:" + obj.publicEndPoint);
            }
        }
        void OnRecv_Request_plevel(IModulePipeline from)
        {
            this.Tell_Response_plevel(from);
        }



        void onRecv_FakeRemote(MessagePackObjectDictionary dict)
        {
            if(dict.TryGetValue("msg",out MessagePackObject msg))
            {
                var msgdict = msg.AsDictionary();
                var cmd = (CmdList)msgdict["cmd"].AsUInt16();
                this.OnReceiveMsgFromRemote(null,cmd, msgdict);//null应该修改为自己到自己的pipeline,不然在处理消息过程用到from的时候，可能报错。
            }
        }
        void OnRecv_SendMsg(IModulePipeline from, MessagePackObjectDictionary dict)
        {
            var msg = dict["msg"];
            var endpointStr = dict["target"].AsString();
            if (this.linkIDs.TryGetValue(endpointStr, out ulong peer))
            {
                this.linkNodes[peer].remoteNode.Tell(msg);
            }
        }
        void OnRecv_Request_ConnectTo(IModulePipeline from, MessagePackObjectDictionary dict)
        {
            var endpointstr = dict["endpoint"].AsString();
            if(IPEndPoint.TryParse(endpointstr, out IPEndPoint endpoint))
            {
                this.ConnectOne(endpoint);
            }
        }
        void OnRecv_Request_Disconnect(IModulePipeline from, MessagePackObjectDictionary dict)
        {
            var endpointstr = dict["endpoint"].AsString();
            this.DisconnectOneByEndpoint(endpointstr);
        }
        void OnRecv_BoardCast_LosePlevel(IModulePipeline from, MessagePackObjectDictionary dict)
        {
            if(this.linkNodes.TryGetValue(from.system.PeerID,out LinkObj obj))
            {
                var beAffected=this.losePlevelFromLinkObj(obj);
                if(!beAffected)
                {
                    if(this.pLevel!=-1)
                    {
                        this.Tell_BoradCast_PeerState(from);
                    }
                }
            }
        }
        

    }

    public static class LinkNodeFunc
    {
        public static LinkObj GetLinkNode(this System.Collections.Concurrent.ConcurrentDictionary<UInt64, LinkObj> dic,string pubep)
        {
            foreach(var item in dic)
            {
                if(item.Value?.publicEndPoint?.ToString() == pubep)
                {
                    return item.Value;
                }
            }
            return null;
        }
    }

}
