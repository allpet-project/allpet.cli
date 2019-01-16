using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace allpet.nodecli
{
    public class Config
    {
        public string dbPath;
        public node.Config_ChainInit chainInfo;

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
