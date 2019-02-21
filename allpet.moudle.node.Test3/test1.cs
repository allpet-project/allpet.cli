using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AllPet.Common;
using Newtonsoft.Json.Linq;
using AllPet.Pipeline.MsgPack;
using AllPet.Pipeline;


namespace allpet.moudle.node.Test3
{
    /// <summary>
    /// 发现新节点并连接
    /// </summary>
    class test1
    {
       public static void run()
        {
            Console.WriteLine("CMD(1=启动一堆节点 2=启动测试节点)>");
            var cmd = Console.ReadLine();
            switch (cmd)
            {
                case "1":
                    runBaseNodes();
                    break;
                case "2":
                    runTestNode();
                    break;
            }
            //var timer = new System.Timers.Timer();
            //timer.Elapsed += (object source, System.Timers.ElapsedEventArgs e) =>
            //{
            //    Console.ForegroundColor = ConsoleColor.Red;
            //    var pipeline = node.sys.GetPipeline(null, "this/node");
                
            //    Console.WriteLine("list nodes:"+node.sys.GetPipeline());
            //};
            //timer.Interval = 1000;
            //timer.AutoReset = true;
            //timer.Enabled = true;

        }
        static string initpeer = "127.0.0.1:2081";//2081


        static void runBaseNodes()
        {

            //new Node(null, null, "0.0.0.0:1890");
            new Node("0.0.0.0:5880", initpeer, "0.0.0.0:5880");
            new Node("0.0.0.0:5881", initpeer, "0.0.0.0:5881");

            new Node("0.0.0.0:6880", "127.0.0.1:5880", "0.0.0.0:6880");

        }

        static void runTestNode()
        {
            var node = new Node("0.0.0.0:8883", initpeer);

            var pipeline = node.sys.GetPipeline(null, "this/node");
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
            node.sys.Dispose();

        }

    }
}
