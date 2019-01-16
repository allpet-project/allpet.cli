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
    public class Block
    {
        public HashList txids;
        public byte[] MerkelRoot;
    }
    public class BlockChain
    {
        //MerklePoint point=new MerklePoint(firstblock)
    }
}
