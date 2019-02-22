using AllPet.Common;
using Newtonsoft.Json.Linq;
using System;
using AllPet.Pipeline.MsgPack;

namespace allpet.module.node_rpc.test
{
    class Program
    {
        public static AllPet.Common.ILogger logger;
        public static Config config;

        static void Main(string[] args)
        {
            logger = new AllPet.Common.Logger();
            logger.Warn("Allpet.Node.RPC v0.001");

            var config = new Config(logger);

            //init current path.
            //把当前目录搞对，怎么启动都能找到dll了
            var lastpath = System.IO.Path.GetDirectoryName(typeof(Program).Assembly.Location); ;
            Console.WriteLine("exepath=" + lastpath);
            Environment.CurrentDirectory = lastpath;



            var system = AllPet.Pipeline.PipelineSystem.CreatePipelineSystemV1(logger);
            
            var config_node = config.GetJson("config.json", ".ModulesConfig.Node") as JObject;
            var config_rpc = config.GetJson("config.json", ".ModulesConfig.RPC") as JObject;
            if (config_node.ContainsKey("Key_Nep2") && config_node.ContainsKey("Key_Password") == false)
            {
                Console.Write("input Key for Nep2>");
                var pass = Console.ReadLine();
                config_node["Key_Password"] = pass;
            }

            if (Config.IsOpen(config_node))
            {
                system.RegistModule("node", new AllPet.Module.Module_Node(logger, config_node));
            }
            if (Config.IsOpen(config_rpc))
            {
                system.RegistModule("rpc", new AllPet.Module.Module_RPC(logger, config_rpc));
            }
            system.OpenNetwork(new AllPet.peer.tcp.PeerOption()
            {

            });

            var endpoint = config.GetIPEndPoint("config.json", ".ListenEndPoint");
            if (endpoint != null && endpoint.Port != 0)
            {
                try
                {
                    system.OpenListen(endpoint);
                }
                catch (Exception err)
                {
                    logger.Error("open listen err:" + err);
                }
            }
            //是不是开listen 这个事情可以留给Module
            system.Start();

            //等待cli结束才退出
            var pipeline = system.GetPipeline(null, "this/node");
            while (pipeline.IsVaild)
            {
                var line = Console.ReadLine();
                if (string.IsNullOrEmpty(line) == false)
                {
                    if (line == "exit")
                    {
                        break;
                    }
                    var cmds = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    var dict = new MsgPack.MessagePackObjectDictionary();
                    dict["cmd"] = (UInt16)AllPet.Module.CmdList.Local_Cmd;
                    var list = new MsgPack.MessagePackObject[cmds.Length];
                    for (var i = 0; i < cmds.Length; i++)
                    {
                        list[i] = cmds[i];
                    }
                    dict["params"] = list;
                    pipeline.Tell(new MsgPack.MessagePackObject(dict));
                }
            }
            system.Dispose();

        }


    }
}
