using allpet.peer.tcp;
using System;

namespace allpet.node
{
    public interface INode : IDisposable
    {
        void InitChain(string dbpath, Config_ChainInit info);//链初始化

        void StartNetwork();//启动网络


        UInt64 GetBlockCount();
        //获取节点对象
        allpet.peer.tcp.IPeer Newwork
        {
            get;
        }
    }
    public class Node
    {
        static INode CreateNode()
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
    class PeerNode : INode
    {

        public PeerNode()
        {

        }

        allpet.db.simple.DB db;
        readonly static byte[] TableID_SystemInfo = new byte[] { 0x01, 0x01 };
        readonly static byte[] Key_SystemInfo_BlockCount = new byte[] { 0x01 };
        readonly static byte[] Key_SystemInfo_TXCount = new byte[] { 0x01 };

        readonly static byte[] TableID_Blocks = new byte[] { 0x01, 0x02 };
        readonly static byte[] TableID_TXs = new byte[] { 0x01, 0x03 };
        readonly static byte[] TableID_Owners = new byte[] { 0x01, 0x04 };

        public IPeer Newwork
        {
            get;
            private set;
        }


        public ulong GetBlockCount()
        {
            var data = db.GetDirect(TableID_SystemInfo, Key_SystemInfo_BlockCount);
            if (data == null || data.Length == 0)
                return 0;
            UInt64 blockcount = BitConverter.ToUInt64(data);
            return blockcount;
        }



        public void InitChain(string dbpath, Config_ChainInit info)
        {
            if (this.db != null)
                throw new Exception("already had inited.");
            throw new NotImplementedException();
        }
        public void StartNetwork()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            this.db.Dispose();
            this.db = null;
        }
    }
}
