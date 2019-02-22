using AllPet.Pipeline;
using AllPet.Pipeline.MsgPack;
using MsgPack;
using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using AllPet.Common;
using System.Linq;
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
            link.pLevel = dict["pleve"].AsInt32();                     

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
            if (this.pLevel < 0)
            {
                if (link.pLevel >= 0)//加入的节点优先级有效，且本身节点不是记账人
                {
                    this.pLevel = link.pLevel + 1;
                }
            }
            else if(this.pLevel > link.pLevel)
            {
                this.pLevel = link.pLevel + 1;
                //如果是变更，则广播低优先级节点
                foreach (var item in this.linkNodes)
                {
                    if (item.Value.hadJoin && item.Value.pLevel < this.pLevel)
                    {
                        Tell_BoradCast_PeerState(item.Value.remoteNode);
                    }
                }
            }
            System.Console.WriteLine($"node:{this.config.PublicEndPoint}    pLeve:{this.pLevel}    isProved:{this.isProved}");
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
        void OnRecv_Post_SendRaw(IModulePipeline from, MessagePackObjectDictionary dict)
        {
            var nodeid = dict.ContainsKey("nodeid") ?dict["nodeid"].AsString():string.Empty;
            logger.Info($"------OnRecv_Post_SendRaw  From:{from?.system?.Remote?.ToString()??"Local"}  nodeid:{nodeid}-------");

            //收到消息后要么转发,要么保存
            if (this.isProved)
            {                
                //如果不是自己发过来的，就保存
                if (this.guid.ToString() != nodeid)
                {
                    SaveBlockChain(dict["rawdata"].AsList());
                }
            }
            else
            {
                bool isSended = false;
                var list = this.provedNodes.OrderBy(x=>x.Value.pLevel);
                foreach (var item in list)
                {
                    if (item.Value.hadJoin)
                    {
                        Tell_SendRaw(item.Value.remoteNode, dict["rawdata"].AsList(), dict["nodeid"].AsString());
                        isSended = true;
                        break;
                    }
                }
                if (!isSended)
                {
                    var linkList = this.linkNodes.OrderBy(x => x.Value.pLevel);
                    foreach (var item in this.linkNodes)
                    {
                        if (item.Value.hadJoin)
                        {
                            Tell_SendRaw(item.Value.remoteNode, dict["rawdata"].AsList(), dict["nodeid"].AsString());
                            break;
                        }
                    }
                }
            }
        }
        void OnRecv_BoradCast_PeerState(IModulePipeline from, MessagePackObjectDictionary dict)
        {
            var parentPleve = dict["pleve"].AsInt32();
            if(this.pLevel > parentPleve)
            {
                this.pLevel = parentPleve+1;
            }
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
        private bool ContainsRemote(IPEndPoint ipEndPoint)
        {
            var linkRemote = ipEndPoint.ToString();
            foreach (var item in this.provedNodes)
            {
                if ((item.Value.publicEndPoint.ToString() == linkRemote)
                    ||(item.Value.provedPubep == linkRemote))
                {
                    return true;
                }
            }
            return false;
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
