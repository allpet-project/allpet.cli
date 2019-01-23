using AllPet.peer.tcp;
using AllPet.Pipeline;
using System;

namespace AllPet.node
{
    public interface INode :AllPet.Pipeline.IModuleInstance, IDisposable
    {
        void InitChain(string dbpath, Config_ChainInit info);//链初始化

        void StartNetwork();//启动网络


        UInt64 GetBlockCount();
        //获取节点对象
        AllPet.peer.tcp.IPeer Newwork
        {
            get;
        }
    }
    public class Node
    {
        public static INode CreateNode()
        {
            return new PeerNode();
        }
    }

    public class Config_ChainInit
    {
        public string MagicStr;
        public string[] InitOwner;
        public byte[] ToInitScript()
        {
            return null;
        }
    }
    class PeerNode : AllPet.Pipeline.Module, INode
    {

        public PeerNode()
        {

        }

        public override void OnStart()
        {
        }
        public override void OnTell(IModulePipeline from, byte[] data)
        {
        }
        block.BlockChain chain;

        public IPeer Newwork
        {
            get;
            private set;
        }


        public ulong GetBlockCount()
        {
            return chain.GetBlockCount();
        }



        public void InitChain(string dbpath, Config_ChainInit info)
        {
            chain = new block.BlockChain();
            chain.InitChain(dbpath, info);
        }
        public void StartNetwork()
        {

        }

        public void Dispose()
        {
            this.chain.Dispose();
            this.chain = null;
        }
    }
}
