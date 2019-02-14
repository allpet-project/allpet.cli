using AllPet.Common;
using Newtonsoft.Json.Linq;
using System;
using AllPet.Pipeline.MsgPack;

namespace allpet.module.node.test
{
    class Program
    {
        public static AllPet.Common.ILogger logger;
        public static Config config;

        static void Main(string[] args)
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


            var config_node = config.GetJson("config.json", ".ModulesConfig.Node") as JObject;
            if (Config.IsOpen(config_node))
            {
                system.RegistModule("node", new AllPet.Module.Module_Node(logger, config_node));
            }
            else
            {
                logger.Error("cant find config for node");
                return;
            }


            system.OpenNetwork(new AllPet.peer.tcp.PeerOption() { });
            var endpoint = config.GetIPEndPoint("config.json", ".ListenEndPoint");
            if (endpoint != null && endpoint.Port != 0)
            {
                try
                {
                    system.OpenListen(endpoint);
                }
                catch(Exception err)
                {
                    logger.Error("listen error:" + err.ToString());
                }
            }

            system.Start();

            //等待cli结束才退出
            var pipeline = system.GetPipeline(null, "this/node");
            while (pipeline.IsVaild)
            {
                var line = Console.ReadLine();
                if (string.IsNullOrEmpty(line)==false)
                {
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
