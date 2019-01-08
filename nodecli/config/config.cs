using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace allpet.nodecli
{
    class Config
    {
        class ChainInfo
        {
            public string MagicStr;
            public string[] InitOwner;
            public byte[] ToInitScript()
            {
                return null;
            }
        }
        ChainInfo chainInfo = new ChainInfo();

        static string[] GetStringArrayFromJson(JArray jarray)
        {
            var strs = new string[jarray.Count];
            for (var i = 0; i < jarray.Count; i++)
            {
                strs[i] = (string)jarray[i];
            }
            return strs;
        }
        private Config()
        {

        }
        public static Config Parse(string txt)
        {
            var jobj = JObject.Parse(txt);
            Config c = new Config();
            c.chainInfo.MagicStr = (string)jobj["ChainInfo"]["MagicStr"];
            c.chainInfo.InitOwner = GetStringArrayFromJson((JArray)jobj["ChainInfo"]["InitOwner"]);
            return c;
        }
    }
}
