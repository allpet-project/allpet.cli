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
            link.pLeve = dict["pleve"].AsInt32();                     

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
            if (this.pLeve < 0)
            {
                if (link.pLeve >= 0)//加入的节点优先级有效，且本身节点不是记账人
                {
                    this.pLeve = link.pLeve + 1;
                }
            }
            else if(this.pLeve > link.pLeve)
            {
                this.pLeve = link.pLeve + 1;
                //如果是变更，则广播低优先级节点
                foreach (var item in this.linkNodes)
                {
                    if (item.Value.hadJoin && item.Value.pLeve < this.pLeve)
                    {
                        Tell_BoradCast_PeerState(item.Value.remoteNode);
                    }
                }
            }

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
                    Console.WriteLine("OnRecv_Response_PeerList------------------------------->");
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
                    if(minLink == null || item.Value.pLeve < minLink.pLeve)
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
            if(this.pLeve > parentPleve)
            {
                this.pLeve = parentPleve+1;
            }
        }
        void OnRecv_Post_TouchProvedPeer(IModulePipeline from, MessagePackObjectDictionary dict)
        {
            var pubep = dict["pubep"].AsString();
            if (this.isProved)
            {
                if (!pubep.Contains("$"))
                {
                    //本身就是记账人节点，直接返回
                    Tell_Response_Iamhere(from, this.config.PublicEndPoint.ToString());
                    return;
                }
                else
                {
                    var subPubep = pubep.Substring(pubep.IndexOf("$"));
                    Tell_Response_ProvedRelay(from, subPubep,this.config.PublicEndPoint.ToString());
                }
            }
            pubep += "$" + this.config.PublicEndPoint.ToString();

            System.Console.WriteLine("OnRecv_Post_TouchProvedPeer  from:" + from.system.Remote.ToString());
            foreach (var item in this.linkNodes)
            {
                //Tell_Post_TouchProvedPeer(item.Value.remoteNode, pubep);
                System.Console.WriteLine("OnRecv_Post_TouchProvedPeer:" + item.Value.remoteNode.system.Remote.ToString());
            }
        }
        void OnRecv_Response_Iamhere(IModulePipeline from, MessagePackObjectDictionary dict)
        {
            var link = this.linkNodes[from.system.PeerID];
            link.provedPubep = dict["provedpubep"].AsString();
            link.isProved = dict["isProved"].AsBoolean();
            this.provedNodes[from.system.PeerID] = link;
        }
        void OnRecv_Response_ProvedRelay(IModulePipeline from, MessagePackObjectDictionary dict)
        {
            var pubep = dict["pubep"].AsString();
            var provedpubep = dict["provedpubep"].AsString();
            var isProved = dict["isProved"].AsBoolean();

            var url = pubep.Substring(0, pubep.IndexOf("$"));
            var remote = this.GetPipeline(url);

            if(isProved)
            {
                Tell_Response_Iamhere(remote, provedpubep);
            }
            else
            {
                var subPubep = pubep.Substring(pubep.IndexOf("$"));
                Tell_Response_ProvedRelay(remote, subPubep, provedpubep);
            }
        }
        private bool ContainsRemote(IPEndPoint ipEndPoint)
        {
            var linkRemote = ipEndPoint.ToString();
            foreach (var item in this.provedNodes)
            {
                if (item.Value.publicEndPoint.ToString() == linkRemote)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
