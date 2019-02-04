using AllPet.Pipeline;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

//add this using to use Bson
using AllPet.Pipeline.NewtonsoftBson;

namespace allpet.peer.pipeline.test.test
{
    class test7_local_auto
    {


        public static async Task Test()
        {
            var system = AllPet.Pipeline.PipelineSystem.CreatePipelineSystemV1(new AllPet.Common.Logger());
            system.RegistModule("hello", new Hello());//actor习惯，连注册这个活都丢线程池，我这里简化一些
            system.RegistModule("hello2", new Hello2());//actor习惯，连注册这个活都丢线程池，我这里简化一些
            system.Start();
            var actor = system.GetPipeline(null, "this/hello");
            {
                var dict = new Newtonsoft.Json.Linq.JObject();
                dict["abc"] = 1;
                dict["aaa"] = "hello world.";
                var list = new Newtonsoft.Json.Linq.JArray("heelo", "hello", "hello");
                dict["array"] = list;
                actor.Tell(dict);
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
                var obj = new Newtonsoft.Json.Linq.JArray ("heelo","hello","hello" ); ;
                actor.TellLocalObj(obj);

            }
        }
        class Hello : Module_Bson
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
                refhello2.Tell(new Newtonsoft.Json.Linq.JValue("hi there"));
            }

            public override void OnTell(IModulePipeline from, Newtonsoft.Json.Linq.JToken json)
            {
                Console.WriteLine("got:" + json.ToString());
                refhello2.Tell(new Newtonsoft.Json.Linq.JValue("hi there"));
            }
        }
        class Hello2 : Module_Bson
        {

            public override void OnStart()
            {
            }
            public override void OnTell(IModulePipeline from, Newtonsoft.Json.Linq.JToken json)
            {
                Console.WriteLine("got:" + json.ToString());

            }
        }
    }
}
