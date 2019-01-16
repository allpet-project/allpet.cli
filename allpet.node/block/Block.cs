using System;
using System.Collections.Generic;
using System.Text;

namespace allpet.node.block
{

    public class HashPoint
    {
        public byte[] CurrentHash
        {
            get;
            private set;
        }

        [ThreadStatic]
        static byte[] BufLink;
        public unsafe void AddHash(byte[] hash)
        {
            if (BufLink == null)
                BufLink = new byte[64];
            fixed (byte* pbuf = BufLink, phash = hash)
            {
                if (CurrentHash == null)
                {
                    Buffer.MemoryCopy(phash, pbuf + 32, 32, 32);
                    CurrentHash = hash;
                }
                else
                {
                    Buffer.MemoryCopy(pbuf + 32, pbuf, 32, 32);
                    Buffer.MemoryCopy(phash, pbuf + 32, 32, 32);
                    CurrentHash = Allpet.Helper.CalcSha256(BufLink, 0, 64);
                }
            }
        }
    }

    public class HashList : List<byte[]>
    {
        public HashPoint HashPoint => new HashPoint();
        public void AddHash(byte[] hash)
        {
            HashPoint.AddHash(hash);
            this.Add(hash);
        }
    }
    public class BlockHeader
    {
        public byte[] lastBlockHash;
        public byte[] nonce;
        public byte[] TxidsHash;
    }
    public class BlockSign
    {

    }
    public class Block
    {
        public BlockHeader header;
        public BlockSign sign;
        public byte[] ToBytes()
        {
            return null;
        }
    }

    public enum TXParamType
    {
        //常量
        Const_UINT64 = 0x01,
        Const_BigInteger,
        Const_String,
        Const_Bytes,
        //特殊
        Storage_Writer,//改变存储区
        Storage_Adder,//存储区加法器
    }

    public class TXParamDesc
    {
        public TXParamType type;//Param類型
        public string key;
        public byte[] value;
    }

    //调用交易,一切皆是调用
    public class TXBody
    {
        public byte tag;
        public byte[] script;
        public string method;
        public TXParamDesc[] _params;
    }
    public class Witness
    {
        public byte[] iScript; //push signdata，裏面是簽名，和neo保持一致，固定這麽來
        public byte[] vScript; //push 公鑰 ，checksig，中間一部分是公鑰
    }
    public class TransAction
    {
        public UInt64 txIndex;
        public byte[] txHash;
        public TXBody body;
        public Witness witness;
        public void Sign(Allpet.Helper_NEO.Signer signer)
        {
            //var data=            body.ToBytes();
            //var hash = data.toHash();
            //var signdata = signer.SighHash(hash);

        }
    }

    public class BlockChain : IDisposable
    {
        public void Dispose()
        {
            if (this.db != null)
                this.db.Dispose();
            this.db = null;
        }
        allpet.db.simple.DB db;
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
