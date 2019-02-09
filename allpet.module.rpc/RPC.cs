using AllPet.Common;
using AllPet.Pipeline;
using AllPet.Pipeline.MsgPack;
using MsgPack;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace AllPet.Module
{
    class Config_Module
    {
        public System.Net.IPEndPoint HttpListenEndPoint;
        public System.Net.IPEndPoint HttpsListenEndPoint;
        public string HttpsPFXFilePath;
        public string HttpsPFXFilePassword;
        public Config_Module(Newtonsoft.Json.Linq.JObject json)
        {
            HttpListenEndPoint = json["HttpListenEndPoint"].AsIPEndPoint();

            if (json.ContainsKey("HttpsListenEndPoint"))
                HttpsListenEndPoint = json["HttpsListenEndPoint"].AsIPEndPoint();
            else
                HttpsListenEndPoint = new System.Net.IPEndPoint(System.Net.IPAddress.Any, 0);

            if (json.ContainsKey("HttpsPFXFilePath"))
                HttpsPFXFilePath = json["HttpsPFXFilePath"].AsString();
            else
                HttpsPFXFilePath = "";

            if (json.ContainsKey("HttpsPFXFilePassword"))
                HttpsPFXFilePassword = json["HttpsPFXFilePassword"].AsString();
            else
                HttpsPFXFilePassword = "";
        }
    }
    public class Module_RPC : Module_MsgPack
    {
        AllPet.Common.ILogger logger;
        Config_Module config;
        http.server.httpserver server;
        const UInt16 CMDID_RPC = 0x0300;
        System.Collections.Concurrent.ConcurrentDictionary<int, MessagePackObject?> recvRPC;
        int RPCID;
        public Module_RPC(AllPet.Common.ILogger logger, Newtonsoft.Json.Linq.JObject configJson) : base(true)
        {
            this.logger = logger;
            this.config = new Config_Module(configJson);
            this.server = new http.server.httpserver();

            this.recvRPC = new System.Collections.Concurrent.ConcurrentDictionary<int, MessagePackObject?>();
            this.RPCID = 0;
        }

        public override void OnStart()
        {
            this.server.SetJsonRPCFail("/", ActionRPCFail);
            this.server.AddJsonRPC("/", "help", ActionRPC_Help);
            this.server.AddJsonRPC("/", "listpeer", ActionRPC_ListPeer);
            this.server.Start(this.config.HttpListenEndPoint.Port, this.config.HttpsListenEndPoint.Port, this.config.HttpsPFXFilePath, this.config.HttpsPFXFilePassword);
            logger.Warn("RPC start at port=" + this.config.HttpListenEndPoint.Port);
        }
        int GetFreeID()
        {
            return RPCID++;
        }
        async Task<JObject> ActionRPC_Help(JObject request)
        {
            JObject jojb = new JObject();
            jojb["message"] = "hahaha";
            return jojb;
        }
        async Task<JObject> ActionRPC_ListPeer(JObject request)
        {
            //这里呈现RPC接口的第一种模式，立即返回型，自己等待
            var node = this.GetPipeline("this/node");
            MessagePackObjectDictionary dict = new MessagePackObjectDictionary();
            dict["cmd"] = CMDID_RPC;
            dict["method"] = "listpeer";
            var _id = GetFreeID();
            dict["id"] = _id;
            node.Tell(new MessagePackObject(dict));
            //等待死循环,限制等待一秒
            DateTime time = DateTime.Now;
            while ((DateTime.Now - time).TotalSeconds < 1.0f)
            {
                if (this.recvRPC.TryRemove(_id, out MessagePackObject? got))
                {
                    var strresult = got.Value.AsDictionary()["result"].ToString();
                    JObject jobj = new JObject();
                    jobj["peers"] = JArray.Parse(strresult);
                    return jobj;
                }
                await System.Threading.Tasks.Task.Delay(1);
            }
            return null;
        }
        async Task<http.server.JSONRPCController.ErrorObject> ActionRPCFail(JObject request, string errorMessage)
        {
            var error = new http.server.JSONRPCController.ErrorObject();
            error.message = errorMessage;
            error.data = request;
            error.code = -100;
            return error;

        }
        public override void OnTell(IModulePipeline from, MessagePackObject? obj)
        {
            var dict = obj.Value.AsDictionary();
            var cmd = dict["cmd"].AsUInt16();
            if (cmd == CMDID_RPC)
            {
                var id = dict["id"].AsInt32();
                this.recvRPC[id] = obj;
            }
        }
    }
}
