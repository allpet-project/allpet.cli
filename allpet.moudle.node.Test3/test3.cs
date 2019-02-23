using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using AllPet.Pipeline.MsgPack;

namespace allpet.moudle.node.Test3
{
    class test3
    {
        public static void run()
        {
            Console.WriteLine("CMD(1=启动观测节点 2=启动共识节点/其他节点) >");
            var cmd = Console.ReadLine();
            switch (cmd)
            {
                case "1":
                    runobserveNodes();
                    break;
                case "2":
                    runTestNodes();
                    break;

            }
            while(true)
            {

            }
        }

        static void runobserveNodes()
        {
            var linkto="127.0.0.1:1892";
            var node= new Node(null, linkto, null,true);//观察节点
            node.actor.beObserver = true;

            var pipeline = node.sys.GetPipeline(null, "this/node");
            while (pipeline.IsVaild)
            {
                var line = Console.ReadLine();
                if (string.IsNullOrEmpty(line) == false)
                {
                    if (line == "exit")
                    {
                        //node.actor.Dispose();
                        node.sys.Dispose();

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
        }


        static void runTestNodes()
        {
            new Node("127.0.0.1:1890", null, "0.0.0.0:1890",false,"proveconfig.json");//共识节点

            new Node("127.0.0.1:1891", "127.0.0.1:1890", "0.0.0.0:1891", false);
            new Node("127.0.0.1:1892", "127.0.0.1:1891", "0.0.0.0:1892", false);
            new Node("127.0.0.1:1893", "127.0.0.1:1892", "0.0.0.0:1893", false);
            new Node("127.0.0.1:1894", "127.0.0.1:1893", "0.0.0.0:1894", false);
            new Node("127.0.0.1:1895", "127.0.0.1:1894", "0.0.0.0:1895", false);
            new Node("127.0.0.1:1896", "127.0.0.1:1895", "0.0.0.0:1896", false);
            new Node("127.0.0.1:1897", "127.0.0.1:1896", "0.0.0.0:1897", false);
            new Node("127.0.0.1:1898", "127.0.0.1:1897", "0.0.0.0:1898", false);
            new Node("127.0.0.1:1899", "127.0.0.1:1898", "0.0.0.0:1899", false);


            //new Node(null, null, "0.0.0.0:2890", false, "proveconfig.json");//共识节点

            //new Node("127.0.0.1:2891", "127.0.0.1:2890", "0.0.0.0:2891", false);
            //new Node("127.0.0.1:2892", "127.0.0.1:2891", "0.0.0.0:2892", false);
            //new Node("127.0.0.1:2893", "127.0.0.1:2892", "0.0.0.0:2893", false);
            //new Node("127.0.0.1:2894", "127.0.0.1:2893", "0.0.0.0:2894", false);
        }


        static void requestPlevels()
        {

        }
    }
}
