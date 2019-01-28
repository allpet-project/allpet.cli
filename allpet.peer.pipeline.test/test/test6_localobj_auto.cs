using AllPet.Pipeline;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

//add this using to use msgpack
using AllPet.Pipeline.MsgPack;

namespace allpet.peer.pipeline.test.test
{
    class test6_local_auto
    {


        public static async Task Test()
        {
            var system = AllPet.Pipeline.PipelineSystem.CreatePipelineSystemV1();
            system.RegistModule("hello", new Hello());//actor习惯，连注册这个活都丢线程池，我这里简化一些
            system.RegistModule("hello2", new Hello2());//actor习惯，连注册这个活都丢线程池，我这里简化一些
            system.Start();
            var actor = system.GetPipeline(null, "this/hello");
            {
                var dict = new MsgPack.MessagePackObjectDictionary();
                dict["abc"] = 1;
                dict["aaa"] = "hello world.";
                var list = new MsgPack.MessagePackObject[] { "heelo","hello","hello" };
                dict["array"] = list;

                var obj = new MsgPack.MessagePackObject(dict);
                actor.Tell(obj);
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
                MsgPack.MessagePackObject obj = new MsgPack.MessagePackObject[] { "heelo","hello","hello" }; ;
                actor.TellLocalObj(obj);

            }
        }
        class Hello : Module_MsgPack
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
                refhello2.Tell(new MsgPack.MessagePackObject("hi there"));
            }

            public override void OnTell(IModulePipeline from, MsgPack.MessagePackObject? obj)
            {
                Console.WriteLine("got:" + obj.ToString());
                refhello2.Tell(new MsgPack.MessagePackObject("hi there"));
            }
        }
        class Hello2 : Module_MsgPack
        {

            public override void OnStart()
            {
            }
            public override void OnTell(IModulePipeline from, MsgPack.MessagePackObject? obj)
            {
                Console.WriteLine("got:" + obj.ToString());

            }
        }
    }
}
