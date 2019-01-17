using AllPet.node;
using System;
using System.Collections.Generic;
using System.Text;

namespace AllPet.node.block
{
    public class BlockChain : IDisposable
    {
        public void Dispose()
        {
            if (this.db != null)
                this.db.Dispose();
            this.db = null;
        }
        AllPet.db.simple.DB db;
        readonly static byte[] TableID_SystemInfo = new byte[] { 0x01, 0x01 };
        readonly static byte[] Key_SystemInfo_BlockCount = new byte[] { 0x01 };
        readonly static byte[] Key_SystemInfo_TXCount = new byte[] { 0x01 };

        readonly static byte[] TableID_Blocks = new byte[] { 0x01, 0x02 };
        readonly static byte[] TableID_TXs = new byte[] { 0x01, 0x03 };
        readonly static byte[] TableID_Owners = new byte[] { 0x01, 0x04 };

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
            db = new db.simple.DB();
            db.Open(dbpath, true);
            var blockcount = db.GetUInt64Direct(TableID_SystemInfo, Key_SystemInfo_BlockCount);
            if (blockcount == 0)
            {
                //insert first block
                //first block 会有几笔特殊交易
                //设置magicinfo
                //设置初始见证人
                //发行默认货币PET
                var block = new block.Block();

                db.PutDirect(TableID_Blocks, BitConverter.GetBytes(blockcount), block.ToBytes());
            }
        }

        public void SetTx(UInt64 id, TransAction tx)
        {

        }
        System.Collections.Concurrent.ConcurrentQueue<TransAction> queueTransAction;
        public void MakeBlock(UInt16 from, UInt64 to, params UInt64[] skip)
        {

        }
    }

}
