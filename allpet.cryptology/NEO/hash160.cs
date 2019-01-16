using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Allpet
{
    public class Hash160 : IComparable<Hash160>
    {
        public Hash160(byte[] data)
        {
            if (data.Length != 20)
                throw new Exception("error length.");
            this.data = data;

        }
        public Hash160(string hexstr)
        {
            var bts = Helper.HexString2Bytes(hexstr);
            if (bts.Length != 20)
                throw new Exception("error length.");
            this.data = bts.Reverse().ToArray();
        }
        public override string ToString()
        {
            return "0x" + Helper.Bytes2HexString(this.data.Reverse().ToArray());
        }
        public byte[] data;

        public unsafe int CompareTo(Hash160 other)
        {
            fixed (byte* px = data, py = other.data)
            {
                uint* lpx = (uint*)px;
                uint* lpy = (uint*)py;
                for (int i = 5; i >= 0; i--)
                {
                    if (lpx[i] > lpy[i])
                        return 1;
                    if (lpx[i] < lpy[i])
                        return -1;
                }
            }
            return 0;
        }
        public unsafe override bool Equals(object obj)
        {
            var other = obj as Hash160;
            fixed (byte* px = data, py = other.data)
            {
                uint* lpx = (uint*)px;
                uint* lpy = (uint*)py;
                for (int i = 5; i >= 0; i--)
                {
                    if (lpx[i] != lpy[i])
                        return false;
                }
            }
            return true;
        }
        public override int GetHashCode()
        {
            return new System.Numerics.BigInteger(data).GetHashCode();
        }
        public static implicit operator byte[] (Hash160 value)
        {
            return value.data;
        }
        public static implicit operator Hash160(byte[] value)
        {
            return new Hash160(value);
        }
    }
}
