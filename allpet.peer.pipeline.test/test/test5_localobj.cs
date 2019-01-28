using AllPet.Pipeline;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace allpet.peer.pipeline.test.test
{
    class test5_local
    {
        class Send1
        {
            public Send1(string info)
            {
                this.str1 = info;
            }
            public string str1;
        }
        class Send2
        {
            public int int1;
            public int int2;
        }


        public static async Task Test()
        {
            var system = AllPet.Pipeline.PipelineSystem.CreatePipelineSystemV1();
            system.RegistModule("hello", new Hello());//actor习惯，连注册这个活都丢线程池，我这里简化一些
            system.RegistModule("hello2", new Hello2());//actor习惯，连注册这个活都丢线程池，我这里简化一些
            system.Start();
            var actor = system.GetPipeline(null, "this/hello");
            {
                actor.TellLocalObj(new Send1("hello"));
            }
            while (true)
            {
                Console.Write("5.localobj>");
                var line = Console.ReadLine();
                if (line == "exit")
                {
                    //不能这样粗暴关闭的，关闭应该由actor内部发起
                    system.Dispose();
                    break;
                }
                actor.TellLocalObj(new Send1(line));

            }
        }
        class Hello : Module
        {
            /// <summary>
            ///  base(false)表示這個模塊是單綫程投遞的，ontell 保證在 同一個綫程裏面
            ///  base(true)或者沒有，則該模塊的OnTell為多綫程投遞，須自行處理綫程問題
            /// </summary>
            public Hello() : base(false)//這個false 表示這個模塊是單綫程投遞的，ontell 保證在 同一個綫程裏面
            {
            }
            IModulePipeline refhello2;
            public override void OnStart()
            {
                this.refhello2 = this.GetPipeline("this/hello2");
                refhello2.TellLocalObj(new Send2());
            }
            public override void OnTell(IModulePipeline from, byte[] data)
            {
                throw new NotImplementedException();
            }
            public override void OnTellLocalObj(IModulePipeline from, object obj)
            {
                switch (obj)
                {
                    case Send1 s1:
                        Console.WriteLine("hello got send1=" + s1.str1);
                        refhello2.TellLocalObj(new Send2());
                        break;
                    case Send2 s2:
                        Console.WriteLine("hello2 got send2=" + s2.int1 + "," + s2.int2);
                        break;
                    default:
                        Console.WriteLine("unknown obj.");
                        break;
                }
            }
        }
        class Hello2 : Module
        {

            public override void OnStart()
            {
            }
            public override void OnTell(IModulePipeline from, byte[] data)
            {
                throw new NotImplementedException();
            }
            public override void OnTellLocalObj(IModulePipeline from, object obj)
            {
                switch (obj)
                {
                    case Send1 s1:
                        Console.WriteLine("hello2 got send1=" + s1.str1);
                        break;
                    case Send2 s2:
                        Console.WriteLine("hello2 got send2=" + s2.int1 + "," + s2.int2);
                        break;
                    default:
                        Console.WriteLine("unknown obj.");
                        break;
                }
            }
        }
    }
}
