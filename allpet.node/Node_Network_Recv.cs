using AllPet.Pipeline;
using AllPet.Pipeline.MsgPack;
using MsgPack;
using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using AllPet.Common;
using System.Linq;
using System.Threading.Tasks;

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


            //----------------
            bool down_BoradCast = false;
            if (dict.ContainsKey("isproved"))
            {
                var endpoint = link.publicEndPoint.ToString();
                if(this.addLinkedProvedNode(endpoint, from.system.PeerID))
                {
                    down_BoradCast = true;
                }
            }
            else
            {
                if (this.pLevel >= link.pLevel)
                {//当优先级>=自己的时候才将对方能连接到的共识节点同步过来,当自己的优先级变更的（接入/断开）,也要维护linkprovedlist
                    var linkedProvedNodes = dict["provednodes"].AsList();
                    var peer = from.system.PeerID;
                    if (this.addLinkedProvedNode(linkedProvedNodes, peer))
                    {
                        down_BoradCast = true;
                    }
                }
            }

            //如果连接上了，要更新自己的优先级
            if (this.pLevel < 0)
            {
                if (link.pLevel >= 0)//加入的节点优先级有效，且本身节点不是记账人
                {
                    this.pLevel = link.pLevel + 1;
                }
            }
            else if(this.pLevel >= link.pLevel)
            {
                this.pLevel =Math.Min(link.pLevel + 1,this.pLevel);
                //如果是变更，则广播低优先级节点
                foreach (var item in this.linkNodes)
                {
                    if (item.Value.hadJoin && (item.Value.pLevel < this.pLevel||(item.Value.pLevel == this.pLevel&& down_BoradCast)))
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
            bool isSended = false; ;
            foreach (var item in this.provedNodes)
            {
                if (item.Value.hadJoin)
                {
                    Tell_SendRaw(item.Value.remoteNode, dict);
                    isSended = true;
                    break;
                }
            }
            if (!isSended)
            {
                LinkObj minLink = null;
                foreach (var item in this.linkNodes)
                {
                    if(minLink == null || item.Value.pLevel < minLink.pLevel)
                    {
                        minLink = item.Value;
                    }                    
                }
                if (minLink != null)
                {
                    Tell_SendRaw(minLink.remoteNode, dict);
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
            var linkedprovedNode = dict["provednodes"].AsList();
            this.addLinkedProvedNode(linkedprovedNode,from.system.PeerID);
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

        /// <summary>
        /// key：共识节点的publicendpoint  value：peer list
        /// </summary>
        private System.Collections.Concurrent.ConcurrentDictionary<string, System.Collections.Concurrent.ConcurrentQueue<ulong>> linkProvedDic = new System.Collections.Concurrent.ConcurrentDictionary<string, System.Collections.Concurrent.ConcurrentQueue<ulong>>();
        //private System.Collections.Concurrent.ConcurrentQueue<MessagePackObject> waitSendMsgs = new System.Collections.Concurrent.ConcurrentQueue<MessagePackObject>();
        //private Action<IModulePipeline,string> OnkownPathToprovedNode;

        bool addLinkedProvedNode(string endpoint, ulong peer)
        {
            if (!this.linkProvedDic.ContainsKey(endpoint))
            {
                this.linkProvedDic[endpoint] = new System.Collections.Concurrent.ConcurrentQueue<ulong>();
            }
            if (!this.linkProvedDic[endpoint].Contains(peer))
            {
                this.linkProvedDic[endpoint].Enqueue(peer);
                return true;
            }
            return false;
        }

        bool addLinkedProvedNode(IList<MessagePackObject> linkedProvedNodes, ulong peer)
        {
            var beAdd = false;
            foreach (var endpoint in linkedProvedNodes)
            {
                if (this.addLinkedProvedNode(endpoint.AsString(), peer))
                {
                    beAdd = true;
                }
            }
            return beAdd;
        }



        void OnRecv_Request_SendOneMsg(IModulePipeline from, MessagePackObjectDictionary dict)
        {
            var fromendpoint = dict["from"].AsString();
            if (this.isProved)
            {
                logger.Info("收到一条消息！！！from:"+ fromendpoint+" proved");

                //-------------------------发回执？？
            }else
            {
                if (from != null)
                {
                    var returnpeer = dict["returnpeer"].AsList();
                    returnpeer.Add(from.system.PeerID);
                    dict["returnpeer"] = returnpeer.ToArray();
                }

                var msg = new MessagePackObject(dict);
                //if (this.linkProvedDic.IsEmpty)
                //{//找共识节点，将路径保存下来

                //    foreach (var node in this.linkNodes)
                //    {
                //        if (node.Value.pLevel > this.pLevel)
                //        {
                //            Tell_Request_FindProvedNode(node.Value.remoteNode, null);
                //        }
                //    }
                //    waitSendMsgs.Enqueue(msg);
                //    this.OnkownPathToprovedNode += (remote,provedNode) =>
                //    {
                //        while (waitSendMsgs.Count > 0)
                //        {
                //            if (waitSendMsgs.TryDequeue(out MessagePackObject onemsg))
                //            {
                //                var dictmsg = onemsg.AsDictionary();
                //                dictmsg["proved"] = new MessagePackObject(provedNode);
                //                this.Tell_Request_SendOneMsg(remote, new MessagePackObject(dictmsg));
                //            }
                //        }
                //    };
                //}
                //else
                {
                    if (this.SendMsg(msg))
                    {
                        logger.Info("发送msg 失败！！未告诉下个节点msg.发送时机： OnRecv_SendOneMsgToProvedNode");
                    }
                }
            }
        }

        private bool SendMsg(MessagePackObject msg)
        {
            foreach(var Linknode in this.linkProvedDic)
            {
                var nodeArr = Linknode.Value;
                foreach(var peer in nodeArr)
                {
                    if(this.linkNodes.TryGetValue(peer,out LinkObj obj))
                    {
                        var dict = msg.AsDictionary();
                        dict["proved"] =new MessagePackObject(Linknode.Key);
                        this.Tell_Request_SendOneMsg(obj.remoteNode,new MessagePackObject(dict));
                        return true;
                    }
                }
            }
            return false;
        }

        void OnRecv_Request_FindProvedNode(IModulePipeline from, MessagePackObjectDictionary dict)
        {
            var returnpeer = dict["returnpeer"].AsList();
            if (this.isProved)
            {
                this.Tell_Response_FindProvedNode(from, returnpeer);
            }else
            {
                if (this.linkProvedDic.IsEmpty)
                {
                    returnpeer.Add(from.system.PeerID);
                    foreach (var node in this.linkNodes)
                    {
                        if (node.Value.pLevel > this.pLevel)
                        {
                            Tell_Request_FindProvedNode(node.Value.remoteNode,returnpeer);
                        }
                    }
                }
                else
                {
                    this.Tell_Response_FindProvedNode(from, returnpeer);
                }
            }
        }

        void OnRecv_Response_FindProvedNode(IModulePipeline from, MessagePackObjectDictionary dict)
        {
            var nodes = dict["nodes"].AsList();
            foreach (var node in nodes)
            {
                var nodedic=node.AsDictionary();
                string id=nodedic["id"].AsString();
                if(!this.linkProvedDic.ContainsKey(id))
                {
                    this.linkProvedDic[id] = new System.Collections.Concurrent.ConcurrentQueue<ulong>();
                }
                //var pathList = nodedic["paths"].AsList();
                //foreach(var path in pathList)
                //{
                //    var pathstr=path.AsString();
                //    this.linkProvedList[id].Add(from.system.PeerID+"/"+pathstr);
                //}
                if(!this.linkProvedDic[id].Contains(from.system.PeerID))
                {
                    this.linkProvedDic[id].Enqueue(from.system.PeerID);
                    if(this.OnkownPathToprovedNode!=null)
                    {
                        this.OnkownPathToprovedNode(from, id);
                    }
                }
            }

            if (dict.ContainsKey("returnpeer"))
            {
                var returnpeer = dict["returnpeer"].AsList();
                var peer = returnpeer[returnpeer.Count-1].AsUInt64();
                returnpeer.Remove(peer);

                if(this.linkNodes.TryGetValue(peer, out LinkObj linkobj))
                {
                    this.Tell_Response_FindProvedNode(linkobj.remoteNode, returnpeer);
                }
            }
        }
    }
    /// <summary>
    /// 能够连接到的共识节点
    /// </summary>
    //public class LinkedProvedNode
    //{
    //    public Hash256 id;
    //    public string path;
    //}

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
