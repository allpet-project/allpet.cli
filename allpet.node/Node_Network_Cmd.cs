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
    public enum CmdList : UInt16
    {
        Local_Cmd = 0x0000,

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

        BoradCast_PeerState,//一个节点证明了他自己
        BoardCast_NewBlock,//新的块产生了
    }
    public partial class Module_Node : Module_MsgPack
    {
        public override void OnTell(IModulePipeline from, MessagePackObject? obj)
        {
            if (from == null || from.IsLocal)//本地发来的消息
            {
                var dict = obj.Value.AsDictionary();
                var cmd = (CmdList)dict["cmd"].AsInt16();
                logger.Info("local msg:" + obj.Value.ToString());

                switch (cmd)
                {
                    case CmdList.Local_Cmd:
                        {
                            var _params = dict["params"].AsList();
                            var _cmd = _params[0].AsString();
                            if (_cmd == "peer.update")
                            {
                                foreach (var n in this.linkNodes.Values)
                                {
                                    if (n.hadJoin)
                                        this.Tell_Request_PeerList(n.remoteNode);
                                }
                            }
                            if (_cmd == "peer.list")
                            {
                                foreach (var n in this.linkNodes.Values)
                                {
                                    if (n.hadJoin)

                                    {
                                        var publickey = n.PublicKey == null ? null : Helper.Bytes2HexString(n.PublicKey);
                                        logger.Info("peer=" + n.remoteNode.system.Remote.ToString() + " public=" + publickey + " pubep=" + n.publicEndPoint);
                                    }
                                }

                            }
                        }
                        break;
                    default:
                        logger.Error("unknow msg:" + dict.ToString());
                        break;
                }
                return;
            }
            else
            {
                //远程发来的消息
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
                logger.Info("remote msg:" + obj.Value.ToString());
                switch (cmd)
                {
                    case CmdList.Request_JoinPeer:
                        {
                            OnRecv_RequestJoinPeer(from, dict);
                        }
                        break;
                    case CmdList.Response_AcceptJoin:
                        {
                            OnRecv_ResponseAcceptJoin(from, dict);
                        }
                        break;
                    case CmdList.Request_ProvePeer:
                        {
                            OnRecv_RequestProvePeer(from, dict);
                        }
                        break;
                    case CmdList.Request_PeerList:
                        OnRecv_Request_PeerList(from, dict);
                        break;
                    case CmdList.Response_PeerList:
                        OnRecv_Response_PeerList(from, dict);
                        break;
                    default:
                        logger.Error("unknow msg:" + dict.ToString());
                        break;
                }
            }
        }
    }
}
