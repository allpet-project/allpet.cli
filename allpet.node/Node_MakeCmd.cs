using AllPet.Pipeline.MsgPack;
using MsgPack;
using System;
using System.Collections.Generic;
using System.Text;

namespace AllPet.Module
{
    partial class Module_Node : Module_MsgPack
    {
        public MessagePackObject makeCmd_ConnectTo(string endpoint)
        {
            var dict = new MessagePackObjectDictionary();
            dict["cmd"] = (UInt16)CmdList.Request_ConnectTo;
            dict["endpoint"] = endpoint;
            return new MessagePackObject(dict);
        }
        public MessagePackObject makeCmd_DisconnectTo(string endpoint)
        {
            var dict = new MessagePackObjectDictionary();
            dict["cmd"] = (UInt16)CmdList.Request_DisconnectTo;
            dict["endpoint"] = endpoint;
            return new MessagePackObject(dict);
        }

        public MessagePackObject makeCmd_SendMsg(string targetEndpoint, MessagePackObject msg)
        {
            var dict = new MessagePackObjectDictionary();
            dict["cmd"] = (UInt16)CmdList.Request_SendMsg;
            dict["msg"] = msg;
            dict["target"] = targetEndpoint;
            return new MessagePackObject(dict);
        }

        public MessagePackObject makeCmd_FakeRemote(MessagePackObject msg)
        {
            var dict = new MessagePackObjectDictionary();
            dict["cmd"] = (UInt16)CmdList.Fake_Remote;
            dict["msg"] = msg;
            return new MessagePackObject(dict);
        }


    }
}
