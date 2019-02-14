﻿using System;
using System.Collections.Generic;
using System.Text;

namespace AllPet.Module.Node
{
    public class Transaction
    {
        public UInt64 Index;//交易索引
        public byte[] message;//交易体
        public TransactionSign signdata;
    }
    public class TransactionSign
    {
        public byte[] VScript;//只支持checksig公钥
        public byte[] IScript;//push signdata
    }
    //这个模块要对接数据库，TxPool要被保存
    public class TXPool
    {
        System.Collections.Concurrent.ConcurrentDictionary<UInt64, Hash256> map_tx2index;
        System.Collections.Concurrent.ConcurrentDictionary<Hash256, Transaction> TXData;
        public UInt64 MaxTransactionID
        {
            get;
            private set;
        }
        public void AddTx(Transaction trans)
        {
            //第一步，验证交易合法性，合法就收
            //第二步，验证Hash是否已经存在
            //第三步，放进去并调整MaxTransactionID
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
    }
}