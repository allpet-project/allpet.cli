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
        private long lastIndex = -1;
        private ulong GetLastIndex()
        {
            lock(this)
            {
                lastIndex++;
            }
            return (ulong)lastIndex;
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
            foreach(var item in _params)
            {
                var hash256 = Helper.CalcSha256(item.AsBinary());
                var index = this.GetLastIndex();
                TransAction tx = new TransAction();
                tx.txIndex = index;
                tx.txHash = hash256;
                tx.body = new TXBody()
                {
                     script = item.AsBinary()                     
                };
                BlockChain blockChain = new BlockChain();
                
                blockChain.SetTx(index, tx);
            }
            var result = new MessagePackObject();
            return new RPC_Result(result);
        }
    }
}
