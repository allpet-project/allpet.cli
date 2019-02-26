using AllPet.Module.Node;
using System;
using System.Collections.Generic;
using System.Text;

namespace AllPet.Module.block
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
                    CurrentHash = AllPet.Helper.CalcSha256(BufLink, 0, 64);
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
    [Serializable]
    public class BlockHeader
    {
        public byte[] lastBlockHash;
        public byte[] nonce;
        public byte[] TxidsHash;
        public BlockType blockType;
        public BlockHeader(BlockType blockType = BlockType.Blank)
        {
            this.blockType = blockType;
        }
    }
    public class BlockSign
    {

    }
    public class Block
    {
        public byte[] data = new byte[] { 1 };
        public BlockHeader header;
        public BlockSign sign;
        public byte[] index;        
        public System.Collections.Concurrent.ConcurrentDictionary<Hash256, Transaction> TXData = new System.Collections.Concurrent.ConcurrentDictionary<Hash256, Transaction>();
        
        public byte[] ToBytes()
        {
            return data;
        }
    }

    public enum BlockType
    {
        Blank,
        TxData
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
        public void Sign(AllPet.Helper_NEO.Signer signer)
        {
            //var data=            body.ToBytes();
            //var hash = data.toHash();
            //var signdata = signer.SighHash(hash);

        }
    }

}
