using System;
using System.Collections.Generic;
using System.Text;

namespace AllPet.Module.Node
{
    [Serializable]
    public class Transaction
    {
        public UInt64 Index;//交易索引
        public byte[] message;//交易体
        public TransactionSign signdata;
    }
    [Serializable]
    public class TransactionSign
    {
        public byte[] VScript;//只支持checksig公钥
        public byte[] IScript;//push signdata
    }
    //这个模块要对接数据库，TxPool要被保存
    public class TXPool
    {
        public static int txpoolcount = 0;//测试用
        System.Collections.Concurrent.ConcurrentDictionary<UInt64, Hash256> map_tx2index = new System.Collections.Concurrent.ConcurrentDictionary<ulong, Hash256>();
        System.Collections.Concurrent.ConcurrentDictionary<Hash256, Transaction> TXData = new System.Collections.Concurrent.ConcurrentDictionary<Hash256, Transaction>();
        public UInt64 MaxTransactionID
        {
            get;
            private set;
        }
        public void AddTx(Transaction trans)
        {
            //第一步，验证交易合法性，合法就收
            
            //第二步，验证Hash是否已经存在
            var txid = Helper_NEO.CalcHash256(trans.message);
            if(TXData.ContainsKey(txid))
            {
                txpoolcount++;
                return;
            }
            //第三步，放进去并调整MaxTransactionID
            TXData.TryAdd(txid, trans);
            map_tx2index.TryAdd(MaxTransactionID, txid);
            MaxTransactionID++;
        }
        public Transaction GetTxByIndex(UInt64 id)
        {
            var hash = map_tx2index[id];
            return GetTxByHash(hash);
        }
        public Transaction GetTxByHash(Hash256 hash)
        {
            return TXData[hash];
        }
        public System.Collections.Concurrent.ConcurrentDictionary<Hash256, Transaction> Txs
        {
            get
            {
                return this.TXData;
            }
            private set { }
        }
        public void Clear()
        {
            this.TXData.Clear();
        }
    }
}
