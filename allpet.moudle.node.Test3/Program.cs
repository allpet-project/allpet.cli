﻿using AllPet.Common;
using Newtonsoft.Json.Linq;
using System;
using AllPet.Pipeline.MsgPack;
namespace allpet.moudle.node.Test3
{
    class Program
    {
        public static AllPet.Common.ILogger logger;
        public static Config config;

        static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("CMD(1=测试：发现新加入的节点并连接。)>");
                var cmd = Console.ReadLine();
                switch (cmd)
                {
                    case "1":
                        test1.run();
                        break;
                }
            }
        }
            
            
    }
}
