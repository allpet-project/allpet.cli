using System;
using System.Collections.Generic;

namespace allpet.db.simple
{
    public class DB
    {
        IntPtr dbPtr;
        IntPtr defaultWriteOpPtr;
        public void Open(string path, bool createIfMissing = false)
        {
            if (dbPtr != IntPtr.Zero)
                throw new Exception("already open a db.");
            this.defaultWriteOpPtr = RocksDbSharp.Native.Instance.rocksdb_writeoptions_create();

            var HandleOption = RocksDbSharp.Native.Instance.rocksdb_options_create();
            if (createIfMissing)
            {
                RocksDbSharp.Native.Instance.rocksdb_options_set_create_if_missing(HandleOption, true);
            }
            RocksDbSharp.Native.Instance.rocksdb_options_set_compression(HandleOption, RocksDbSharp.CompressionTypeEnum.rocksdb_snappy_compression);
            //RocksDbSharp.DbOptions option = new RocksDbSharp.DbOptions();
            //option.SetCreateIfMissing(true);
            //option.SetCompression(RocksDbSharp.CompressionTypeEnum.rocksdb_snappy_compression);
            IntPtr handleDB = RocksDbSharp.Native.Instance.rocksdb_open(HandleOption, path);
            this.dbPtr = handleDB;

            snapshotLast = CreateSnapInfo();
            snapshotLast.AddRef();
        }
        //创建快照
        private SnapShot CreateSnapInfo()
        {
            //看最新高度的快照是否已经产生
            var snapshot = new SnapShot(this.dbPtr);
            snapshot.Init();
            return snapshot;
        }
        private SnapShot snapshotLast;

        //如果 height=0，取最新的快照
        public ISnapShot UseSnapShot()
        {
            var snap = snapshotLast;

            snap.AddRef();
            return snap;
        }
        public IWriteBatch CreateWriteBatch()
        {
            return new WriteBatch(this.dbPtr, UseSnapShot() as SnapShot);
        }
        public void WriteBatch(IWriteBatch wb)
        {
            RocksDbSharp.Native.Instance.rocksdb_write(this.dbPtr, this.defaultWriteOpPtr, (wb as WriteBatch).batchptr);

        }
    }
    public interface ISnapShot : IDisposable
    {
        byte[] GetValueData(byte[] tableid, byte[] key);
        IKeyFinder CreateKeyFinder(byte[] tableid, byte[] beginkey = null, byte[] endkey = null);
        IKeyIterator CreateKeyIterator(byte[] tableid, byte[] _beginkey = null, byte[] _endkey = null);
        byte[] GetTableInfoData(byte[] tableid);
        uint GetTableCount(byte[] tableid);
    }
    class SnapShot : ISnapShot
    {
        public SnapShot(IntPtr dbPtr)
        {
            this.dbPtr = dbPtr;
        }
        public void Init()
        {
            //this.readop = new RocksDbSharp.ReadOptions();
            this.readopHandle = RocksDbSharp.Native.Instance.rocksdb_readoptions_create();

            snapshotHandle = RocksDbSharp.Native.Instance.rocksdb_create_snapshot(this.dbPtr);
            RocksDbSharp.Native.Instance.rocksdb_readoptions_set_snapshot(readopHandle, snapshotHandle);
        }
        int refCount = 0;
        public IntPtr dbPtr;
        //public RocksDbSharp.RocksDb db;
        public IntPtr readopHandle;
        //public RocksDbSharp.ReadOptions readop;
        public IntPtr snapshotHandle = IntPtr.Zero;
        //public RocksDbSharp.Snapshot snapshot;

        public void Dispose()
        {
            lock (this)
            {
                refCount--;
                if (refCount == 0 && snapshotHandle != IntPtr.Zero)
                {
                    RocksDbSharp.Native.Instance.rocksdb_release_snapshot(this.dbPtr, snapshotHandle);
                    //snapshot.Dispose();
                    snapshotHandle = IntPtr.Zero;

                    RocksDbSharp.Native.Instance.rocksdb_readoptions_destroy(readopHandle);
                    readopHandle = IntPtr.Zero;
                }
            }
        }
        /// <summary>
        /// 对snapshot的引用计数加锁，保证处理是线程安全的
        /// </summary>
        public void AddRef()
        {
            lock (this)
            {
                refCount++;
            }
        }
        public byte[] GetValueData(byte[] tableid, byte[] key)
        {
            byte[] finialkey = LightDB.Helper.CalcKey(tableid, key);
            return RocksDbSharp.Native.Instance.rocksdb_get(this.dbPtr, this.readopHandle, finialkey);
            //(readOptions ?? DefaultReadOptions).Handle, key, keyLength, cf);

            //return this.db.Get(finialkey, null, readop);
        }
        public IKeyFinder CreateKeyFinder(byte[] tableid, byte[] beginkey = null, byte[] endkey = null)
        {
            //TableKeyFinder find = new TableKeyFinder(this, tableid, beginkey, endkey);
            //return find;
            return null;
        }
        public IKeyIterator CreateKeyIterator(byte[] tableid, byte[] _beginkey = null, byte[] _endkey = null)
        {
            //var beginkey = Helper.CalcKey(tableid, _beginkey);
            //var endkey = Helper.CalcKey(tableid, _endkey);
            //return new TableIterator(this, tableid, beginkey, endkey);
            return null;
        }
        public byte[] GetTableInfoData(byte[] tableid)
        {
            var tablekey = LightDB.Helper.CalcKey(tableid, null, LightDB.SplitWord.TableInfo);
            var data = RocksDbSharp.Native.Instance.rocksdb_get(this.dbPtr, this.readopHandle, tablekey);
            if (data == null)
                return null;
            return data;
        }
        public uint GetTableCount(byte[] tableid)
        {
            var tablekey = LightDB.Helper.CalcKey(tableid, null, LightDB.SplitWord.TableCount);
            var data = RocksDbSharp.Native.Instance.rocksdb_get(this.dbPtr, this.readopHandle, tablekey);
            return BitConverter.ToUInt32(data);
        }
    }
    public interface IKeyIterator : IEnumerator<byte[]>
    {

    }
    public interface IKeyFinder : IEnumerable<byte[]>
    {

    }

    public interface IWriteBatch
    {
        ISnapShot snapshot
        {
            get;
        }
        byte[] GetData(byte[] finalkey);
        void CreateTable(byte[] tableid, byte[] finaldata);
        void DeleteTable(byte[] tableid);
        void Put(byte[] tableid, byte[] key, byte[] finaldata);
        void Delete(byte[] tableid, byte[] key);
    }
    class WriteBatch : IWriteBatch, IDisposable
    {
        public WriteBatch(IntPtr dbptr, SnapShot snapshot)
        {
            this.dbPtr = dbptr;
            this.batchptr = RocksDbSharp.Native.Instance.rocksdb_writebatch_create();
            //this.batch = new RocksDbSharp.WriteBatch();
            this._snapshot = snapshot;
            this.cache = new Dictionary<string, byte[]>();
        }
        //RocksDbSharp.RocksDb db;
        public IntPtr dbPtr;
        public SnapShot _snapshot;
        public ISnapShot snapshot
        {
            get
            {
                return _snapshot;
            }
        }
        //public RocksDbSharp.WriteBatch batch;
        public IntPtr batchptr;
        Dictionary<string, byte[]> cache;

        public void Dispose()
        {
            if (batchptr != IntPtr.Zero)
            {
                RocksDbSharp.Native.Instance.rocksdb_writebatch_destroy(batchptr);
                batchptr = IntPtr.Zero;
                //batch.Dispose();
                //batch = null;
            }
            _snapshot.Dispose();
        }
        public byte[] GetData(byte[] finalkey)
        {
            var hexkey = LightDB.Helper.ToString_Hex(finalkey);
            if (cache.ContainsKey(hexkey))
            {
                return cache[hexkey];
            }
            else
            {
                var data = RocksDbSharp.Native.Instance.rocksdb_get(dbPtr, _snapshot.readopHandle, finalkey);
                if (data == null || data.Length == 0)
                    return null;
                //db.Get(finalkey, null, snapshot.readop);
                cache[hexkey] = data;
                return data;
            }
        }
        private void PutDataFinal(byte[] finalkey, byte[] value)
        {
            var hexkey = LightDB.Helper.ToString_Hex(finalkey);
            cache[hexkey] = value;
            RocksDbSharp.Native.Instance.rocksdb_writebatch_put(batchptr, finalkey, (ulong)finalkey.Length, value, (ulong)value.Length);
            //batch.Put(finalkey, value);
        }
        private void DeleteFinal(byte[] finalkey)
        {
            var hexkey = LightDB.Helper.ToString_Hex(finalkey);
            cache.Remove(hexkey);
            RocksDbSharp.Native.Instance.rocksdb_writebatch_delete(batchptr, finalkey, (ulong)finalkey.Length);
            //batch.Delete(finalkey);
        }
        public void CreateTable(byte[] tableid, byte[] tableinfo)
        {
            var finalkey = LightDB.Helper.CalcKey(tableid, null, LightDB.SplitWord.TableInfo);
            var countkey = LightDB.Helper.CalcKey(tableid, null, LightDB.SplitWord.TableCount);
            var data = GetData(finalkey);
            if (data != null && data.Length != 0)
            {
                throw new Exception("alread have that.");
            }
            PutDataFinal(finalkey, tableinfo);

            var byteCount = GetData(countkey);
            if (byteCount == null)
            {
                byteCount = BitConverter.GetBytes((UInt32)0);
            }
            PutDataFinal(countkey, byteCount);
        }

        public void DeleteTable(byte[] tableid)
        {
            var finalkey = LightDB.Helper.CalcKey(tableid, null, LightDB.SplitWord.TableInfo);
            //var countkey = Helper.CalcKey(tableid, null, SplitWord.TableCount);
            var vdata = GetData(finalkey);
            if (vdata != null && vdata.Length != 0)
            {
                DeleteFinal(finalkey);
            }
        }
        public void Put(byte[] tableid, byte[] key, byte[] finaldata)
        {
            var finalkey = LightDB.Helper.CalcKey(tableid, key);
            var countkey = LightDB.Helper.CalcKey(tableid, null, LightDB.SplitWord.TableCount);



            var countdata = GetData(countkey);
            UInt32 count = 0;
            if (countdata != null)
            {
                count = BitConverter.ToUInt32(countdata);
            }
            var vdata = GetData(finalkey);
            if (vdata == null || vdata.Length == 0)
            {
                count++;
            }
            else
            {
                if (LightDB.Helper.BytesEquals(vdata, finaldata) == false)
                    count++;
            }
            PutDataFinal(finalkey, finaldata);

            var countvalue = BitConverter.GetBytes(count);
            PutDataFinal(countkey, countvalue);
        }

        public void Delete(byte[] tableid, byte[] key)
        {
            var finalkey = LightDB.Helper.CalcKey(tableid, key);

            var countkey = LightDB.Helper.CalcKey(tableid, null, LightDB.SplitWord.TableCount);
            var countdata = GetData(countkey);
            UInt32 count = 0;
            if (countdata != null)
            {
                count = BitConverter.ToUInt32(countdata);
            }

            var vdata = GetData(finalkey);
            if (vdata != null && vdata.Length != 0)
            {
                DeleteFinal(finalkey);
                count--;
                var countvalue = BitConverter.GetBytes(count);
                PutDataFinal(countkey, countvalue);

            }
        }
    }
}