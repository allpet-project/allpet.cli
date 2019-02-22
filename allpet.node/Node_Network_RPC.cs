using AllPet.Pipeline;
using AllPet.Pipeline.MsgPack;
using MsgPack;
using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using AllPet.Common;
using System.Linq;
using AllPet.Module.block;
using System.Threading;

namespace AllPet.Module
{
    public class RPC_Result
    {
        public RPC_Result(MessagePackObject? value,int error_code=0,string error_msg=null)
        {
            this.result = value;
            this.error_code = error_code;
            this.error_msg = error_msg;
        }
        public string error_msg;
        public int error_code;
        public MessagePackObject? result;
    }
    public partial class Module_Node : Module_MsgPack
    {
        private ulong lastIndex ;
        private ulong blockIndex;//块的lastindex
        private ulong txIndex;//哪些tx的index被出块了
        private ulong blockCount;

        private ulong GetLastIndex()
        {
            var index = this.lastIndex;
            lock (blockTimerLock)
            {
                lastIndex++;
            }
            return index;
        }
        public RPC_Result RPC_ListPeer(IList<MessagePackObject> _params)
        {
            List<MessagePackObject> listPeer = new List<MessagePackObject>();
            foreach (var n in this.linkNodes.Values)
            {
                if (n.hadJoin)
                {
                    MessagePackObjectDictionary peerItem = new MessagePackObjectDictionary();
                    peerItem["endpoint"] = n.publicEndPoint.ToString();
                    peerItem["publickkey"] = n.PublicKey;

                    listPeer.Add(new MessagePackObject(peerItem));
                }
            }
            var result = new MessagePackObject(listPeer);
            return new RPC_Result(result);
        }
        public RPC_Result RPC_GetTXCount(IList<MessagePackObject> _params)
        {
            List<MessagePackObject> listPeer = new List<MessagePackObject>();
            foreach (var n in this.linkNodes.Values)
            {
                if (n.hadJoin)
                {
                    MessagePackObjectDictionary peerItem = new MessagePackObjectDictionary();
                    peerItem["endpoint"] = n.publicEndPoint.ToString();
                    peerItem["publickkey"] = n.PublicKey;

                    listPeer.Add(new MessagePackObject(peerItem));
                }
            }
            var result = new MessagePackObject(listPeer);
            return new RPC_Result(result);
        }
        public RPC_Result RPC_GetTX(IList<MessagePackObject> _params)
        {
            List<MessagePackObject> listPeer = new List<MessagePackObject>();
            foreach (var n in this.linkNodes.Values)
            {
                if (n.hadJoin)
                {
                    MessagePackObjectDictionary peerItem = new MessagePackObjectDictionary();
                    peerItem["endpoint"] = n.publicEndPoint.ToString();
                    peerItem["publickkey"] = n.PublicKey;

                    listPeer.Add(new MessagePackObject(peerItem));
                }
            }
            var result = new MessagePackObject(listPeer);
            return new RPC_Result(result);
        }
        public RPC_Result RPC_SendRawTransaction(IList<MessagePackObject> _params)
        {
            foreach (var item in _params)
            {
                var index = this.GetLastIndex();
                var hash256 = Helper.CalcSha256(item.AsBinary());                
                             
                this.blockChain.SetTx(this.lastIndex, index, hash256, item.AsBinary());
            }
            //this.Tell_SendRaw(this._System.GetPipeline(this,"this/node"),null);
            var result = new MessagePackObject(0);
            return new RPC_Result(result);
        }


        public RPC_Result Rpc_SendOneMsgToProvedNode(IModulePipeline frome,MessagePackObject _params)
        {
            ulong handle = 0;
            lock(MsgHandle.inc)
            {
                handle = MsgHandle.inc.Next();
            }
            MessagePackObjectDictionary dict = new MessagePackObjectDictionary();
            dict["msgid"] = handle;
            dict["msg"] = _params;
            dict["from"] = this.config.PublicEndPoint.ToString();//将自己的PublicEndPoint塞进去
            dict["returnpeer"] = new MessagePackObject(new MessagePackObject[0]);
            this.OnRecv_Request_SendOneMsg(null,new MessagePackObject(dict));
            var result = new MessagePackObject(handle);
            return new RPC_Result(result);
        }
    }


    public class MsgHandle
    {
        private static MsgHandle _inc;
        public static MsgHandle inc { get {
                if (_inc == null)
                {
                    _inc = new MsgHandle();
                }
                return _inc;
            } }

        UInt64 id = 0;
        public UInt64 Next()
        {
            return id++;
        }
    }
}
