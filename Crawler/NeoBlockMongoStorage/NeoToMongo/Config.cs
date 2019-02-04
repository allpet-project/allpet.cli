using System;
using System.Collections.Generic;
using System.IO;

namespace NeoToMongo
{
    class Config
    {
        public static string mongodbConnStr = string.Empty;
        public static string mongodbDatabase = string.Empty;
        public static string NeoCliJsonRPCUrl = string.Empty;
        public static int startBlockHeight = 0;

        public static bool enableAssetFilter = false;
        public static List<string> asseFiltertList = new List<string>();

        //public static int sleepTime = 0;
        //public static bool beUtxoSleep = false;
        /// <summary>
        /// 加载配置
        /// </summary>
        public static void loadFromPath(string path)
        {
            string result = File.ReadAllText(path);
            MyJson.JsonNode_Object config = MyJson.Parse(result) as MyJson.JsonNode_Object;
            mongodbConnStr = config.AsDict().GetDictItem("mongodbConnStr").AsString();
            mongodbDatabase = config.AsDict().GetDictItem("mongodbDatabase").AsString();
            NeoCliJsonRPCUrl = config.AsDict().GetDictItem("NeoCliJsonRPCUrl").AsString();
            startBlockHeight = config.AsDict().GetDictItem("startBlockHeight").AsInt();
            enableAssetFilter = config.AsDict().GetDictItem("enableAssetFilter").AsBool();
            var assetlist=config.AsDict().GetDictItem("asseFiltertList").AsDict();
            MyJson.JsonNode_Array assetArr = assetlist["asset"].AsList();
            MyJson.JsonNode_Array nep5Arr = assetlist["nep5"].AsList();

            foreach(MyJson.JsonNode_ValueString item in assetArr)
            {
                asseFiltertList.Add(item.AsString());
            }
            foreach (MyJson.JsonNode_ValueString item in nep5Arr)
            {
                asseFiltertList.Add(item.AsString());
            }
            //sleepTime = config.AsDict().GetDictItem("sleepTime").AsInt();
        }

    }
}
