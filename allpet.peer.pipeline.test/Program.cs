using System;
using System.Threading.Tasks;
using allpet.peer.pipeline.test.test;
using AllPet.Pipeline;

namespace AllPet.Pipeline.test
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("pipeline test.");
            var system = AllPet.Pipeline.PipelineSystem.CreatePipelineSystemV1();
            system.RegistModule("mainloop", new Module_Loop());
            system.Start();
            var pipe = system.GetPipeline(null, "this/mainloop");
            while (pipe.IsVaild)
            {
                System.Threading.Thread.Sleep(100);
            }
        }
    }
    class Module_Loop : AllPet.Pipeline.Module
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
                TestLoop();
            });
        }
        public override void OnTell(IModulePipeline from, byte[] data)
        {
        }

        async void TestLoop()
        {
            while (true)
            {
                Console.Write(">");
                var line = Console.ReadLine();
                if (line == "1")
                {
                    await test1_local.Test();//這個測試創建兩個本地actor，并讓他們通訊
                }
                if (line == "2")
                {
                    await test2_remote.Test();
                }
                if (line == "3")
                {
                    await test3_perform.Test();
                }
                if (line == "exit")
                {
                    this.Dispose();//這將會導致這個模塊關閉
                    break;
                }
            }
        }


    }

}

