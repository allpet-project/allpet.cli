using AllPet.Common;
using Newtonsoft.Json.Linq;
using System;
using AllPet.Pipeline.MsgPack;
using AllPet.Pipeline;
using AllPet.Module;

namespace allpet.moudle.node.Test3
{
   public class Node
    {

        public AllPet.Common.ILogger logger;
        public Config config;

        public ISystem sys;
        public Module_Node actor;
        public Node(string EndPoint = null,string initpeer=null,string ListenEndPoint=null,bool beEnableQueryPeers = true,string jsonFile= "config.json")
        {
            logger = new AllPet.Common.Logger();
            logger.Warn("Allpet.Node v0.001 Peer 01");

            var config = new Config(logger);

            //init current path.
            //把当前目录搞对，怎么启动都能找到dll了
            var lastpath = System.IO.Path.GetDirectoryName(typeof(Program).Assembly.Location); ;
            Console.WriteLine("exepath=" + lastpath);
            Environment.CurrentDirectory = lastpath;



            var system = AllPet.Pipeline.PipelineSystem.CreatePipelineSystemV1(new AllPet.Common.Logger());


            var config_node = config.GetJson(jsonFile, ".ModulesConfig.Node") as JObject;

            //-----------------配置 publicendpoint /initpeer
            if(EndPoint != null)
            {
                config_node["PublicEndPoint"] = EndPoint;
            }
            if(initpeer!=null)
            {
                config_node["InitPeer"] =new JArray(initpeer);
            }


            if (Config.IsOpen(config_node))
            {
                this.actor = new AllPet.Module.Module_Node(logger, config_node);
                system.RegistModule("node", this.actor);
                this.actor.beEnableQueryPeers = beEnableQueryPeers;
            }
            else
            {
                logger.Error("cant find config for node");
                return;
            }


            system.OpenNetwork(new AllPet.peer.tcp.PeerOption() { });
            var endpoint = config.GetIPEndPoint(jsonFile, ".ListenEndPoint");
            if(ListenEndPoint!=null)
            {
                endpoint= ListenEndPoint.AsIPEndPoint();
            }

            if (endpoint != null && endpoint.Port != 0)
            {
                try
                {
                    system.OpenListen(endpoint);
                }
                catch (Exception err)
                {
                    logger.Error("listen error:" + err.ToString());
                }
            }

            system.Start();
            //等待cli结束才退出
            var pipeline = system.GetPipeline(null, "this/node");
            //while (pipeline.IsVaild)
            //{
            //    var line = Console.ReadLine();
            //    if (string.IsNullOrEmpty(line) == false)
            //    {
            //        if (line == "exit")
            //        {
            //            break;
            //        }
            //        var cmds = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            //        var dict = new MsgPack.MessagePackObjectDictionary();
            //        dict["cmd"] = (UInt16)AllPet.Module.CmdList.Local_Cmd;
            //        var list = new MsgPack.MessagePackObject[cmds.Length];
            //        for (var i = 0; i < cmds.Length; i++)
            //        {
            //            list[i] = cmds[i];
            //        }
            //        dict["params"] = list;
            //        pipeline.Tell(new MsgPack.MessagePackObject(dict));
            //    }
            //}
            //system.Dispose();

            this.sys = system;
        }
    }
}
