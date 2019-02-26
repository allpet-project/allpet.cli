using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using AllPet.Module;
using AllPet.Pipeline.MsgPack;
using MsgPack;

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
        }

        static void runobserveNodes()
        {
            var linkto="127.0.0.1:1892";
            var node= new Node(null, linkto, null,true);//观察节点
            node.actor.beObserver = true;

            var pipeline = node.sys.GetPipeline(null, "this/node");
            while (pipeline.IsVaild)
            {
                Console.WriteLine("localCmd a1=a系统的plevel(5)联向b系统的plevel（6）  a2=>:a系统的plevel(5)联向b系统的plevel（5） a3=a系统的plevel(5)联向b系统的plevel（2）");
                Console.WriteLine("localCmd b1=断开a系统的plevel(5)到b系统的plevel(6)连接  b2=>:断开a系统的plevel(5)到b系统的plevel(5)连接 b3=断开a系统的plevel(5)联向b系统的plevel(2)连接");

                var line = Console.ReadLine();
                if (string.IsNullOrEmpty(line) == false)
                {
                    if (line == "exit")
                    {
                        //node.actor.Dispose();
                        node.sys.Dispose();

                        break;
                    }
                    if(line=="a1"||line=="a2"||line=="a3"||line=="b1"||line=="b2"||line=="b3")
                    {
                        switch(line)
                        {
                            case "a1":
                                {
                                    var msg = node.actor.makeCmd_SendMsg("127.0.0.1:1895", node.actor.makeCmd_ConnectTo("127.0.0.1:2896"));
                                    var localcmd = node.actor.makeCmd_FakeRemote(msg);
                                    pipeline.Tell(localcmd);
                                }
                                break;
                            case "a2":
                                {
                                    var msg = node.actor.makeCmd_SendMsg("127.0.0.1:1895", node.actor.makeCmd_ConnectTo("127.0.0.1:2895"));
                                    var localcmd = node.actor.makeCmd_FakeRemote(msg);
                                    pipeline.Tell(localcmd);
                                }
                                break;
                            case "a3":
                                {
                                    var msg = node.actor.makeCmd_SendMsg("127.0.0.1:1895", node.actor.makeCmd_ConnectTo("127.0.0.1:2892"));
                                    var localcmd = node.actor.makeCmd_FakeRemote(msg);
                                    pipeline.Tell(localcmd);
                                }
                                break;
                            case "b1":
                                {
                                    var msg = node.actor.makeCmd_SendMsg("127.0.0.1:1895", node.actor.makeCmd_DisconnectTo("127.0.0.1:2896"));
                                    var localcmd = node.actor.makeCmd_FakeRemote(msg);
                                    pipeline.Tell(localcmd);
                                }
                                break;
                            case "b2":
                                {
                                    var msg = node.actor.makeCmd_SendMsg("127.0.0.1:1895", node.actor.makeCmd_DisconnectTo("127.0.0.1:2895"));
                                    var localcmd = node.actor.makeCmd_FakeRemote(msg);
                                    pipeline.Tell(localcmd);
                                }
                                break;
                            case "b3":
                                {
                                    var msg = node.actor.makeCmd_SendMsg("127.0.0.1:1895", node.actor.makeCmd_DisconnectTo("127.0.0.1:2892"));
                                    var localcmd = node.actor.makeCmd_FakeRemote(msg);
                                    pipeline.Tell(localcmd);
                                }
                                break;
                        }

                    }else
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


            new Node("127.0.0.1:2890", null, "0.0.0.0:2890", false, "proveconfig.json");//共识节点

            new Node("127.0.0.1:2891", "127.0.0.1:2890", "0.0.0.0:2891", false);
            new Node("127.0.0.1:2892", "127.0.0.1:2891", "0.0.0.0:2892", false);
            new Node("127.0.0.1:2893", "127.0.0.1:2892", "0.0.0.0:2893", false);
            new Node("127.0.0.1:2894", "127.0.0.1:2893", "0.0.0.0:2894", false);
            new Node("127.0.0.1:2895", "127.0.0.1:2894", "0.0.0.0:2895", false);
            new Node("127.0.0.1:2896", "127.0.0.1:2895", "0.0.0.0:2896", false);
            new Node("127.0.0.1:2897", "127.0.0.1:2896", "0.0.0.0:2897", false);
            new Node("127.0.0.1:2898", "127.0.0.1:2897", "0.0.0.0:2898", false);
            new Node("127.0.0.1:2899", "127.0.0.1:2898", "0.0.0.0:2899", false);
        }
    }
}
