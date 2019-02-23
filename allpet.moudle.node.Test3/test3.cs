using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace allpet.moudle.node.Test3
{
    class test3
    {
        public static void run()
        {
            Console.WriteLine("CMD(1=启动共识节点 2=启动观测节点/)>");
            var cmd = Console.ReadLine();
            switch (cmd)
            {
                case "1":
                    runBaseNodes();
                    break;
                case "2":
                    new Node(null, "127.0.0.1:1890", null);//观察节点
                    //runTestNodes();
                    break;

            }
        }

        static void runBaseNodes()
        {
            new Node(null, null, "0.0.0.0:1890","proveconfig.json");//共识节点
        }



        static void runTestNodes()
        {
            new Node(null, "127.0.0.1:1890", "0.0.0.0:1891");
        }
    }
}
