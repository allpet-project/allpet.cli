using AllPet.nodecli.httpinterface;
using System;
using System.Collections.Generic;

namespace AllPet.nodecli
{
    class Program
    {
        static void Main(string[] args)
        {
            var system = AllPet.Pipeline.PipelineSystem.CreatePipelineSystemV1();
            system.RegistModule("cli", new Module_Cli());
            system.RegistModule("node", AllPet.node.Node.CreateNode());
            system.OpenNetwork(new AllPet.peer.tcp.PeerOption()
            {

            });
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
