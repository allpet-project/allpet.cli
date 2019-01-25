using System;
using System.Collections.Generic;
using System.Text;

namespace SimplDb.Protocol.Sdk
{
    class Converter
    {
        public static Action<object> Convert<T>(Action<T> myActionT)
        {
            if (myActionT == null) return null;
            return new Action<object>(o => myActionT((T)o));
        }

        public static dynamic ChangeTo(dynamic source, Type dest)
        {
            return System.Convert.ChangeType(source, dest);
        }
    }
}
