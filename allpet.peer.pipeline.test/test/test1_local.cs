using AllPet.Pipeline;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace allpet.peer.pipeline.test.test
{
    class test1_local
    {
        public static async Task Test()
        {
            var system = AllPet.Pipeline.PipelineSystem.CreatePipelineSystemV1(new AllPet.Common.Logger());
            system.RegistModule("hello", new Hello());//actor习惯，连注册这个活都丢线程池，我这里简化一些
            system.RegistModule("hello2", new Hello2());//actor习惯，连注册这个活都丢线程池，我这里简化一些
            system.Start();
            var actor = system.GetPipeline(null, "this/hello");
            {
                actor.Tell(System.Text.Encoding.UTF8.GetBytes("yeah very good."));
            }
            while (true)
            {
                Console.Write("1.local>");
                var line = Console.ReadLine();
                if (line == "exit")
                {
                    //不能这样粗暴关闭的，关闭应该由actor内部发起
                    system.Dispose();
                    break;
                }
                if (line == "")
                    continue;
                actor.Tell(System.Text.Encoding.UTF8.GetBytes(line));

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
                var refhello2 = this.GetPipeline("this/hello2");
                refhello2.Tell(global::System.Text.Encoding.UTF8.GetBytes("abcde"));
            }
            public override void OnTell(IModulePipeline from, byte[] data)
            {
                if (from != null && from.system.Remote != null)//从远程投递而来
                {
                    Console.WriteLine("Hello get from:" + from.system.Remote + " // " + from.path);
                    from.Tell(global::System.Text.Encoding.UTF8.GetBytes("hihihihi"));
                }
                Console.WriteLine("Hello get info=" + global::System.Text.Encoding.UTF8.GetString(data));
                if(from==null)
                {
                    var refhello2 = this.GetPipeline("this/hello2");
                    refhello2.Tell(global::System.Text.Encoding.UTF8.GetBytes("abcde"));
                }
            }
            public override void OnTellLocalObj(IModulePipeline from, object obj)
            {
                throw new NotImplementedException();
            }
        }
        class Hello2 : Module
        {

            public override void OnStart()
            {
            }
            public override void OnTell(IModulePipeline from, byte[] data)
            {
                Console.WriteLine("Hello2:" + global::System.Text.Encoding.UTF8.GetString(data));

                from.Tell(global::System.Text.Encoding.UTF8.GetBytes("hello back."));
            }
            public override void OnTellLocalObj(IModulePipeline from, object obj)
            {
                throw new NotImplementedException();
            }
        }
    }
}
