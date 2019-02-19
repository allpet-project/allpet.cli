﻿using AllPet.Pipeline;
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
        Response_ProvedRelay,//告知请求寻找一个证明节点的中继
        Post_SendRaw,//产生新的消息

        BoradCast_PeerState,//一个节点证明了他自己,当证明或者优先级发生改变的时候，向所有下级节点广播
        BoardCast_NewBlock,//新的块产生了

        /// <summary>
        /// RPC开头的都是对称响应式的消息，收到的命令中必须有一个id，必须返回发送，返回的id就是收到的id
        /// </summary>
        RPC = 0x0300,

    }
    public partial class Module_Node : Module_MsgPack
    {

        public override void OnTell(IModulePipeline from, MessagePackObject? obj)
        {
            var dict = obj.Value.AsDictionary();
            var cmd = (CmdList)dict["cmd"].AsUInt16();
            //rpc消息无所谓本地还是远程
            if (cmd == CmdList.RPC)
            {
                MessagePackObjectDictionary msgBack = new MessagePackObjectDictionary();
                msgBack["cmd"] = (UInt16)cmd;
                msgBack["id"] = dict["id"];
                string method = dict["method"].AsString();
                msgBack["method"] = method;
                var _params = dict.ContainsKey("params") ? dict["params"].AsList() : null;
                RPC_Result result = null;
                switch (method)
                {
                    case "listpeer":
                        result = RPC_ListPeer(_params);
                        break;
                    case "sendrawtransaction":
                        result = RPC_SendRawTransaction(_params);
                        break;
                    case "gettransactioncount":
                        result = RPC_GetTXCount(_params);
                        break;
                    case "gettransaction":
                        result = RPC_GetTX(_params);
                        break;
                    default:
                        result = new RPC_Result(null, -100, "not found that RPC command.");
                        break;
                }
                if (result.error_code != 0)
                {
                    msgBack["error"] = result.error_msg;
                    msgBack["errorcode"] = result.error_code;
                }
                if (result.result != null)
                    msgBack["result"] = result.result.Value;

                from.Tell(new MessagePackObject(msgBack));
                //rpc消息处理完
            }
            else if (from == null || from.IsLocal)//本地发来的消息
            {
                logger.Info("local msg:" + obj.Value.ToString());

                switch (cmd)
                {
                    case CmdList.Local_Cmd:
                        {
                            var _params = dict["params"].AsList();
                            if (_params.Count > 0)
                            {
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
                                            logger.Info("peer=" + n.remoteNode.system.PeerID + " publickey=" + publickey + " pubep=" + n.publicEndPoint+" beAccepted:"+n.beAccepted);
                                        }
                                    }

                                }
                                if (_cmd == "proved.list")
                                {
                                    foreach (var n in this.provedNodes.Values)
                                    {
                                        var publickey = n.PublicKey == null ? null : Helper.Bytes2HexString(n.PublicKey);
                                        logger.Info("proved=" + n.remoteNode.system.Remote.ToString() + " public=" + publickey + " pubep=" + n.publicEndPoint);
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
            else //远程发来的消息
            {

                if (this.linkNodes.TryGetValue(from.system.PeerID, out LinkObj link) == false)
                {
                    linkNodes[from.system.PeerID] = new LinkObj()
                    {
                        ID = null,
                        remoteNode = from,
                        publicEndPoint = null,
                        beAccepted=true
                    };
                    RegNetEvent(from.system);
                }
                logger.Info("remote msg:" + obj.Value.ToString());
                switch (cmd)
                {
                    case CmdList.Request_JoinPeer://告知其他节点我的存在，包括是不是共识节点之类的
                        {
                            OnRecv_RequestJoinPeer(from, dict);
                        }
                        break;
                    case CmdList.Response_AcceptJoin://同意他加入，并给他一个测试信息
                        {
                            OnRecv_ResponseAcceptJoin(from, dict);
                        }
                        break;
                    case CmdList.Request_ProvePeer:
                        {
                            OnRecv_RequestProvePeer(from, dict);
                        }
                        break;
                    case CmdList.Request_PeerList://询问一个节点所能到达的节点
                        OnRecv_Request_PeerList(from, dict);
                        break;
                    case CmdList.Response_PeerList://告知一个节点所能到达的节点
                        OnRecv_Response_PeerList(from, dict);
                        break;
                    case CmdList.Post_SendRaw:
                        OnRecv_Post_SendRaw(from, dict);
                        break;
                    case CmdList.BoradCast_PeerState://告知一个节点，节点状态变更（优先级）
                        {
                            OnRecv_BoradCast_PeerState(from, dict);
                        }
                        break;
                    case CmdList.Post_TouchProvedPeer:
                        {
                            OnRecv_Post_TouchProvedPeer(from, dict);
                        }
                        break;
                    case CmdList.Response_Iamhere://告知是否是记账节点或者是否能够到达共识节点
                        {
                            OnRecv_Response_Iamhere(from, dict);
                        }
                        break;
                    case CmdList.Response_ProvedRelay://告知是否是记账节点或者是否能够到达共识节点
                        {
                            OnRecv_Response_ProvedRelay(from, dict);
                        }
                        break;
                    default:
                        logger.Error("unknow msg:" + dict.ToString());
                        break;
                }
            }
        }
    }
}
