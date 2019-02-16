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
            dict["isproved"] = this.isProved;//告诉对方我是记账节点
            dict["priority"] = this.priority;//告诉对方我的优先级
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
                    var ipep = n.publicEndPoint.ToString();
                    list.Add(new MessagePackObject(item));
                }
            }
            dict["nodes"] = list.ToArray();

            remote.Tell(new MessagePackObject(dict));
        }
    }
}
