using System;
using System.IO;

namespace NetAPI
{

    public class PrivateNetConfig
    {
        public string mongodbConnStr = string.Empty;
        public string mongodbDatabase = string.Empty;
        public string NeoCliJsonRPCUrl = string.Empty;
    }
    class Config
    {
        public static string mongodbConnStr = string.Empty;
        public static string mongodbDatabase = string.Empty;
        public static string NeoCliJsonRPCUrl = string.Empty;
        public static int sleepTime = 0;
        public static bool beUtxoSleep = false;
        public static bool startMonitorFlag = true;

        public static PrivateNetConfig privateNet = new PrivateNetConfig();

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
            startMonitorFlag = config.AsDict().GetDictItem("startMonitorFlag").AsBool();

            sleepTime = config.AsDict().GetDictItem("sleepTime").AsInt();

            MyJson.JsonNode_Object privateNetInfo = config["privateChain"].AsDict();
            privateNet.mongodbConnStr = privateNetInfo["mongodbConnStr"].AsString();
            privateNet.mongodbDatabase = privateNetInfo["mongodbDatabase"].AsString();
            privateNet.NeoCliJsonRPCUrl = privateNetInfo["NeoCliJsonRPCUrl"].AsString();
        }

    }
}
