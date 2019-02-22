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
    partial class Module_Node : Module_MsgPack
    {
        void Tell_ReqJoinPeer(IModulePipeline remote)
        {
            var dict = new MessagePackObjectDictionary();
            dict["cmd"] = (UInt16)CmdList.Request_JoinPeer;
            dict["id"] = this.guid.data;
            dict["pubep"] = this.config.PublicEndPoint.ToString();
            //Console.WriteLine("Tell_ReqJoinPeer----->:"+ dict["pubep"]);
            dict["chaininfo"] = chainHash.data;
            remote.Tell(new MessagePackObject(dict));
        }
        void Tell_ResponseAcceptJoin(IModulePipeline remote)
        {
            var link = this.linkNodes[remote.system.PeerID];
            link.CheckInfo = Guid.NewGuid().ToByteArray();
            var dict = new MessagePackObjectDictionary();
            dict["cmd"] = (UInt16)CmdList.Response_AcceptJoin;
            dict["checkinfo"] = link.CheckInfo;
            dict["pleve"] = this.pLevel;//告诉对方我的优先级
            //选个挑战信息
            remote.Tell(new MessagePackObject(dict));
        }
        void Tell_Request_ProvePeer(IModulePipeline remote, byte[] addinfo, byte[] signdata)
        {
            var dict = new MessagePackObjectDictionary();
            dict["cmd"] = (UInt16)CmdList.Request_ProvePeer;
            dict["pubkey"] = this.pubkey;
            dict["addinfo"] = addinfo;
            dict["signdata"] = signdata;            
            remote.Tell(new MessagePackObject(dict));
        }
        void Tell_Request_PeerList(IModulePipeline remote)
        {
            var dict = new MessagePackObjectDictionary();
            dict["cmd"] = (UInt16)CmdList.Request_PeerList;
            remote.Tell(new MessagePackObject(dict));
        }
        void Tell_Response_PeerList(IModulePipeline remote)
        {
            var dict = new MessagePackObjectDictionary();
            dict["cmd"] = (UInt16)CmdList.Response_PeerList;
            var list = new List<MessagePackObject>();
            foreach (var n in this.linkNodes.Values)
            {
                if (n.hadJoin && n.publicEndPoint != null)
                {
                    var item = new MessagePackObjectDictionary();
                    item["pubep"] = n.publicEndPoint.ToString();
                    item["pubkey"] = n.PublicKey;
                    item["id"] = n.ID.data;
                    //var ipep = n.publicEndPoint.ToString();
                    list.Add(new MessagePackObject(item));
                }
            }
            dict["nodes"] = list.ToArray();

            remote.Tell(new MessagePackObject(dict));
        }

        void Tell_BoradCast_PeerState(IModulePipeline remote)
        {
            var dict = new MessagePackObjectDictionary();
            dict["cmd"] = (UInt16)CmdList.BoradCast_PeerState;
            dict["pleve"] = this.pLevel;//告诉对方我的优先级
            remote.Tell(new MessagePackObject(dict));
        }
        void Tell_SendRaw(IModulePipeline remote, MessagePackObjectDictionary dict)
        {
            
        }
        void Tell_Post_TouchProvedPeer(IModulePipeline remote,string pubep,string nodeid)
        {
            var dict = new MessagePackObjectDictionary();
            dict["cmd"] = (UInt16)CmdList.Post_TouchProvedPeer;
            dict["pubep"] = pubep;
            dict["nodeid"] = nodeid;
            remote.Tell(new MessagePackObject(dict));
        }
        void Tell_Response_Iamhere(IModulePipeline remote, string provedpubep)
        {
            var dict = new MessagePackObjectDictionary();
            dict["cmd"] = (UInt16)CmdList.Response_Iamhere;
            dict["provedpubep"] = provedpubep;
            dict["isProved"] = this.isProved;
            remote.Tell(new MessagePackObject(dict));
        }
        void Tell_Response_ProvedRelay(IModulePipeline remote,string pubep,string provedpubep)
        {
            var dict = new MessagePackObjectDictionary();
            dict["cmd"] = (UInt16)CmdList.Response_ProvedRelay;
            dict["pubep"] = pubep;
            dict["provedpubep"] = provedpubep;
            dict["isProved"] = this.isProved;
            remote.Tell(new MessagePackObject(dict));
        }

        /// <summary>
        /// 发送消息到共识节点
        /// </summary>
        /// <param name="remote"></param>
        /// <param name="dict"></param>
        void Tell_Reques_SendOneMsg(IModulePipeline remote, MessagePackObject dict)
        {
            var msg = dict.AsDictionary();
            msg["cmd"] = (UInt16)CmdList.Request_SendOneMsg;
            remote.Tell(new MessagePackObject(msg));
        }

        void Tell_Request_FindProvedNode(IModulePipeline remote,IList<MessagePackObject> returnpeer)
        {
            var dicts = new MessagePackObjectDictionary();
            dicts["cmd"] = (UInt16)CmdList.Request_FindProvedNode;
            if(returnpeer==null)
            {
                dicts["returnpeer"] = new MessagePackObject(new MessagePackObject[0]);
            }else
            {
                dicts["returnpeer"] = returnpeer.ToArray();
            }
            remote.Tell(new MessagePackObject(dicts));
        }

        /// <summary>
        /// 只告诉你我能找到哪个共识节点
        /// </summary>
        /// <param name="remote"></param>
        /// <param name="returnPeer"></param>
        void Tell_Response_FindProvedNode(IModulePipeline remote,IList<MessagePackObject> returnPeer)
        {
            var reDic = new MessagePackObjectDictionary();
            reDic["returnPeer"] = returnPeer.ToArray();

            if (this.isProved)
            {
                var nodearr = new List<MessagePackObject>();
                nodearr.Add(this.config.PublicEndPoint.ToString());
                reDic["nodes"] = nodearr.ToArray();
            }
            else
            {
                var nodearr = new List<MessagePackObject>();
                foreach (var item in this.linkProvedList)
                {
                    nodearr.Add(new MessagePackObject(item.Key));
                    //MessagePackObjectDictionary linknode = new MessagePackObjectDictionary();
                    //linknode["id"] = new MessagePackObject(item.Key);

                    //var listpath = new List<MessagePackObject>();
                    //foreach (var link in item.Value)
                    //{
                    //    var linkpath = new MessagePackObjectDictionary();
                    //    linkpath["path"] = link;
                    //    listpath.Add(new MessagePackObject(linkpath));
                    //}
                    //linknode["paths"] = listpath.ToArray();
                    //nodearr.Add(new MessagePackObject(linknode));
                }
                reDic["nodes"] = nodearr.ToArray();
            }
            remote.Tell(new MessagePackObject(reDic));

        }
    }
}
