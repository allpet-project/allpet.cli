using AllPet.Pipeline;
using Microsoft.Extensions.Configuration;
using SimpleDb.Server.Actor;
using System;
using System.IO;

namespace SimpleDb.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("SimpleDb.Server Start.....");
            //var system = AllPet.Pipeline.PipelineSystem.CreatePipelineSystemV1();
            //system.RegistModule("mainloop", new Module_Loop());
            //system.Start();
            //var pipe = system.GetPipeline(null, "this/mainloop");
            //while (pipe.IsVaild)
            //{
            //    System.Threading.Thread.Sleep(100);
            //}

            var serverSys = AllPet.Pipeline.PipelineSystem.CreatePipelineSystemV1();
            serverSys.OpenNetwork(new AllPet.peer.tcp.PeerOption());
            serverSys.OpenListen(new System.Net.IPEndPoint(System.Net.IPAddress.Any, 8888));
            serverSys.RegistModule("simpledb", new SimpleDbModule());
            serverSys.Start();

            

            Console.ReadLine();
        }
    }
    class Module_Loop : Module
    {
        public override void Dispose()
        {
            //如果要重写dispose，必须执行base.Dispose
            base.Dispose();
        }
        public override void OnStart()
        {
            //不要堵死OnStart函數
            System.Threading.ThreadPool.QueueUserWorkItem((s) =>
            {
                MainLoop();
            });
        }
        public override void OnTell(IModulePipeline from, byte[] data)
        {

        }

        async void MainLoop()
        {
            var serverSys = AllPet.Pipeline.PipelineSystem.CreatePipelineSystemV1();
            serverSys.OpenNetwork(new AllPet.peer.tcp.PeerOption());
            serverSys.OpenListen(new System.Net.IPEndPoint(System.Net.IPAddress.Any, 8888));            
            serverSys.RegistModule("simpledb", new SimpleDbModule());
            serverSys.Start();

            while (true)
            {
                Console.Write(">");
                var line = Console.ReadLine();
                
                if (line == "exit")
                {
                    this.Dispose();//這將會導致這個模塊關閉
                    break;
                }
            }
        }
    }
}
