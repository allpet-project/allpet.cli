using AllPet.Common;
using AllPet.nodecli.httpinterface;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace AllPet.nodecli
{
    class Program
    {
        public static AllPet.Common.ILogger logger;
        public static Config config;

        static void Main(string[] args)
        {
            logger = new AllPet.Common.Logger();
            logger.Warn("Allpet.Node v0.001");

            var config = new Config(logger);

            //init current path.
            //把当前目录搞对，怎么启动都能找到dll了
            var lastpath = System.IO.Path.GetDirectoryName(typeof(Program).Assembly.Location); ;
            Console.WriteLine("exepath=" + lastpath);
            Environment.CurrentDirectory = lastpath;



            var system = AllPet.Pipeline.PipelineSystem.CreatePipelineSystemV1(logger);

            var config_cli = config.GetJson("config.json", ".ModulesConfig.Cli") as JObject;
            var config_node = config.GetJson("config.json", ".ModulesConfig.Node") as JObject;
            var config_rpc = config.GetJson("config.json", ".ModulesConfig.RPC") as JObject;
            if (config_node.ContainsKey("Key_Nep2") && config_node.ContainsKey("Key_Password")==false)
            {
                Console.Write("input Key for Nep2>");
                var pass = Console.ReadLine();
                config_node["Key_Password"] = pass;
            }
            if (Config.IsOpen(config_cli))
            {
                system.RegistModule("cli", new Module_Cli(logger, config_cli));
            }

            if (Config.IsOpen(config_node))
            {
                system.RegistModule("node", new AllPet.Module.Module_Node(logger, config_node));
            }
            if(Config.IsOpen(config_rpc))
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
            var pipeline = system.GetPipeline(null, "this/cli");
            while (pipeline.IsVaild)
            {
                System.Threading.Thread.Sleep(100);
            }

        }


    }
}
